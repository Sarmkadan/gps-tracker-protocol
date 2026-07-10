#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;

namespace GpsTrackerProtocol.Tests;

public static class GpsUtilitiesTestsExtensions
{
    /// <summary>
    /// Creates a bounding box with the given center and delta values.
    /// </summary>
    /// <param name="centerLat">Center latitude</param>
    /// <param name="centerLon">Center longitude</param>
    /// <param name="deltaLat">Delta latitude (half the height)</param>
    /// <param name="deltaLon">Delta longitude (half the width)</param>
    /// <returns>Tuple of (minLat, maxLat, minLon, maxLon)</returns>
    public static (double minLat, double maxLat, double minLon, double maxLon) CreateBoundingBox(
        this GpsUtilitiesTests _,
        double centerLat,
        double centerLon,
        double deltaLat = 0.5,
        double deltaLon = 0.5)
    {
        return (
            centerLat - deltaLat,
            centerLat + deltaLat,
            centerLon - deltaLon,
            centerLon + deltaLon
        );
    }

    /// <summary>
    /// Asserts that two coordinates are approximately equal within a tolerance.
    /// </summary>
    /// <param name="actualLat">Actual latitude</param>
    /// <param name="actualLon">Actual longitude</param>
    /// <param name="expectedLat">Expected latitude</param>
    /// <param name="expectedLon">Expected longitude</param>
    /// <param name="tolerance">Tolerance in degrees</param>
    public static void ShouldBeApproximately(
        this GpsUtilitiesTests _,
        double actualLat,
        double actualLon,
        double expectedLat,
        double expectedLon,
        double tolerance = 0.0001)
    {
        actualLat.Should().BeApproximately(expectedLat, tolerance);
        actualLon.Should().BeApproximately(expectedLon, tolerance);
    }

    /// <summary>
    /// Creates a coordinate pair from latitude and longitude.
    /// </summary>
    /// <param name="lat">Latitude</param>
    /// <param name="lon">Longitude</param>
    /// <returns>Tuple of (latitude, longitude)</returns>
    public static (double lat, double lon) CreateCoordinate(
        this GpsUtilitiesTests _,
        double lat,
        double lon)
    {
        return (lat, lon);
    }

    /// <summary>
    /// Calculates the midpoint between two coordinates.
    /// </summary>
    /// <param name="lat1">First latitude</param>
    /// <param name="lon1">First longitude</param>
    /// <param name="lat2">Second latitude</param>
    /// <param name="lon2">Second longitude</param>
    /// <returns>Tuple of (midpointLat, midpointLon)</returns>
    public static (double midpointLat, double midpointLon) CalculateMidpoint(
        this GpsUtilitiesTests _,
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        // Simple midpoint calculation (approximation for small distances)
        var midpointLat = (lat1 + lat2) / 2;
        var midpointLon = (lon1 + lon2) / 2;

        return (midpointLat, midpointLon);
    }

    /// <summary>
    /// Validates that a coordinate is approximately valid (within bounds with tolerance).
    /// </summary>
    /// <param name="lat">Latitude to validate</param>
    /// <param name="lon">Longitude to validate</param>
    /// <param name="tolerance">Additional tolerance in degrees</param>
    /// <returns>True if coordinate is valid</returns>
    public static bool IsApproximatelyValidCoordinate(
        this GpsUtilitiesTests _,
        double lat,
        double lon,
        double tolerance = 0.0001)
    {
        // Check if within bounds with small tolerance for floating point precision
        var isValid = lat >= -90 - tolerance && lat <= 90 + tolerance &&
                      lon >= -180 - tolerance && lon <= 180 + tolerance;

        return isValid;
    }
}
