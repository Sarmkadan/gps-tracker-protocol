#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// GPS coordinate calculations and conversions.
/// Calculates distances, bearings, and validates coordinate ranges.
/// </summary>
public static class GpsUtilities
{
    private const double EarthRadiusKm = 6371.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;
    private const double DegreesToRadians = Math.PI / 180.0;

    /// <summary>
    /// Calculates great-circle distance between two coordinates using Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of first point in decimal degrees.</param>
    /// <param name="lon1">Longitude of first point in decimal degrees.</param>
    /// <param name="lat2">Latitude of second point in decimal degrees.</param>
    /// <param name="lon2">Longitude of second point in decimal degrees.</param>
    /// <returns>Distance in kilometers, or 0 if coordinates are invalid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are NaN or infinite.</exception>
    public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        // Validate input coordinates
        if (double.IsNaN(lat1) || double.IsNaN(lon1) || double.IsNaN(lat2) || double.IsNaN(lon2))
        {
            throw new ArgumentOutOfRangeException(nameof(lat1), "Latitude and longitude cannot be NaN.");
        }

        if (double.IsInfinity(lat1) || double.IsInfinity(lon1) || double.IsInfinity(lat2) || double.IsInfinity(lon2))
        {
            throw new ArgumentOutOfRangeException(nameof(lat1), "Latitude and longitude cannot be infinite.");
        }

        if (!IsValidCoordinate(lat1, lon1) || !IsValidCoordinate(lat2, lon2))
        {
            return 0;
        }

        var lat1Rad = lat1 * DegreesToRadians;
        var lon1Rad = lon1 * DegreesToRadians;
        var lat2Rad = lat2 * DegreesToRadians;
        var lon2Rad = lon2 * DegreesToRadians;

        var dLat = lat2Rad - lat1Rad;
        var dLon = lon2Rad - lon1Rad;

        // Haversine formula using atan2 for better numerical stability
        // a = sin²(Δφ/2) + cos(φ1) * cos(φ2) * sin²(Δλ/2)
        // c = 2 * atan2(√a, √(1−a))
        // d = R * c
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        // Clamp 'a' to [0, 1] to prevent NaN from floating-point rounding errors
        // This can happen when points are identical or extremely close together
        a = Math.Max(0.0, Math.Min(1.0, a));

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates bearing (azimuth) from one coordinate to another.
    /// Returns degrees from 0-360 where 0 is North.
    /// </summary>
    /// <param name="lat1">Latitude of starting point in decimal degrees.</param>
    /// <param name="lon1">Longitude of starting point in decimal degrees.</param>
    /// <param name="lat2">Latitude of target point in decimal degrees.</param>
    /// <param name="lon2">Longitude of target point in decimal degrees.</param>
    /// <returns>Bearing in degrees [0, 360), or 0 if coordinates are invalid.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are NaN or infinite.</exception>
    public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        // Validate input coordinates
        if (double.IsNaN(lat1) || double.IsNaN(lon1) || double.IsNaN(lat2) || double.IsNaN(lon2))
        {
            throw new ArgumentOutOfRangeException(nameof(lat1), "Latitude and longitude cannot be NaN.");
        }

        if (double.IsInfinity(lat1) || double.IsInfinity(lon1) || double.IsInfinity(lat2) || double.IsInfinity(lon2))
        {
            throw new ArgumentOutOfRangeException(nameof(lat1), "Latitude and longitude cannot be infinite.");
        }

        if (!IsValidCoordinate(lat1, lon1) || !IsValidCoordinate(lat2, lon2))
        {
            return 0;
        }

        var lat1Rad = lat1 * DegreesToRadians;
        var lon1Rad = lon1 * DegreesToRadians;
        var lat2Rad = lat2 * DegreesToRadians;
        var lon2Rad = lon2 * DegreesToRadians;

