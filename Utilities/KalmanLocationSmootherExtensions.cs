using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Extension methods for <see cref="KalmanLocationSmoother"/> that provide additional utility functions
/// for working with smoothed GPS data and batch processing.
/// </summary>
public static class KalmanLocationSmootherExtensions
{
    /// <summary>
    /// Calculates the smoothed speed between two consecutive smoothed locations in km/h.
    /// Returns null if either location is null or if the time delta is too small for meaningful calculation.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance</param>
    /// <param name="previous">The previous smoothed location</param>
    /// <param name="current">The current smoothed location</param>
    /// <returns>The speed in km/h, or null if calculation is not possible</returns>
    public static double? GetSmoothedSpeedKmh(this KalmanLocationSmoother smoother, LocationData? previous, LocationData? current)
    {
        if (previous == null || current == null)
        {
            return null;
        }

        if (previous.Timestamp == current.Timestamp)
        {
            return null; // Same timestamp, cannot calculate speed
        }

        var timeDeltaSeconds = (current.Timestamp - previous.Timestamp).TotalSeconds;
        if (timeDeltaSeconds <= 0.1) // Minimum 100ms for meaningful speed calculation
        {
            return null;
        }

        var distanceMeters = KalmanLocationSmoother.DistanceMeters(
            previous.Latitude,
            previous.Longitude,
            current.Latitude,
            current.Longitude
        );

        var speedKmh = (distanceMeters / 1000.0) / (timeDeltaSeconds / 3600.0);
        return speedKmh;
    }

    /// <summary>
    /// Batch smooths an enumerable of location data, returning only successfully smoothed locations.
    /// Useful for processing historical data or batches of fixes.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance</param>
    /// <param name="fixes">The collection of location data to smooth</param>
    /// <returns>An enumerable of successfully smoothed locations</returns>
    public static IEnumerable<LocationData> SmoothBatch(this KalmanLocationSmoother smoother, IEnumerable<LocationData> fixes)
    {
        if (fixes == null)
        {
            yield break;
        }

        foreach (var fix in fixes)
        {
            var smoothed = smoother.Smooth(fix);
            if (smoothed != null)
            {
                yield return smoothed;
            }
        }
    }

    /// <summary>
    /// Gets the smoothed location if available, or returns the original location unchanged.
    /// Useful for fallback scenarios where you want to use smoothed data when available,
    /// but fall back to raw data when smoothing is not possible.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance</param>
    /// <param name="fix">The location data to process</param>
    /// <returns>Smoothed location if successful, otherwise the original location with raw values in ExtendedData</returns>
    public static LocationData SmoothOrFallback(this KalmanLocationSmoother smoother, LocationData fix)
    {
        var smoothed = smoother.Smooth(fix);
        return smoothed ?? CopyLocationData(fix);
    }

    /// <summary>
    /// Calculates the average speed across a batch of smoothed locations in km/h.
    /// Returns null if there are fewer than 2 locations or if speed calculation is not possible.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance</param>
    /// <param name="locations">The collection of location data</param>
    /// <returns>The average speed in km/h, or null if calculation is not possible</returns>
    public static double? GetAverageSpeedKmh(this KalmanLocationSmoother smoother, IEnumerable<LocationData> locations)
    {
        if (locations == null)
        {
            return null;
        }

        var locationList = locations.ToList();
        if (locationList.Count < 2)
        {
            return null;
        }

        double totalSpeed = 0;
        int validPairs = 0;

        for (int i = 1; i < locationList.Count; i++)
        {
            var speed = smoother.GetSmoothedSpeedKmh(locationList[i - 1], locationList[i]);
            if (speed.HasValue)
            {
                totalSpeed += speed.Value;
                validPairs++;
            }
        }

        return validPairs > 0 ? totalSpeed / validPairs : null;
    }

    private static LocationData CopyLocationData(LocationData original)
    {
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
}