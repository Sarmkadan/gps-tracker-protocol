#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;

/// <summary>
/// Evaluates device positions against registered geofences and fires
/// enter/exit events. Maintains per-device state so that events are
/// emitted only on transitions, not on every position update.
/// </summary>
public class GeofenceEngine
{
    private readonly IGeofenceService _geofenceService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<GeofenceEngine> _logger;

    // Tracks which geofences each device is currently inside: deviceId -> set of geofenceIds
    private readonly ConcurrentDictionary<string, HashSet<string>> _deviceState = new();

    /// <summary>
    /// Initializes a new instance of <see cref="GeofenceEngine"/>.
    /// </summary>
    /// <param name="geofenceService">Service that stores geofence definitions.</param>
    /// <param name="eventPublisher">Publisher for geofence transition events.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public GeofenceEngine(
        IGeofenceService geofenceService,
        IEventPublisher eventPublisher,
        ILogger<GeofenceEngine> logger)
    {
        _geofenceService = geofenceService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates a new position for a device against all nearby geofences,
    /// publishing enter/exit events when the device crosses a boundary.
    /// </summary>
    /// <param name="deviceId">Identifier of the device.</param>
    /// <param name="location">The new position to evaluate.</param>
    /// <returns>List of geofence IDs whose state changed for this device.</returns>
    public IReadOnlyList<string> Evaluate(string deviceId, LocationData location)
    {
        var changed = new List<string>();
        var nearbyIds = _geofenceService
            .GetNearbyGeofences(location.Latitude, location.Longitude, searchRadiusKm: 50)
            .ToList();

        var previouslyInside = _deviceState.GetOrAdd(deviceId, _ => new HashSet<string>());
        var currentlyInside = new HashSet<string>();

        foreach (var fenceId in nearbyIds)
        {
            if (_geofenceService.IsInsideGeofence(fenceId, location.Latitude, location.Longitude))
                currentlyInside.Add(fenceId);
        }

        // Entered
        foreach (var fenceId in currentlyInside)
        {
            if (!previouslyInside.Contains(fenceId))
            {
                _logger.LogInformation("Device {DeviceId} entered geofence {FenceId}", deviceId, fenceId);
                _ = _eventPublisher.PublishAsync(new GeofenceEnteredEvent
                {
                    DeviceId = deviceId,
                    AggregateId = deviceId,
                    GeofenceId = fenceId,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                });
                changed.Add(fenceId);
            }
        }

        // Exited
        foreach (var fenceId in previouslyInside)
        {
            if (!currentlyInside.Contains(fenceId))
            {
                _logger.LogInformation("Device {DeviceId} exited geofence {FenceId}", deviceId, fenceId);
                _ = _eventPublisher.PublishAsync(new GeofenceExitedEvent
                {
                    DeviceId = deviceId,
                    AggregateId = deviceId,
                    GeofenceId = fenceId,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                });
                changed.Add(fenceId);
            }
        }

        // Update state
        lock (previouslyInside)
        {
            previouslyInside.Clear();
            foreach (var id in currentlyInside)
                previouslyInside.Add(id);
        }

        return changed;
    }

    /// <summary>
    /// Returns the set of geofence IDs a device is currently inside.
    /// </summary>
    /// <param name="deviceId">The device to query.</param>
    /// <returns>Set of geofence IDs, empty if the device has no known state.</returns>
    public IReadOnlySet<string> GetDeviceGeofences(string deviceId)
    {
        if (_deviceState.TryGetValue(deviceId, out var fences))
        {
            lock (fences) { return new HashSet<string>(fences); }
        }
        return new HashSet<string>();
    }

    /// <summary>
    /// Clears all tracked geofence state for a device (e.g. on disconnect).
    /// </summary>
    /// <param name="deviceId">The device whose state to clear.</param>
    public void ClearDeviceState(string deviceId)
    {
        _deviceState.TryRemove(deviceId, out _);
    }
}
