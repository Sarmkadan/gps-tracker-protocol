#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Utilities;

/// <summary>
/// Records sequential GPS positions per device, forming tracks (breadcrumb trails).
/// Provides distance calculation, track segmentation by time gaps, and export.
/// </summary>
public class TrackRecorder
{
    private readonly ConcurrentDictionary<string, List<LocationData>> _tracks = new();
    private readonly ILogger<TrackRecorder> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TrackRecorder"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public TrackRecorder(ILogger<TrackRecorder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Appends a position to the device's track.
    /// Duplicate timestamps are silently ignored.
    /// </summary>
    /// <param name="deviceId">Identifier of the device.</param>
    /// <param name="location">The position to record.</param>
    public void Record(string deviceId, LocationData location)
    {
        var track = _tracks.GetOrAdd(deviceId, _ => new List<LocationData>());
        lock (track)
        {
            if (track.Count > 0 && track[^1].Timestamp == location.Timestamp)
                return;
            track.Add(location);
        }
        _logger.LogDebug("Recorded position for {DeviceId}: {Lat},{Lon}",
            deviceId, location.Latitude, location.Longitude);
    }

    /// <summary>
    /// Returns all recorded positions for a device, ordered chronologically.
    /// </summary>
    /// <param name="deviceId">Identifier of the device.</param>
    /// <returns>Ordered list of positions, empty if no data exists.</returns>
    public IReadOnlyList<LocationData> GetTrack(string deviceId)
    {
        if (_tracks.TryGetValue(deviceId, out var track))
        {
            lock (track) { return track.ToList().AsReadOnly(); }
        }
        return Array.Empty<LocationData>();
    }

    /// <summary>
    /// Calculates the total distance in kilometers of a device's recorded track
    /// using the Haversine formula between consecutive points.
    /// </summary>
    /// <param name="deviceId">Identifier of the device.</param>
    /// <returns>Total distance in kilometers, or 0 if fewer than 2 points.</returns>
    public double CalculateTotalDistanceKm(string deviceId)
    {
        var points = GetTrack(deviceId);
        if (points.Count < 2) return 0;

        double total = 0;
        for (int i = 1; i < points.Count; i++)
        {
            total += GpsUtilities.CalculateDistanceKm(
                points[i - 1].Latitude, points[i - 1].Longitude,
                points[i].Latitude, points[i].Longitude);
        }
        return total;
    }

    /// <summary>
    /// Splits a device's track into segments separated by gaps longer than the given threshold.
    /// Useful for identifying individual trips within a continuous recording session.
    /// </summary>
    /// <param name="deviceId">Identifier of the device.</param>
    /// <param name="gapThreshold">Minimum silence duration to start a new segment.</param>
    /// <returns>List of track segments, each being a list of consecutive positions.</returns>
    public IReadOnlyList<IReadOnlyList<LocationData>> SegmentByTimeGap(string deviceId, TimeSpan gapThreshold)
    {
        var points = GetTrack(deviceId);
        if (points.Count == 0) return Array.Empty<IReadOnlyList<LocationData>>();

        var segments = new List<IReadOnlyList<LocationData>>();
        var current = new List<LocationData> { points[0] };

        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].Timestamp - points[i - 1].Timestamp > gapThreshold)
            {
                segments.Add(current.AsReadOnly());
                current = new List<LocationData>();
            }
            current.Add(points[i]);
        }

        if (current.Count > 0)
            segments.Add(current.AsReadOnly());

        return segments;
    }

    /// <summary>
    /// Clears the recorded track for a device.
    /// </summary>
    /// <param name="deviceId">Identifier of the device.</param>
    public void ClearTrack(string deviceId)
    {
        _tracks.TryRemove(deviceId, out _);
        _logger.LogInformation("Track cleared for device {DeviceId}", deviceId);
    }
}
