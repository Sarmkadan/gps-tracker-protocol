#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;

namespace GpsTrackerProtocol.Tests;

/// <summary>
/// Provides extension methods for testing GPS utility functionality.
/// </summary>
public static class GpsUtilitiesTestsExtensions
{
    /// <summary>
    /// Creates a bounding box with the given center and delta values.
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter for extension method)</param>
    /// <param name="centerLat">Center latitude in degrees</param>
    /// <param name="centerLon">Center longitude in degrees</param>
    /// <param name="deltaLat">Delta latitude in degrees (half the height of the bounding box)</param>
    /// <param name="deltaLon">Delta longitude in degrees (half the width of the bounding box)</param>
    /// <returns>Tuple containing (minLat, maxLat, minLon, maxLon) representing the bounding box coordinates</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if delta values are negative</exception>
    public static (double minLat, double maxLat, double minLon, double maxLon) CreateBoundingBox(
        this GpsUtilitiesTests _,
        double centerLat,
        double centerLon,
        double deltaLat = 0.5,
        double deltaLon = 0.5)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deltaLat, nameof(deltaLat));
        ArgumentOutOfRangeException.ThrowIfNegative(deltaLon, nameof(deltaLon));

        return (
            centerLat - deltaLat,
            centerLat + deltaLat,
            centerLon - deltaLon,
            centerLon + deltaLon
        );
    }

    /// <summary>
    /// Asserts that two coordinates are approximately equal within a specified tolerance.
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter for extension method)</param>
    /// <param name="actualLat">Actual latitude value to assert</param>
    /// <param name="actualLon">Actual longitude value to assert</param>
    /// <param name="expectedLat">Expected latitude value</param>
    /// <param name="expectedLon">Expected longitude value</param>
    /// <param name="tolerance">Tolerance in degrees for comparison (default: 0.0001)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if tolerance is negative</exception>
    public static void ShouldBeApproximately(
        this GpsUtilitiesTests _,
        double actualLat,
        double actualLon,
        double expectedLat,
        double expectedLon,
        double tolerance = 0.0001)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tolerance, nameof(tolerance));

        actualLat.Should().BeApproximately(expectedLat, tolerance);
        actualLon.Should().BeApproximately(expectedLon, tolerance);
    }

    /// <summary>
    /// Creates a coordinate pair from latitude and longitude values.
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter for extension method)</param>
    /// <param name="lat">Latitude value in degrees</param>
    /// <param name="lon">Longitude value in degrees</param>
    /// <returns>Tuple containing (latitude, longitude) coordinates</returns>
    public static (double lat, double lon) CreateCoordinate(
        this GpsUtilitiesTests _,
        double lat,
        double lon)
    {
        return (lat, lon);
    }

    /// <summary>
    /// Calculates the midpoint between two coordinates using simple arithmetic mean.
    /// <para>Note: This is an approximation suitable for small distances where Earth's curvature can be ignored.</para>
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter for extension method)</param>
    /// <param name="lat1">First latitude in degrees</param>
    /// <param name="lon1">First longitude in degrees</param>
    /// <param name="lat2">Second latitude in degrees</param>
    /// <param name="lon2">Second longitude in degrees</param>
    /// <returns>Tuple containing (midpointLat, midpointLon) representing the midpoint coordinates</returns>
    public static (double midpointLat, double midpointLon) CalculateMidpoint(
        this GpsUtilitiesTests _,
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        var midpointLat = (lat1 + lat2) / 2;
        var midpointLon = (lon1 + lon2) / 2;

        return (midpointLat, midpointLon);
    }

    /// <summary>
    /// Validates that a coordinate is approximately valid within standard GPS bounds.
    /// </summary>
    /// <param name="_">The test fixture instance (unused parameter for extension method)</param>
    /// <param name="lat">Latitude value in degrees to validate</param>
    /// <param name="lon">Longitude value in degrees to validate</param>
    /// <param name="tolerance">Additional tolerance in degrees for floating-point precision (default: 0.0001)</param>
    /// <returns>True if the coordinate is within valid GPS bounds; otherwise false</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if tolerance is negative</exception>
    public static bool IsApproximatelyValidCoordinate(
        this GpsUtilitiesTests _,
        double lat,
        double lon,
        double tolerance = 0.0001)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tolerance, nameof(tolerance));

        // Check if within bounds with small tolerance for floating point precision
        var isValid = lat >= -90 - tolerance && lat <= 90 + tolerance &&
                      lon >= -180 - tolerance && lon <= 180 + tolerance;

        return isValid;
    }
}
