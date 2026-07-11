#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using Xunit;

/// <summary>
/// Tests for the GpsUtilities class.
/// </summary>
public class GpsUtilitiesTests
{
    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns 0 when the start and end coordinates are the same.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_SameCoordinates_ReturnsZero()
    {
        var distance = GpsUtilities.CalculateDistanceKm(51.5074, -0.1278, 51.5074, -0.1278);

        distance.Should().Be(0);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns approximately 111.19 km when the start and end coordinates are one degree apart.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_OneDegreeLat_ReturnsApproximatelyOneHundredEleven()
    {
        // One degree of latitude ≈ 111.19 km at the equator
        var distance = GpsUtilities.CalculateDistanceKm(0, 0, 1, 0);

        distance.Should().BeApproximately(111.19, 0.1);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns 0 when the start and end coordinates are invalid.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_InvalidCoordinates_ReturnsZero()
    {
        var distance = GpsUtilities.CalculateDistanceKm(100, 200, 0, 0);

        distance.Should().Be(0);
    }

    /// <summary>
    /// Verifies that the IsValidCoordinate method returns the expected result for various input coordinates.
    /// </summary>
    /// <param name="lat">The latitude coordinate.</param>
    /// <param name="lon">The longitude coordinate.</param>
    /// <param name="expected">The expected result.</param>
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(90, 180, true)]
    [InlineData(-90, -180, true)]
    [InlineData(91, 0, false)]
    [InlineData(0, 181, false)]
    [InlineData(-91, -181, false)]
    public void IsValidCoordinate_VariousInputs_ReturnsExpectedResult(double lat, double lon, bool expected)
    {
        GpsUtilities.IsValidCoordinate(lat, lon).Should().Be(expected);
    }

    /// <summary>
    /// Verifies that the CalculateBearing method returns 0 degrees when moving due north.
    /// </summary>
    [Fact]
    public void CalculateBearing_DueNorth_ReturnsZeroDegrees()
    {
        // Moving from equator toward north pole: bearing = 0°
        var bearing = GpsUtilities.CalculateBearing(0, 0, 1, 0);

        bearing.Should().BeApproximately(0, 0.001);
    }

    /// <summary>
    /// Verifies that the CalculateBearing method returns 90 degrees when moving due east.
    /// </summary>
    [Fact]
    public void CalculateBearing_DueEast_ReturnsNinetyDegrees()
    {
        // Moving eastward along the equator: bearing = 90°
        var bearing = GpsUtilities.CalculateBearing(0, 0, 0, 1);

        bearing.Should().BeApproximately(90, 0.01);
    }

    /// <summary>
    /// Verifies that the CalculateBearing method returns 0 degrees when the start and end coordinates are invalid.
    /// </summary>
    [Fact]
    public void CalculateBearing_InvalidCoordinates_ReturnsZero()
    {
        var bearing = GpsUtilities.CalculateBearing(200, 0, 0, 0);

        bearing.Should().Be(0);
    }

    /// <summary>
    /// Verifies that the DmsToDecimal method returns the correct decimal value for a northern latitude.
    /// </summary>
    [Fact]
    public void DmsToDecimal_NorthernLatitude_ReturnsPositiveDecimal()
    {
        // 5130.0 = 51 degrees 30 minutes → 51.5 decimal degrees
        var result = GpsUtilities.DmsToDecimal(5130.0, "N");

        result.Should().BeApproximately(51.5, 0.0001);
    }

    /// <summary>
    /// Verifies that the DmsToDecimal method returns the correct decimal value for a southern latitude.
    /// </summary>
    [Fact]
    public void DmsToDecimal_SouthernLatitude_ReturnsNegativeDecimal()
    {
        var result = GpsUtilities.DmsToDecimal(5130.0, "S");

        result.Should().BeApproximately(-51.5, 0.0001);
    }

    /// <summary>
    /// Verifies that the DmsToDecimal method returns the correct decimal value for a western longitude.
    /// </summary>
    [Fact]
    public void DmsToDecimal_WesternLongitude_ReturnsNegativeDecimal()
    {
        // 00130.0 = 1 degree 30 minutes → -1.5 decimal degrees (West)
        var result = GpsUtilities.DmsToDecimal(130.0, "W");

        result.Should().BeApproximately(-1.5, 0.0001);
    }

    /// <summary>
    /// Verifies that the KnotsToKmh method returns the correct value for 1 knot.
    /// </summary>
    [Fact]
    public void KnotsToKmh_OneKnot_ReturnsOnePointEightFiveTwo()
    {
        GpsUtilities.KnotsToKmh(1).Should().BeApproximately(1.852, 0.0001);
    }

    /// <summary>
    /// Verifies that the KmhToKnots method returns the correct value for 1.852 km/h.
    /// </summary>
    [Fact]
    public void KmhToKnots_OnePointEightFiveTwoKmh_ReturnsOneKnot()
    {
        GpsUtilities.KmhToKnots(1.852).Should().BeApproximately(1.0, 0.0001);
    }

    /// <summary>
    /// Verifies that the KmhToMs method returns the correct value for 3.6 km/h.
    /// </summary>
    [Fact]
    public void KmhToMs_ThreePointSixKmh_ReturnsOneMs()
    {
        GpsUtilities.KmhToMs(3.6).Should().BeApproximately(1.0, 0.0001);
    }

    /// <summary>
    /// Verifies that the IsWithinBounds method returns true when the point is inside the bounding box.
    /// </summary>
    [Fact]
    public void IsWithinBounds_PointInsideBoundingBox_ReturnsTrue()
    {
        GpsUtilities.IsWithinBounds(51.5, -0.1, 50.0, 52.0, -1.0, 1.0).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the IsWithinBounds method returns false when the point is outside the bounding box.
    /// </summary>
    [Fact]
    public void IsWithinBounds_PointOutsideBoundingBox_ReturnsFalse()
    {
        // Latitude 53.0 exceeds the maxLat of 52.0
        GpsUtilities.IsWithinBounds(53.0, -0.1, 50.0, 52.0, -1.0, 1.0).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the GetBoundingBoxCenter method returns the midpoint of the bounding box.
    /// </summary>
    [Fact]
    public void GetBoundingBoxCenter_SymmetricBounds_ReturnsMidpoint()
    {
        var (lat, lon) = GpsUtilities.GetBoundingBoxCenter(0, 10, 0, 10);

        lat.Should().Be(5);
        lon.Should().Be(5);
    }

    /// <summary>
    /// Verifies that the CalculateZoomLevel method returns the maximum zoom level when the bounding box is zero.
    /// </summary>
    [Fact]
    public void CalculateZoomLevel_ZeroBoundingBox_ReturnsMaxZoom()
    {
        // When min == max (single point), returns zoom level 18
        var zoom = GpsUtilities.CalculateZoomLevel(51.5, 51.5, -0.1, -0.1);

        zoom.Should().Be(18);
    }
}
