#nullable enable
using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using Xunit;

namespace GpsTrackerProtocol.Tests;

public sealed class GpsUtilitiesEdgeCaseTests
{
    [Theory]
    [InlineData(-90, -180, true)]
    [InlineData(90, 180, true)]
    [InlineData(0, 0, true)]
    [InlineData(-91, 0, false)]
    [InlineData(91, 0, false)]
    [InlineData(0, -181, false)]
    [InlineData(0, 181, false)]
    public void IsValidCoordinate_BoundaryValues(double lat, double lon, bool expected) =>
        GpsUtilities.IsValidCoordinate(lat, lon).Should().Be(expected);

    [Fact]
    public void CalculateDistanceKm_SamePoint_ReturnsZero()
    {
        var distance = GpsUtilities.CalculateDistanceKm(42.6977, 23.3219, 42.6977, 23.3219);
        distance.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void CalculateDistanceKm_KnownDistance_ReturnsApproximateValue()
    {
        // Sofia to Plovdiv ~130 km
        var distance = GpsUtilities.CalculateDistanceKm(42.6977, 23.3219, 42.1354, 24.7453);
        distance.Should().BeInRange(120, 140);
    }

    [Fact]
    public void CalculateDistanceKm_InvalidCoordinates_ReturnsZero()
    {
        var distance = GpsUtilities.CalculateDistanceKm(999, 999, 0, 0);
        distance.Should().Be(0);
    }

    [Fact]
    public void CalculateDistanceKm_AntipodialPoints_ReturnsApproxHalfEarthCircumference()
    {
        // North pole to South pole ~20015 km
        var distance = GpsUtilities.CalculateDistanceKm(90, 0, -90, 0);
        distance.Should().BeInRange(20000, 20100);
    }

    [Fact]
    public void CalculateBearing_DueNorth_ReturnsApproxZero()
    {
        var bearing = GpsUtilities.CalculateBearing(0, 0, 1, 0);
        bearing.Should().BeApproximately(0, 1);
    }

    [Fact]
    public void CalculateBearing_DueEast_ReturnsApprox90()
    {
        var bearing = GpsUtilities.CalculateBearing(0, 0, 0, 1);
        bearing.Should().BeApproximately(90, 1);
    }

    [Fact]
    public void CalculateBearing_DueSouth_ReturnsApprox180()
    {
        var bearing = GpsUtilities.CalculateBearing(1, 0, 0, 0);
        bearing.Should().BeApproximately(180, 1);
    }

    [Fact]
    public void CalculateBearing_InvalidCoordinates_ReturnsZero()
    {
        var bearing = GpsUtilities.CalculateBearing(999, 999, 0, 0);
        bearing.Should().Be(0);
    }

    [Fact]
    public void CalculateBearing_SamePoint_DoesNotThrow()
    {
        var act = () => GpsUtilities.CalculateBearing(42.69, 23.32, 42.69, 23.32);
        act.Should().NotThrow();
    }
}
