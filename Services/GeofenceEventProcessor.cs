// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;
using GpsTrackerProtocol.Integration;

/// <summary>
/// Detects geofence boundary crossings from incoming location updates and delivers
/// webhook notifications to registered device subscribers.
/// </summary>
public interface IGeofenceEventProcessor
{
    /// <summary>
    /// Evaluates a location update against all registered geofences and raises
    /// <see cref="GeofenceEnteredEvent"/> or <see cref="GeofenceExitedEvent"/>
    /// for every state transition detected since the previous report for this device.
    /// </summary>
    /// <param name="location">The new location to evaluate. Must satisfy <c>IsValid()</c>.</param>
    /// <param name="cancellationToken">Token for cooperative cancellation.</param>
    Task ProcessLocationAsync(LocationData location, CancellationToken cancellationToken = default);

    /// <summary>Associates a webhook URL with a device so geofence events are POSTed there.</summary>
    /// <param name="deviceId">The device to subscribe.</param>
    /// <param name="webhookUrl">Destination URL for <see cref="GeofenceWebhookPayload"/> payloads.</param>
    void RegisterWebhook(string deviceId, string webhookUrl);

    /// <summary>Removes the webhook subscription for the specified device.</summary>
    /// <param name="deviceId">The device to unsubscribe.</param>
    void UnregisterWebhook(string deviceId);

    /// <summary>Returns the set of geofence IDs the device is currently inside.</summary>
    /// <param name="deviceId">Device to query.</param>
    IReadOnlySet<string> GetCurrentGeofences(string deviceId);
}

/// <summary>
/// Processes location updates to detect geofence boundary crossings, then notifies
/// subscribers via the internal event bus and registered webhook endpoints.
/// Thread-safe; safe to call concurrently from multiple TCP/UDP receivers.
/// </summary>
public class GeofenceEventProcessor : IGeofenceEventProcessor
{
    private readonly IGeofenceService _geofenceService;
    private readonly IWebhookClient _webhookClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;
    private readonly ILogger<GeofenceEventProcessor> _logger;

    private readonly Dictionary<string, HashSet<string>> _deviceState = new();
    private readonly Dictionary<string, string> _webhookSubscriptions = new();
    private readonly Dictionary<(string DeviceId, string GeofenceId), DateTime> _entryTimestamps = new();
    private readonly object _lock = new();

    /// <summary>Initialises a new <see cref="GeofenceEventProcessor"/> with required dependencies.</summary>
    public GeofenceEventProcessor(
        IGeofenceService geofenceService,
        IWebhookClient webhookClient,
        IEventPublisher eventPublisher,
        INotificationService notificationService,
        ILogger<GeofenceEventProcessor> logger)
    {
        _geofenceService = geofenceService;
        _webhookClient = webhookClient;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ProcessLocationAsync(LocationData location, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(location);
        cancellationToken.ThrowIfCancellationRequested();

        if (!location.IsValid())
        {
            _logger.LogWarning("Skipping invalid location for device {DeviceId}", location.DeviceId);
            return;
        }

        var nearby = _geofenceService
            .GetNearbyGeofences(location.Latitude, location.Longitude, 5.0)
            .ToHashSet();

        var currentlyInside = nearby
            .Where(id => _geofenceService.IsInsideGeofence(id, location.Latitude, location.Longitude))
            .ToHashSet();

        List<(string GeofenceId, bool Entered, TimeSpan Dwell)> transitions;
        string? webhookUrl;

        lock (_lock)
        {
            _deviceState.TryGetValue(location.DeviceId, out var previouslyInside);
            previouslyInside ??= [];

            transitions = [];

            foreach (var id in currentlyInside.Except(previouslyInside))
            {
                _entryTimestamps[(location.DeviceId, id)] = location.Timestamp;
                transitions.Add((id, true, TimeSpan.Zero));
            }

            foreach (var id in previouslyInside.Except(currentlyInside))
            {
                var dwell = _entryTimestamps.TryGetValue((location.DeviceId, id), out var entryTime)
                    ? location.Timestamp - entryTime
                    : TimeSpan.Zero;
                _entryTimestamps.Remove((location.DeviceId, id));
                transitions.Add((id, false, dwell));
            }

            _deviceState[location.DeviceId] = currentlyInside;
            _webhookSubscriptions.TryGetValue(location.DeviceId, out webhookUrl);
        }

        foreach (var (geofenceId, entered, dwell) in transitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entered)
                await OnEnteredAsync(location, geofenceId, webhookUrl);
            else
                await OnExitedAsync(location, geofenceId, dwell, webhookUrl);
        }
    }

