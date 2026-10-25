using System;

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Extension methods for <see cref="Journey"/> providing convenient metric calculations.
/// </summary>
public static class JourneyExtensions
{
    /// <summary>
    /// Returns the total distance of the journey in kilometers.
    /// </summary>
    /// <param name="journey">The journey to measure. Must not be <c>null</c>.</param>
    /// <returns>The total distance traveled in kilometers, or 0 if the journey has fewer than two waypoints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="journey"/> is <c>null</c>.</exception>
    public static double TotalDistanceKm(this Journey journey)
    {
        ArgumentNullException.ThrowIfNull(journey);

        return journey.GetTotalDistance();
    }

    /// <summary>
    /// Returns the average speed of the journey in kilometers per hour.
    /// </summary>
    /// <param name="journey">The journey to measure. Must not be <c>null</c>.</param>
    /// <returns>The mean of the recorded waypoint speeds in kilometers per hour, or 0 if the journey has fewer than two waypoints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="journey"/> is <c>null</c>.</exception>
    public static double AverageSpeedKmh(this Journey journey)
    {
        ArgumentNullException.ThrowIfNull(journey);

        return journey.GetAverageSpeed();
    }

    /// <summary>
    /// Returns the duration of the journey as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="journey">The journey to measure. Must not be <c>null</c>.</param>
    /// <returns>The time between the journey's start and end. For ongoing journeys, the elapsed time since the start is returned.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="journey"/> is <c>null</c>.</exception>
    public static TimeSpan Duration(this Journey journey)
    {
        ArgumentNullException.ThrowIfNull(journey);

        return journey.GetDuration();
    }
}
