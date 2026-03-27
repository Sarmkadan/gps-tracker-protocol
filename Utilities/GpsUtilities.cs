#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        if (!IsValidCoordinate(lat1, lon1) || !IsValidCoordinate(lat2, lon2))
            return 0;

        var lat1Rad = lat1 * DegreesToRadians;
        var lon1Rad = lon1 * DegreesToRadians;
        var lat2Rad = lat2 * DegreesToRadians;
        var lon2Rad = lon2 * DegreesToRadians;

        var dLat = lat2Rad - lat1Rad;
        var dLon = lon2Rad - lon1Rad;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));
        return EarthRadiusKm * c;
    }

    /// <summary>
    /// Calculates bearing (azimuth) from one coordinate to another.
    /// Returns degrees from 0-360 where 0 is North.
    /// </summary>
    public static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        if (!IsValidCoordinate(lat1, lon1) || !IsValidCoordinate(lat2, lon2))
            return 0;

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
    public static bool IsValidCoordinate(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 &&
               longitude >= -180 && longitude <= 180;
    }

    /// <summary>
    /// Checks if coordinate is within bounding box.
    /// </summary>
    public static bool IsWithinBounds(double latitude, double longitude,
        double minLat, double maxLat, double minLon, double maxLon)
    {
        return latitude >= minLat && latitude <= maxLat &&
               longitude >= minLon && longitude <= maxLon;
    }

    /// <summary>
    /// Converts degrees/minutes/seconds to decimal degrees.
    /// Input format: DDMM.MMMM or DDDMM.MMMM
    /// </summary>
    public static double DmsToDecimal(double dms, string direction)
    {
        var degrees = Math.Floor(dms / 100);
        var minutes = dms - (degrees * 100);
        var decimal_degrees = degrees + (minutes / 60);

        if (direction == "S" || direction == "W")
            decimal_degrees = -decimal_degrees;

        return decimal_degrees;
    }

    /// <summary>
    /// Converts decimal degrees to degrees/minutes/seconds format.
    /// </summary>
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
    public static double KnotsToKmh(double knots)
    {
        return knots * 1.852;
    }

    /// <summary>
    /// Converts speed in kilometers per hour to knots.
    /// </summary>
    public static double KmhToKnots(double kmh)
    {
        return kmh / 1.852;
    }

    /// <summary>
    /// Converts speed in kilometers per hour to meters per second.
    /// </summary>
    public static double KmhToMs(double kmh)
    {
        return kmh / 3.6;
    }

    /// <summary>
    /// Calculates approximate zoom level for bounding box on map.
    /// </summary>
    public static int CalculateZoomLevel(double minLat, double maxLat, double minLon, double maxLon)
    {
        var latDiff = maxLat - minLat;
        var lonDiff = maxLon - minLon;
        var maxDiff = Math.Max(latDiff, lonDiff);

        if (maxDiff == 0) return 18;

        return (int)Math.Ceiling(Math.Log(360 / maxDiff) / Math.Log(2));
    }

    /// <summary>
    /// Calculates center point of bounding box.
    /// </summary>
    public static (double latitude, double longitude) GetBoundingBoxCenter(
        double minLat, double maxLat, double minLon, double maxLon)
    {
        return ((minLat + maxLat) / 2, (minLon + maxLon) / 2);
    }
}
