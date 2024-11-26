using System;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Extension methods for <see cref="KalmanLocationSmoother"/> that provide additional utility functions
/// for working with smoothed GPS data and batch processing.
/// </summary>
public static class KalmanLocationSmootherExtensions
{
    private const double MinTimeDeltaSecondsForSpeed = 0.1; // Minimum 100ms for meaningful speed calculation

    /// <summary>
    /// Calculates the smoothed speed between two consecutive smoothed locations in km/h.
    /// Returns null if either location is null, if the time delta is too small for meaningful calculation,
    /// or if the distance calculation fails.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance.</param>
    /// <param name="previous">The previous smoothed location.</param>
    /// <param name="current">The current smoothed location.</param>
    /// <returns>The speed in km/h, or null if calculation is not possible.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="smoother"/>, <paramref name="previous"/>, or <paramref name="current"/> is null.</exception>
    public static double? GetSmoothedSpeedKmh(this KalmanLocationSmoother smoother, LocationData? previous, LocationData? current)
    {
        ArgumentNullException.ThrowIfNull(smoother);
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        if (previous.Timestamp == current.Timestamp)
        {
            return null; // Same timestamp, cannot calculate speed
        }

        var timeDeltaSeconds = (current.Timestamp - previous.Timestamp).TotalSeconds;
        if (timeDeltaSeconds <= MinTimeDeltaSecondsForSpeed)
        {
            return null; // Time delta too small for meaningful speed calculation
        }

        var distanceMeters = KalmanLocationSmoother.DistanceMeters(
            previous.Latitude,
            previous.Longitude,
            current.Latitude,
            current.Longitude
        );

        if (distanceMeters <= 0)
        {
            return null; // No movement detected
        }

        var speedKmh = (distanceMeters / 1000.0) / (timeDeltaSeconds / 3600.0);
        return speedKmh;
    }

    /// <summary>
    /// Batch smooths an enumerable of location data, returning only successfully smoothed locations.
    /// Useful for processing historical data or batches of fixes.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance.</param>
    /// <param name="fixes">The collection of location data to smooth.</param>
    /// <returns>An enumerable of successfully smoothed locations.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="smoother"/> or <paramref name="fixes"/> is null.</exception>
    public static IEnumerable<LocationData> SmoothBatch(this KalmanLocationSmoother smoother, IEnumerable<LocationData> fixes)
    {
        ArgumentNullException.ThrowIfNull(smoother);
        ArgumentNullException.ThrowIfNull(fixes);

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
    /// Gets the smoothed location if available, or returns a copy of the original location unchanged.
    /// Useful for fallback scenarios where you want to use smoothed data when available,
    /// but fall back to raw data when smoothing is not possible.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance.</param>
    /// <param name="fix">The location data to process.</param>
    /// <returns>Smoothed location if successful, otherwise a copy of the original location.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="smoother"/> or <paramref name="fix"/> is null.</exception>
    public static LocationData SmoothOrFallback(this KalmanLocationSmoother smoother, LocationData fix)
    {
        ArgumentNullException.ThrowIfNull(smoother);
        ArgumentNullException.ThrowIfNull(fix);

        return smoother.Smooth(fix) ?? KalmanLocationSmoother.CopyLocationData(fix);
    }

    /// <summary>
    /// Calculates the average speed across a batch of smoothed locations in km/h.
    /// Returns null if there are fewer than 2 locations, if <paramref name="locations"/> is null,
    /// or if speed calculation is not possible for any location pair.
    /// </summary>
    /// <param name="smoother">The KalmanLocationSmoother instance.</param>
    /// <param name="locations">The collection of location data.</param>
    /// <returns>The average speed in km/h, or null if calculation is not possible.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="smoother"/> or <paramref name="locations"/> is null.</exception>
    public static double? GetAverageSpeedKmh(this KalmanLocationSmoother smoother, IEnumerable<LocationData> locations)
    {
        ArgumentNullException.ThrowIfNull(smoother);
        ArgumentNullException.ThrowIfNull(locations);

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
}