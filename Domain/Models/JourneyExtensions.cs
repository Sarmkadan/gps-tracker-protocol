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
    public static double TotalDistanceKm(this Journey journey)
    {
        if (journey is null)
            throw new ArgumentNullException(nameof(journey));

        return journey.GetTotalDistance();
    }

    /// <summary>
    /// Returns the average speed of the journey in kilometers per hour.
    /// </summary>
    public static double AverageSpeedKmh(this Journey journey)
    {
        if (journey is null)
            throw new ArgumentNullException(nameof(journey));

        return journey.GetAverageSpeed();
    }

    /// <summary>
    /// Returns the duration of the journey as a <see cref="TimeSpan"/>.
    /// </summary>
    public static TimeSpan Duration(this Journey journey)
    {
        if (journey is null)
            throw new ArgumentNullException(nameof(journey));

        return journey.GetDuration();
    }
}
