#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpsTrackerProtocol.Tests.Services;

using System;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests for location sanity filtering functionality.
/// </summary>
public class LocationSanityFilterTests
{
    private readonly ILocationSanityFilter _filter;

    public LocationSanityFilterTests()
    {
        _filter = new LocationSanityFilter();
    }

    [Fact]
    public void FilterLocation_WithNullLocation_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _filter.FilterLocation(null!));
    }

    [Fact]
    public void FilterLocation_WithValidLocation_ReturnsAccepted()
    {
        // Arrange
        var location = new LocationData
        {
            Id = "loc-1",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 60,
            Altitude = 100,
            SatelliteCount = 8
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert
        result.IsAccepted.Should().BeTrue();
        result.Decision.Should().Be(FilterDecision.Accepted);
    }

    [Fact]
    public void FilterLocation_WithNullIslandCoordinates_ReturnsNullIsland()
    {
        // Arrange
        var location = new LocationData
        {
            Id = "loc-null-island",
            DeviceId = "device-123",
            Latitude = 0,
            Longitude = 0,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 0,
            SatelliteCount = 0
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert
        result.IsAccepted.Should().BeFalse();
        result.Decision.Should().Be(FilterDecision.NullIsland);
    }

    [Fact]
    public void FilterLocation_WithInvalidCoordinates_ReturnsInvalidCoordinates()
    {
        // Arrange
        var location = new LocationData
        {
            Id = "loc-invalid-coords",
            DeviceId = "device-123",
            Latitude = 100, // Invalid latitude (> 90)
            Longitude = -200, // Invalid longitude (< -180)
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 0,
            SatelliteCount = 0
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert
        result.IsAccepted.Should().BeFalse();
        result.Decision.Should().Be(FilterDecision.InvalidCoordinates);
    }

    [Fact]
    public void FilterLocation_WithNoSatelliteFix_ReturnsNoGpsFix()
    {
        // Arrange
        var location = new LocationData
        {
            Id = "loc-no-fix",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 100,
            SatelliteCount = -1 // Invalid satellite count
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert
        result.IsAccepted.Should().BeFalse();
        result.Decision.Should().Be(FilterDecision.NoGpsFix);
    }

    [Fact]
    public void FilterLocation_WithFutureTimestamp_ReturnsInvalidTimestamp()
    {
        // Arrange
        var location = new LocationData
        {
            Id = "loc-future",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow.AddHours(2), // 2 hours in the future
            Speed = 0,
            Altitude = 100,
            SatelliteCount = 8
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert
        result.IsAccepted.Should().BeFalse();
        result.Decision.Should().Be(FilterDecision.InvalidTimestamp);
    }

    [Fact]
    public void FilterLocation_WithPastTimestamp_ReturnsInvalidTimestamp()
    {
        // Arrange
        var location = new LocationData
        {
            Id = "loc-past",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow.AddYears(-2), // 2 years in the past
            Speed = 0,
            Altitude = 100,
            SatelliteCount = 8
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert
        result.IsAccepted.Should().BeFalse();
        result.Decision.Should().Be(FilterDecision.InvalidTimestamp);
    }

    [Fact]
    public void FilterLocation_WithImpossibleAltitude_ReturnsImpossibleAltitude()
    {
        // Arrange
        var location = new LocationData
        {
            Id = "loc-impossible-alt",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 20000, // Impossible altitude (> 9000m)
            SatelliteCount = 8
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert
        result.IsAccepted.Should().BeFalse();
        result.Decision.Should().Be(FilterDecision.ImpossibleAltitude);
    }

    [Fact]
    public void FilterLocation_WithImpossibleSpeedBetweenPoints_ReturnsImpossibleSpeed()
    {
        // Arrange
        var previousLocation = new LocationData
        {
            Id = "loc-prev",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow.AddMinutes(-5), // 5 minutes ago
            Speed = 0,
            Altitude = 100,
            SatelliteCount = 8
        };

        var currentLocation = new LocationData
        {
            Id = "loc-impossible-speed",
            DeviceId = "device-123",
            Latitude = 51.5074, // London - about 5500 km from NYC
            Longitude = -0.1278,
            Timestamp = DateTime.UtcNow, // Now
            Speed = 1000, // Impossible speed
            Altitude = 100,
            SatelliteCount = 8
        };

        // Act
        var result = _filter.FilterLocation(currentLocation, previousLocation);

        // Assert
        result.IsAccepted.Should().BeFalse();
        result.Decision.Should().Be(FilterDecision.ImpossibleSpeed);
    }

    [Fact]
    public void FilterLocation_WithValidSpeedBetweenPoints_ReturnsAccepted()
    {
        // Arrange
        var previousLocation = new LocationData
        {
            Id = "loc-prev",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow.AddMinutes(-5), // 5 minutes ago
            Speed = 0,
            Altitude = 100,
            SatelliteCount = 8
        };

        var currentLocation = new LocationData
        {
            Id = "loc-valid-speed",
            DeviceId = "device-123",
            Latitude = 40.7130, // Very close to previous
            Longitude = -74.0062,
            Timestamp = DateTime.UtcNow, // Now
            Speed = 10,
            Altitude = 100,
            SatelliteCount = 8
        };

        // Act
        var result = _filter.FilterLocation(currentLocation, previousLocation);

        // Assert
        result.IsAccepted.Should().BeTrue();
        result.Decision.Should().Be(FilterDecision.Accepted);
    }

    [Fact]
    public void FilterLocation_WithColdStartZeroSatellites_ReturnsAccepted()
    {
        // Arrange - Cold start scenario where device has coordinates but no satellites yet
        var location = new LocationData
        {
            Id = "loc-cold-start",
            DeviceId = "device-123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 100,
            SatelliteCount = 0 // Cold start - no satellites acquired yet
        };

        // Act
        var result = _filter.FilterLocation(location);

        // Assert - Should accept cold start with valid coordinates and timestamp
        result.IsAccepted.Should().BeTrue();
        result.Decision.Should().Be(FilterDecision.Accepted);
    }

    [Fact]
    public void Configuration_ShouldHaveDefaultValues()
    {
        // Arrange
        var filter = new LocationSanityFilter();

        // Act
        var config = filter.Configuration;

        // Assert
        config.MaxSpeedKmh.Should().Be(300.0);
        config.MaxAltitudeMeters.Should().Be(9000.0);
        config.MinAltitudeMeters.Should().Be(-500.0);
        config.MaxTimestampSkewHours.Should().Be(1.0);
        config.RejectNoSatelliteFix.Should().BeTrue();
        config.RejectNullIsland.Should().BeTrue();
        config.RejectInvalidCoordinates.Should().BeTrue();
    }

    [Fact]
    public void FilterLocation_WithCustomConfig_RespectsConfiguration()
    {
        // Arrange
        var config = new LocationSanityFilterConfig
        {
            RejectNullIsland = false, // Disable null island rejection
            MaxSpeedKmh = 1000 // High max speed
        };

        var filter = new LocationSanityFilter(config);

        var nullIslandLocation = new LocationData
        {
            Id = "loc-null-island",
            DeviceId = "device-123",
            Latitude = 0,
            Longitude = 0,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 0,
            SatelliteCount = 0
        };

        // Act
        var result = filter.FilterLocation(nullIslandLocation);

        // Assert - Should accept because we disabled null island rejection
        result.IsAccepted.Should().BeTrue();
        result.Decision.Should().Be(FilterDecision.Accepted);
    }
}