        var dLon = lon2Rad - lon1Rad;

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearing = Math.Atan2(y, x) * RadiansToDegrees;
        return (bearing + 360) % 360;
    }

    /// <summary>
    /// Validates that latitude is within [-90, 90] and longitude within [-180, 180].
    /// </summary>
    /// <param name="latitude">Latitude to validate.</param>
    /// <param name="longitude">Longitude to validate.</param>
    /// <returns>True if coordinates are valid, false otherwise.</returns>
    public static bool IsValidCoordinate(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 &&
               longitude >= -180 && longitude <= 180;
    }

    /// <summary>
    /// Checks if coordinate is within bounding box.
    /// </summary>
    /// <param name="latitude">Latitude to check.</param>
    /// <param name="longitude">Longitude to check.</param>
    /// <param name="minLat">Minimum latitude of bounding box.</param>
    /// <param name="maxLat">Maximum latitude of bounding box.</param>
    /// <param name="minLon">Minimum longitude of bounding box.</param>
    /// <param name="maxLon">Maximum longitude of bounding box.</param>
    /// <returns>True if point is within bounds, false otherwise.</returns>
    public static bool IsWithinBounds(double latitude, double longitude,
                                   double minLat, double maxLat,
                                   double minLon, double maxLon)
    {
        return latitude >= minLat && latitude <= maxLat &&
               longitude >= minLon && longitude <= maxLon;
    }

    /// <summary>
    /// Converts degrees/minutes/seconds to decimal degrees.
    /// Input format: DDMM.MMMM or DDDMM.MMMM
    /// </summary>
    /// <param name="dms">Degrees-minutes value (e.g., 5130.0 = 51°30.0').</param>
    /// <param name="direction">Direction indicator: 'N', 'S', 'E', or 'W'.</param>
    /// <returns>Decimal degrees.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when direction is invalid.</exception>
    public static double DmsToDecimal(double dms, string direction)
    {
        ArgumentException.ThrowIfNullOrEmpty(direction);

        var degrees = Math.Floor(dms / 100);
        var minutes = dms - (degrees * 100);
        var decimal_degrees = degrees + (minutes / 60);

        if (direction == "S" || direction == "W")
        {
            decimal_degrees = -decimal_degrees;
        }
        else if (direction != "N" && direction != "E")
        {
            throw new ArgumentOutOfRangeException(nameof(direction),
                "Direction must be 'N', 'S', 'E', or 'W'.");
        }

        return decimal_degrees;
    }

    /// <summary>
    /// Converts decimal degrees to degrees/minutes/seconds format.
    /// </summary>
    /// <param name="decimal_degrees">Decimal degrees to convert.</param>
    /// <returns>Tuple of (degrees, minutes, seconds).</returns>
    public static (int degrees, int minutes, double seconds) DecimalToDms(double decimal_degrees)
    {
        decimal_degrees = Math.Abs(decimal_degrees);
        int degrees = (int)Math.Floor(decimal_degrees);
        var minutesFull = (decimal_degrees - degrees) * 60;
        int minutes = (int)Math.Floor(minutesFull);
        double seconds = (minutesFull - minutes) * 60;

        return (degrees, minutes, seconds);
    }

    /// <summary>
    /// Converts speed in knots to kilometers per hour.
    /// </summary>
    /// <param name="knots">Speed in knots.</param>
    /// <returns>Speed in kilometers per hour.</returns>
    public static double KnotsToKmh(double knots)
    {
        return knots * 1.852;
    }

    /// <summary>
    /// Converts speed in kilometers per hour to knots.
    /// </summary>
    /// <param name="kmh">Speed in kilometers per hour.</param>
    /// <returns>Speed in knots.</returns>
    public static double KmhToKnots(double kmh)
    {
        return kmh / 1.852;
    }

    /// <summary>
    /// Converts speed in kilometers per hour to meters per second.
    /// </summary>
    /// <param name="kmh">Speed in kilometers per hour.</param>
    /// <returns>Speed in meters per second.</returns>
    public static double KmhToMs(double kmh)
    {
        return kmh / 3.6;
    }

    /// <summary>
    /// Calculates approximate zoom level for bounding box on map.
    /// </summary>
    /// <param name="minLat">Minimum latitude.</param>
    /// <param name="maxLat">Maximum latitude.</param>
    /// <param name="minLon">Minimum longitude.</param>
    /// <param name="maxLon">Maximum longitude.</param>
    /// <returns>Zoom level [0-20].</returns>
    public static int CalculateZoomLevel(double minLat, double maxLat, double minLon, double maxLon)
    {
        var latDiff = maxLat - minLat;
        var lonDiff = maxLon - minLon;
        var maxDiff = Math.Max(latDiff, lonDiff);

        if (maxDiff == 0)
        {
            return 18;
        }

        return (int)Math.Ceiling(Math.Log(360 / maxDiff) / Math.Log(2));
    }

    /// <summary>
    /// Calculates center point of bounding box.
    /// </summary>
    /// <param name="minLat">Minimum latitude.</param>
    /// <param name="maxLat">Maximum latitude.</param>
    /// <param name="minLon">Minimum longitude.</param>
    /// <param name="maxLon">Maximum longitude.</param>
    /// <returns>Tuple of (latitude, longitude) representing the center.</returns>
    public static (double latitude, double longitude) GetBoundingBoxCenter(
        double minLat, double maxLat, double minLon, double maxLon)
    {
        return ((minLat + maxLat) / 2, (minLon + maxLon) / 2);
    }
}
