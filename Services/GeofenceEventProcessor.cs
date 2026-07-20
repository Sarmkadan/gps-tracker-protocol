#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    // DeviceId -> set of geofence ids currently inside
    private readonly Dictionary<string, HashSet<string>> _deviceState = new();

    // DeviceId -> webhook URL
    private readonly Dictionary<string, string> _webhookSubscriptions = new();

    // (DeviceId, GeofenceId) -> entry timestamp
    private readonly Dictionary<(string DeviceId, string GeofenceId), DateTime> _entryTimestamps = new();

    // Tracks which dwell events have already been emitted to avoid duplicates
    private readonly HashSet<(string DeviceId, string GeofenceId)> _dwellEmitted = new();

    private readonly object _lock = new();

    // Configurable dwell threshold (default 5 minutes)
    private readonly TimeSpan _dwellThreshold;

    /// <summary>Initialises a new <see cref="GeofenceEventProcessor"/> with required dependencies.</summary>
    /// <param name="geofenceService">Service providing geofence data.</param>
    /// <param name="webhookClient">Client used to POST webhook payloads.</param>
    /// <param name="eventPublisher">Publishes domain events.</param>
    /// <param name="notificationService">Sends out‑of‑band notifications.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="dwellThreshold">
    /// Optional dwell threshold. If <c>TimeSpan.Zero</c> (default), a 5‑minute threshold is used.
    /// </param>
    public GeofenceEventProcessor(
        IGeofenceService geofenceService,
        IWebhookClient webhookClient,
        IEventPublisher eventPublisher,
        INotificationService notificationService,
        ILogger<GeofenceEventProcessor> logger,
        TimeSpan dwellThreshold = default)
    {
        _geofenceService = geofenceService;
        _webhookClient = webhookClient;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;
        _logger = logger;

        _dwellThreshold = dwellThreshold == TimeSpan.Zero ? TimeSpan.FromMinutes(5) : dwellThreshold;
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
        List<(string GeofenceId, TimeSpan Dwell)> dwellEvents = new();
        string? webhookUrl;

        lock (_lock)
        {
            _deviceState.TryGetValue(location.DeviceId, out var previouslyInside);
            previouslyInside ??= [];

            transitions = new List<(string GeofenceId, bool Entered, TimeSpan Dwell)>();

            // Detect entries
            foreach (var id in currentlyInside.Except(previouslyInside))
            {
                _entryTimestamps[(location.DeviceId, id)] = location.Timestamp;
                transitions.Add((id, true, TimeSpan.Zero));
            }

            // Detect exits
            foreach (var id in previouslyInside.Except(currentlyInside))
            {
                var dwell = _entryTimestamps.TryGetValue((location.DeviceId, id), out var entryTime)
                    ? location.Timestamp - entryTime
                    : TimeSpan.Zero;
                _entryTimestamps.Remove((location.DeviceId, id));
                transitions.Add((id, false, dwell));
            }

            // Detect dwell exceedance
            foreach (var id in currentlyInside)
            {
                if (_entryTimestamps.TryGetValue((location.DeviceId, id), out var entryTime))
                {
                    var dwell = location.Timestamp - entryTime;
                    if (dwell >= _dwellThreshold && !_dwellEmitted.Contains((location.DeviceId, id)))
                    {
                        _dwellEmitted.Add((location.DeviceId, id));
                        dwellEvents.Add((id, dwell));
                    }
                }
            }

            _deviceState[location.DeviceId] = currentlyInside;
            _webhookSubscriptions.TryGetValue(location.DeviceId, out webhookUrl);
        }

        // Process entry / exit events
        foreach (var (geofenceId, entered, dwell) in transitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entered)
                await OnEnteredAsync(location, geofenceId, webhookUrl).ConfigureAwait(false);
            else
                await OnExitedAsync(location, geofenceId, dwell, webhookUrl).ConfigureAwait(false);
        }

        // Process dwell events
        foreach (var (geofenceId, dwell) in dwellEvents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await OnDwellAsync(location, geofenceId, dwell, webhookUrl).ConfigureAwait(false);
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

        await _eventPublisher.PublishAsync(@event).ConfigureAwait(false);
        await _notificationService.SendGeofenceAlertAsync(location.DeviceId, location.Latitude, location.Longitude).ConfigureAwait(false);

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

        await _eventPublisher.PublishAsync(@event).ConfigureAwait(false);

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

    private async Task OnDwellAsync(LocationData location, string geofenceId, TimeSpan dwell, string? webhookUrl)
    {
        _logger.LogInformation("Device {DeviceId} dwell exceeded in geofence {GeofenceId} (duration {Dwell})",
            location.DeviceId, geofenceId, dwell);

        var @event = new GeofenceDwellEvent
        {
            AggregateId   = location.DeviceId,
            DeviceId      = location.DeviceId,
            GeofenceId    = geofenceId,
            DwellDuration = dwell,
            DwellThreshold = _dwellThreshold
        };

        await _eventPublisher.PublishAsync(@event).ConfigureAwait(false);

        // Optional: send webhook for dwell events (not required by spec, but kept for symmetry)
        if (!string.IsNullOrWhiteSpace(webhookUrl))
        {
            await _webhookClient.SendGeofenceEventAsync(webhookUrl, new GeofenceWebhookPayload
            {
                EventType    = "geofence_dwell",
                DeviceId     = location.DeviceId,
                GeofenceId   = geofenceId,
                Latitude     = location.Latitude,
                Longitude    = location.Longitude,
                Speed        = location.Speed,
                DwellSeconds = (long)dwell.TotalSeconds,
                Timestamp    = @event.Timestamp.ToString("o")
            });
        }
    }
}
