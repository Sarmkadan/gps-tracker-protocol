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

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns approximately 111.19 km when moving one degree north.
    /// This tests the known distance property of latitude degrees.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_OneDegreeNorth_ReturnsApproximatelyOneHundredEleven()
    {
        // One degree of latitude ≈ 111.19 km (constant regardless of longitude)
        var distance = GpsUtilities.CalculateDistanceKm(40.0, 0.0, 41.0, 0.0);

        distance.Should().BeApproximately(111.19, 0.1);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns approximately 111.19 km when moving one degree south.
    /// Tests that direction doesn't matter for latitude distance.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_OneDegreeSouth_ReturnsApproximatelyOneHundredEleven()
    {
        var distance = GpsUtilities.CalculateDistanceKm(41.0, 0.0, 40.0, 0.0);

        distance.Should().BeApproximately(111.19, 0.1);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns correct distance for New York to Los Angeles.
    /// Known distance: ~3940 km
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_NewYorkToLosAngeles_ReturnsKnownDistance()
    {
        // New York coordinates
        var nyLat = 40.7128;
        var nyLon = -74.0060;

        // Los Angeles coordinates
        var laLat = 34.0522;
        var laLon = -118.2437;

        var distance = GpsUtilities.CalculateDistanceKm(nyLat, nyLon, laLat, laLon);

        // Known approximate distance between NYC and LA is ~3940 km
        distance.Should().BeApproximately(3940, 10);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns correct distance for London to Paris.
    /// Known distance: ~344 km
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_LondonToParis_ReturnsKnownDistance()
    {
        // London coordinates
        var londonLat = 51.5074;
        var londonLon = -0.1278;

        // Paris coordinates
        var parisLat = 48.8566;
        var parisLon = 2.3522;

        var distance = GpsUtilities.CalculateDistanceKm(londonLat, londonLon, parisLat, parisLon);

        // Known approximate distance between London and Paris is ~344 km
        distance.Should().BeApproximately(344, 5);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns correct distance for Sydney to Tokyo.
    /// Tests long distance calculation across Pacific Ocean.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_SydneyToTokyo_ReturnsKnownDistance()
    {
        // Sydney coordinates
        var sydneyLat = -33.8688;
        var sydneyLon = 151.2093;

        // Tokyo coordinates
        var tokyoLat = 35.6762;
        var tokyoLon = 139.6503;

        var distance = GpsUtilities.CalculateDistanceKm(sydneyLat, sydneyLon, tokyoLat, tokyoLon);

        // Known approximate distance between Sydney and Tokyo is ~7820 km
        distance.Should().BeApproximately(7820, 20);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method handles coordinates near the antimeridian.
    /// The Haversine formula will calculate the long way around (not crossing antimeridian),
    /// so we test that it returns a reasonable distance.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_NearAntimeridian_ReturnsReasonableDistance()
    {
        // Point near antimeridian in Eastern hemisphere (just west of 180°)
        var point1Lat = 0.0;
        var point1Lon = 179.9;

        // Point near antimeridian in Western hemisphere (just east of 180°)
        var point2Lat = 0.0;
        var point2Lon = -179.9;

        var distance = GpsUtilities.CalculateDistanceKm(point1Lat, point1Lon, point2Lat, point2Lon);

        // The Haversine formula calculates the long way around (not crossing antimeridian)
        // Distance should be reasonable (less than half Earth's circumference)
        distance.Should().BeLessThan(20038); // Half of Earth's circumference
        distance.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Verifies that the CalculateDistanceKm method returns correct distance for North Pole to South Pole.
    /// Tests maximum distance calculation.
    /// </summary>
    [Fact]
    public void CalculateDistanceKm_NorthPoleToSouthPole_ReturnsKnownDistance()
    {
        // North Pole
        var northPoleLat = 90.0;
        var northPoleLon = 0.0;

        // South Pole
        var southPoleLat = -90.0;
        var southPoleLon = 0.0;

        var distance = GpsUtilities.CalculateDistanceKm(northPoleLat, northPoleLon, southPoleLat, southPoleLon);

        // Distance from North Pole to South Pole should be approximately Earth's diameter
        distance.Should().BeApproximately(20015, 10);
    }

    /// <summary>
    /// Verifies that the CalculateBearing method returns correct bearing for cardinal directions.
    /// </summary>
    [Fact]
    public void CalculateBearing_CardinalDirections_ReturnsCorrectBearings()
    {
        // North: bearing = 0°
        var bearingNorth = GpsUtilities.CalculateBearing(0, 0, 1, 0);
        bearingNorth.Should().BeApproximately(0, 0.01);

        // East: bearing = 90°
        var bearingEast = GpsUtilities.CalculateBearing(0, 0, 0, 1);
        bearingEast.Should().BeApproximately(90, 0.01);

        // South: bearing = 180°
        var bearingSouth = GpsUtilities.CalculateBearing(0, 0, -1, 0);
        bearingSouth.Should().BeApproximately(180, 0.01);

        // West: bearing = 270°
        var bearingWest = GpsUtilities.CalculateBearing(0, 0, 0, -1);
        bearingWest.Should().BeApproximately(270, 0.01);
    }

    /// <summary>
    /// Verifies that the CalculateBearing method returns correct bearing for intermediate directions.
    /// </summary>
    [Fact]
    public void CalculateBearing_IntermediateDirections_ReturnsCorrectBearings()
    {
        // Northeast: bearing = 45°
        var bearingNortheast = GpsUtilities.CalculateBearing(0, 0, 1, 1);
        bearingNortheast.Should().BeApproximately(45, 0.01);

        // Southeast: bearing = 135°
        var bearingSoutheast = GpsUtilities.CalculateBearing(0, 0, -1, 1);
        bearingSoutheast.Should().BeApproximately(135, 0.01);

        // Southwest: bearing = 225°
        var bearingSouthwest = GpsUtilities.CalculateBearing(0, 0, -1, -1);
        bearingSouthwest.Should().BeApproximately(225, 0.01);

        // Northwest: bearing = 315°
        var bearingNorthwest = GpsUtilities.CalculateBearing(0, 0, 1, -1);
        bearingNorthwest.Should().BeApproximately(315, 0.01);
    }

    /// <summary>
    /// Verifies that the CalculateBearing method returns a valid bearing for real-world coordinates.
    /// </summary>
    [Fact]
    public void CalculateBearing_RealWorldCoordinates_ReturnsValidBearing()
    {
        var lat1 = 40.7128;
        var lon1 = -74.0060;
        var lat2 = 34.0522;
        var lon2 = -118.2437;

        var bearing = GpsUtilities.CalculateBearing(lat1, lon1, lat2, lon2);

        // Should return a valid bearing between 0-360
        bearing.Should().BeInRange(0, 360);
    }

    /// <summary>
    /// Verifies that the CalculateBearing method handles coordinates near the poles correctly.
    /// </summary>
    [Fact]
    public void CalculateBearing_NearPoles_ReturnsValidBearing()
    {
        // From equator to North Pole
        var bearing1 = GpsUtilities.CalculateBearing(0, 0, 89, 0);
        bearing1.Should().BeApproximately(0, 0.01);

        // From North Pole to equator (bearing is undefined at pole, but should not crash)
        var bearing2 = GpsUtilities.CalculateBearing(89, 0, 0, 0);
        bearing2.Should().BeApproximately(180, 0.01);
    }
}
