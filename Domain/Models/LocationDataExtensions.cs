#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension methods for LocationData model.
// =============================================================================

using System;

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Extension methods providing geographic calculations for <see cref="LocationData"/>.
/// </summary>
public static class LocationDataExtensions
{
    private const double EarthRadiusMeters = 6_371_000.0; // mean Earth radius in meters
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;

    /// <summary>
    /// Calculates the great‑circle distance between two location points using the Haversine formula.
    /// The result is expressed in meters.
    /// </summary>
    /// <param name="source">The source location.</param>
    /// <param name="other">The destination location.</param>
    /// <returns>Distance in meters. Returns 0 if either coordinate is invalid.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="source"/> or <paramref name="other"/> is null.</exception>
    public static double DistanceTo(this LocationData source, LocationData other)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (other is null) throw new ArgumentNullException(nameof(other));

        // Validate coordinates (reuse the same logic as GpsUtilities.IsValidCoordinate)
        if (!IsValidCoordinate(source.Latitude, source.Longitude) ||
            !IsValidCoordinate(other.Latitude, other.Longitude))
        {
            return 0;
        }

        var lat1Rad = source.Latitude * DegreesToRadians;
        var lon1Rad = source.Longitude * DegreesToRadians;
        var lat2Rad = other.Latitude * DegreesToRadians;
        var lon2Rad = other.Longitude * DegreesToRadians;

        var dLat = lat2Rad - lat1Rad;
        var dLon = lon2Rad - lon1Rad;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        // Clamp to avoid rounding errors that could push 'a' outside [0,1]
        a = Math.Max(0.0, Math.Min(1.0, a));

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// Calculates the initial bearing (forward azimuth) from the source location to the destination.
    /// The bearing is expressed in degrees clockwise from true north (0‑360).
    /// </summary>
    /// <param name="source">The source location.</param>
    /// <param name="other">The destination location.</param>
    /// <returns>Bearing in degrees. Returns 0 if either coordinate is invalid.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="source"/> or <paramref name="other"/> is null.</exception>
    public static double BearingTo(this LocationData source, LocationData other)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (other is null) throw new ArgumentNullException(nameof(other));

        if (!IsValidCoordinate(source.Latitude, source.Longitude) ||
            !IsValidCoordinate(other.Latitude, other.Longitude))
        {
            return 0;
        }

        var lat1Rad = source.Latitude * DegreesToRadians;
        var lon1Rad = source.Longitude * DegreesToRadians;
        var lat2Rad = other.Latitude * DegreesToRadians;
        var lon2Rad = other.Longitude * DegreesToRadians;

        var dLon = lon2Rad - lon1Rad;

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearing = Math.Atan2(y, x) * RadiansToDegrees;
        return (bearing + 360) % 360;
    }

    /// <summary>
    /// Determines whether the <paramref name="location"/> lies within a given radius from a centre point.
    /// </summary>
    /// <param name="location">The location to test.</param>
    /// <param name="center">The centre point.</param>
    /// <param name="radiusMeters">Radius in meters.</param>
    /// <returns>True if <paramref name="location"/> is within <paramref name="radiusMeters"/> of <paramref name="center"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="location"/> or <paramref name="center"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="radiusMeters"/> is negative.</exception>
    public static bool IsWithinRadius(this LocationData location, LocationData center, double radiusMeters)
    {
        if (location is null) throw new ArgumentNullException(nameof(location));
        if (center is null) throw new ArgumentNullException(nameof(center));
        if (radiusMeters < 0) throw new ArgumentOutOfRangeException(nameof(radiusMeters), "Radius must be non‑negative.");

        return location.DistanceTo(center) <= radiusMeters;
    }

    // Helper to validate latitude/longitude ranges.
    private static bool IsValidCoordinate(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 &&
               longitude >= -180 && longitude <= 180;
    }
}