    /// <inheritdoc/>
    public void RegisterWebhook(string deviceId, string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(webhookUrl))
            return;

        lock (_lock)
            _webhookSubscriptions[deviceId] = webhookUrl;

        _logger.LogInformation("Webhook registered for device {DeviceId}: {Url}", deviceId, webhookUrl);
    }

    /// <inheritdoc/>
    public void UnregisterWebhook(string deviceId)
    {
        lock (_lock)
            _webhookSubscriptions.Remove(deviceId);

        _logger.LogInformation("Webhook unregistered for device {DeviceId}", deviceId);
    }

    /// <inheritdoc/>
    public IReadOnlySet<string> GetCurrentGeofences(string deviceId)
    {
        lock (_lock)
            return _deviceState.TryGetValue(deviceId, out var set) ? set : new HashSet<string>();
    }

    private async Task OnEnteredAsync(LocationData location, string geofenceId, string? webhookUrl)
    {
        _logger.LogInformation("Device {DeviceId} entered geofence {GeofenceId}",
            location.DeviceId, geofenceId);

        var @event = new GeofenceEnteredEvent
        {
            AggregateId = location.DeviceId,
            DeviceId    = location.DeviceId,
            GeofenceId  = geofenceId,
            Latitude    = location.Latitude,
            Longitude   = location.Longitude,
            Speed       = location.Speed
        };

        await _eventPublisher.PublishAsync(@event);
        await _notificationService.SendGeofenceAlertAsync(location.DeviceId, location.Latitude, location.Longitude);

        if (!string.IsNullOrWhiteSpace(webhookUrl))
        {
            await _webhookClient.SendGeofenceEventAsync(webhookUrl, new GeofenceWebhookPayload
            {
                EventType  = "geofence_entered",
                DeviceId   = location.DeviceId,
                GeofenceId = geofenceId,
                Latitude   = location.Latitude,
                Longitude  = location.Longitude,
                Speed      = location.Speed,
                Timestamp  = @event.Timestamp.ToString("o")
            });
        }
    }

    private async Task OnExitedAsync(LocationData location, string geofenceId, TimeSpan dwell, string? webhookUrl)
    {
        _logger.LogInformation("Device {DeviceId} exited geofence {GeofenceId} after {Dwell}",
            location.DeviceId, geofenceId, dwell);

        var @event = new GeofenceExitedEvent
        {
            AggregateId   = location.DeviceId,
            DeviceId      = location.DeviceId,
            GeofenceId    = geofenceId,
            Latitude      = location.Latitude,
            Longitude     = location.Longitude,
            DwellDuration = dwell
        };

        await _eventPublisher.PublishAsync(@event);

        if (!string.IsNullOrWhiteSpace(webhookUrl))
        {
            await _webhookClient.SendGeofenceEventAsync(webhookUrl, new GeofenceWebhookPayload
            {
                EventType    = "geofence_exited",
                DeviceId     = location.DeviceId,
                GeofenceId   = geofenceId,
                Latitude     = location.Latitude,
                Longitude    = location.Longitude,
                DwellSeconds = (long)dwell.TotalSeconds,
                Timestamp    = @event.Timestamp.ToString("o")
            });
        }
    }
}
