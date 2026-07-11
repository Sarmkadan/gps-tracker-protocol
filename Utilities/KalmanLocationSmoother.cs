using System.Collections.Concurrent;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Smooths noisy GPS fixes with a simple per-device Kalman filter and rejects physically impossible jumps.
/// Thread-safe: internal state is kept in a ConcurrentDictionary keyed by DeviceId.
/// </summary>
public class KalmanLocationSmoother
{
    private readonly ConcurrentDictionary<string, DeviceState> _states = new();

    /// <summary>Process noise: expected device speed in meters/second used to grow variance over time. Default 3.</summary>
    public double ProcessNoiseMetersPerSecond { get; set; } = 3.0;

    /// <summary>Fixes implying a speed above this (km/h) versus the previous accepted fix are rejected as outliers. Default 300.</summary>
    public double MaxPlausibleSpeedKmh { get; set; } = 300.0;

    /// <summary>Accuracy (meters) assumed when a fix reports Accuracy <= 0. Default 15.</summary>
    public double DefaultAccuracyMeters { get; set; } = 15.0;

    /// <summary>Applies the filter. First fix per device initializes state and is returned unchanged. Returns a NEW LocationData with smoothed Latitude/Longitude (other fields copied) and ExtendedData["kalman.rawLat"]/["kalman.rawLon"] set to the originals. Returns null if the fix is rejected as an outlier.</summary>
    public LocationData? Smooth(LocationData fix)
    {
        if (fix == null)
        {
            return null;
        }

        var deviceId = fix.DeviceId;
        var state = _states.GetOrAdd(deviceId, _ => new DeviceState());

        // Use reported accuracy or default
        var accuracy = fix.Accuracy > 0 ? fix.Accuracy : DefaultAccuracyMeters;

        // Initialize state on first fix
        if (!state.HasState)
        {
            state.Latitude = fix.Latitude;
            state.Longitude = fix.Longitude;
            state.LatVariance = accuracy * accuracy;
            state.LonVariance = accuracy * accuracy;
            state.LastTimestamp = fix.Timestamp;

            var result = CopyLocationData(fix);
            result.ExtendedData["kalman.rawLat"] = fix.Latitude;
            result.ExtendedData["kalman.rawLon"] = fix.Longitude;
            return result;
        }

        // Calculate time delta in seconds
        var timeDelta = (fix.Timestamp - state.LastTimestamp).TotalSeconds;
        state.LastTimestamp = fix.Timestamp;

        // Don't process if time delta is negative or zero
        if (timeDelta <= 0)
        {
            var result = CopyLocationData(fix);
            result.ExtendedData["kalman.rawLat"] = fix.Latitude;
            result.ExtendedData["kalman.rawLon"] = fix.Longitude;
            return result;
        }

        // Predict step: grow variance based on process noise and time
        var processNoiseSquared = ProcessNoiseMetersPerSecond * ProcessNoiseMetersPerSecond;
        state.LatVariance += timeDelta * processNoiseSquared;
        state.LonVariance += timeDelta * processNoiseSquared;

        // Calculate gain and update state for latitude
        var latGain = state.LatVariance / (state.LatVariance + accuracy * accuracy);
        state.Latitude += latGain * (fix.Latitude - state.Latitude);
        state.LatVariance *= (1 - latGain);

        // Calculate gain and update state for longitude
        var lonGain = state.LonVariance / (state.LonVariance + accuracy * accuracy);
        state.Longitude += lonGain * (fix.Longitude - state.Longitude);
        state.LonVariance *= (1 - lonGain);

        // Check for physically impossible jumps
        var distanceMoved = DistanceMeters(
            state.Latitude, state.Longitude,
            fix.Latitude, fix.Longitude);
        var maxDistance = MaxPlausibleSpeedKmh * 1000.0 / 3600.0 * timeDelta;

        if (distanceMoved > maxDistance)
        {
            // Reject as outlier
            return null;
        }

        // Return smoothed location with raw values in ExtendedData
        var smoothed = new LocationData
        {
            Id = fix.Id,
            DeviceId = fix.DeviceId,
            Latitude = state.Latitude,
            Longitude = state.Longitude,
            Altitude = fix.Altitude,
            Speed = fix.Speed,
            Bearing = fix.Bearing,
            Timestamp = fix.Timestamp,
            Accuracy = fix.Accuracy,
            SatelliteCount = fix.SatelliteCount,
            Protocol = fix.Protocol,
            ExtendedData = new Dictionary<string, object>(fix.ExtendedData)
        };

        smoothed.ExtendedData["kalman.rawLat"] = fix.Latitude;
        smoothed.ExtendedData["kalman.rawLon"] = fix.Longitude;

        return smoothed;
    }

    /// <summary>Clears filter state for one device (e.g. after a long offline gap).</summary>
    public void Reset(string deviceId)
    {
        if (deviceId != null)
        {
            _states.TryRemove(deviceId, out _);
        }
    }

    /// <summary>Clears all per-device state.</summary>
    public void ResetAll()
    {
        _states.Clear();
    }

    /// <summary>Great-circle distance in meters between two coordinates (haversine). Public for reuse and testability.</summary>
    public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in meters
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Creates a deep copy of a LocationData instance, including ExtendedData dictionary.
    /// </summary>
    /// <param name="original">The original location data to copy.</param>
    /// <returns>A new LocationData instance with the same values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="original"/> is null.</exception>
    public static LocationData CopyLocationData(LocationData original)
    {
        ArgumentNullException.ThrowIfNull(original);

        return new LocationData
        {
            Id = original.Id,
            DeviceId = original.DeviceId,
            Latitude = original.Latitude,
            Longitude = original.Longitude,
            Altitude = original.Altitude,
            Speed = original.Speed,
            Bearing = original.Bearing,
            Timestamp = original.Timestamp,
            Accuracy = original.Accuracy,
            SatelliteCount = original.SatelliteCount,
            Protocol = original.Protocol,
            ExtendedData = new Dictionary<string, object>(original.ExtendedData)
        };
    }

    private class DeviceState
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double LatVariance { get; set; }
        public double LonVariance { get; set; }
        public DateTime LastTimestamp { get; set; }
        public bool HasState => true; // Always true after initialization
    }
}