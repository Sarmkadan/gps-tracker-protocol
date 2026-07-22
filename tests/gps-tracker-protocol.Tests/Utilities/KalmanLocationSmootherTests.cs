using System;
using FluentAssertions;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Utilities;
using Xunit;

namespace GpsTrackerProtocol.Tests.Utilities;

public class KalmanLocationSmootherTests
{
    private readonly KalmanLocationSmoother _smoother = new();

    [Fact]
    public void Smooth_FirstFixForDevice_ReturnsUnchangedLocationWithRawValuesInExtendedData()
    {
        // Arrange
        var fix = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 52.5200,
            Longitude = 13.4050,
            Timestamp = DateTime.UtcNow,
            Accuracy = 10.0,
            Speed = 5.0,
            Altitude = 100.0,
            Bearing = 90.0,
            SatelliteCount = 8,
            Protocol = Domain.ProtocolType.Unknown
        };

        // Act
        var result = _smoother.Smooth(fix);

        // Assert
        result.Should().NotBeNull();
        result!.DeviceId.Should().Be("device-001");
        result.Latitude.Should().BeApproximately(52.5200, 0.00001);
        result.Longitude.Should().BeApproximately(13.4050, 0.00001);
        result.Accuracy.Should().Be(10.0);
        result.Speed.Should().Be(5.0);
        result.Altitude.Should().Be(100.0);
        result.Bearing.Should().Be(90.0);
        result.SatelliteCount.Should().Be(8);
        result.Protocol.Should().Be(Domain.ProtocolType.Unknown);

        // First fix should have raw values in ExtendedData
        result.ExtendedData.Should().ContainKey("kalman.rawLat");
        result.ExtendedData["kalman.rawLat"].Should().Be(52.5200);
        result.ExtendedData.Should().ContainKey("kalman.rawLon");
        result.ExtendedData["kalman.rawLon"].Should().Be(13.4050);
    }

    [Fact]
    public void Smooth_FirstFixWithZeroAccuracy_UsesDefaultAccuracy()
    {
        // Arrange
        var fix = new LocationData
        {
            DeviceId = "device-002",
            Latitude = 48.8566,
            Longitude = 2.3522,
            Timestamp = DateTime.UtcNow,
            Accuracy = 0, // Invalid accuracy
            Speed = 0,
            Protocol = Domain.ProtocolType.Unknown
        };

        // Act
        var result = _smoother.Smooth(fix);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().BeApproximately(48.8566, 0.00001);
        result.Longitude.Should().BeApproximately(2.3522, 0.00001);
    }

    [Fact]
    public void Smooth_NullFix_ReturnsNull()
    {
        // Arrange
        LocationData? fix = null;

        // Act
        var result = _smoother.Smooth(fix);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Smooth_ConsecutiveFixes_SmoothsTowardsSecondPoint()
    {
        // Arrange
        var deviceId = "device-003";
        var firstFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 51.5074,
            Longitude = -0.1278,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 5.0,
            Speed = 10.0,
            Protocol = Domain.ProtocolType.Unknown
        };

        var secondFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 51.5075,
            Longitude = -0.1277,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 5), // 5 seconds later
            Accuracy = 5.0,
            Speed = 10.0,
            Protocol = Domain.ProtocolType.Unknown
        };

        // Act - process first fix
        var firstResult = _smoother.Smooth(firstFix);

        // Act - process second fix
        var secondResult = _smoother.Smooth(secondFix);

        // Assert
        secondResult.Should().NotBeNull();
        secondResult!.Latitude.Should().NotBe(secondFix.Latitude); // Should be smoothed
        secondResult.Longitude.Should().NotBe(secondFix.Longitude); // Should be smoothed

        // Smoothed values should be between first and second raw values
        secondResult.Latitude.Should().BeGreaterThan(firstResult.Latitude);
        secondResult.Latitude.Should().BeLessThan(secondFix.Latitude);
        secondResult.Longitude.Should().BeGreaterThan(firstResult.Longitude);
        secondResult.Longitude.Should().BeLessThan(secondFix.Longitude);

        // Raw values should be preserved in ExtendedData
        secondResult.ExtendedData["kalman.rawLat"].Should().Be(secondFix.Latitude);
        secondResult.ExtendedData["kalman.rawLon"].Should().Be(secondFix.Longitude);
    }

    [Fact]
    public void Smooth_OutlierFixWithImpossiblyHighSpeed_ReturnsNull()
    {
        // Arrange
        var deviceId = "device-004";
        var firstFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 40.7128,
            Longitude = -74.0060, // New York
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 10.0,
            Speed = 0
        };

        var secondFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 34.0522, // Los Angeles - too far for 1 second at 300 km/h
            Longitude = -118.2437,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 1), // 1 second later
            Accuracy = 10.0,
            Speed = 0
        };

        // Act - process first fix
        var firstResult = _smoother.Smooth(firstFix);
        firstResult.Should().NotBeNull();

        // Act - process second fix (should be rejected as outlier)
        var secondResult = _smoother.Smooth(secondFix);

        // Assert
        secondResult.Should().BeNull("because the device cannot travel from NY to LA in 1 second");
    }

    [Fact]
    public void Smooth_OutlierFixWithHighButPlausibleSpeed_ReturnsSmoothedResult()
    {
        // Arrange - test at the boundary of MaxPlausibleSpeedKmh (300 km/h)
        var deviceId = "device-005";
        var firstFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 51.5074,
            Longitude = -0.1278,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 10.0,
            Speed = 0
        };

        // Calculate a distance that's just under the max plausible for 1 second
        // Max plausible distance in 1 second at 300 km/h = 300 * 1000 / 3600 = 83.33 meters
        var distanceMeters = 80.0; // Just under the limit
        var firstLat = 51.5074;
        var firstLon = -0.1278;
        var secondLat = firstLat + (distanceMeters / 111320.0); // Approximate conversion
        var secondLon = firstLon;

        var secondFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = secondLat,
            Longitude = secondLon,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 1),
            Accuracy = 10.0,
            Speed = 0
        };

        // Act - process first fix
        var firstResult = _smoother.Smooth(firstFix);
        firstResult.Should().NotBeNull();

        // Act - process second fix (should be accepted)
        var secondResult = _smoother.Smooth(secondFix);

        // Assert
        secondResult.Should().NotBeNull("because 80m in 1s is under the 300 km/h limit");
        secondResult!.Latitude.Should().NotBe(secondFix.Latitude);
    }

    [Fact]
    public void Smooth_NegativeTimeDelta_ReturnsUnchangedLocation()
    {
        // Arrange
        var deviceId = "device-006";
        var firstFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 48.8566,
            Longitude = 2.3522,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 5),
            Accuracy = 10.0
        };

        var secondFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 48.8567,
            Longitude = 2.3523,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 1), // Earlier timestamp!
            Accuracy = 10.0
        };

        // Act - process first fix
        var firstResult = _smoother.Smooth(firstFix);
        firstResult.Should().NotBeNull();

        // Act - process second fix with earlier timestamp
        var secondResult = _smoother.Smooth(secondFix);

        // Assert - should return unchanged location when time delta is negative
        secondResult.Should().NotBeNull();
        secondResult!.Latitude.Should().Be(secondFix.Latitude);
        secondResult.Longitude.Should().Be(secondFix.Longitude);
    }

    [Fact]
    public void Smooth_ZeroTimeDelta_ReturnsUnchangedLocation()
    {
        // Arrange
        var deviceId = "device-007";
        var fix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 35.6762,
            Longitude = 139.6503,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 10.0
        };

        // Act - process same timestamp twice
        var firstResult = _smoother.Smooth(fix);
        var secondResult = _smoother.Smooth(fix);

        // Assert
        secondResult.Should().NotBeNull();
        secondResult!.Latitude.Should().Be(fix.Latitude);
        secondResult.Longitude.Should().Be(fix.Longitude);
    }

    [Fact]
    public void Reset_ClearsStateForSpecificDevice()
    {
        // Arrange
        var deviceId = "device-008";
        var firstFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 52.5200,
            Longitude = 13.4050,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 10.0
        };

        var secondFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 52.5210,
            Longitude = 13.4060,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 5),
            Accuracy = 10.0
        };

        // Act - process two fixes
        var firstResult = _smoother.Smooth(firstFix);
        var secondResult = _smoother.Smooth(secondFix);
        secondResult.Should().NotBeNull();

        // Reset state
        _smoother.Reset(deviceId);

        // Act - process another fix (should be treated as first fix again)
        var thirdFix = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 52.5220,
            Longitude = 13.4070,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 10),
            Accuracy = 10.0
        };
        var thirdResult = _smoother.Smooth(thirdFix);

        // Assert
        thirdResult.Should().NotBeNull();
        thirdResult!.Latitude.Should().BeApproximately(thirdFix.Latitude, 0.00001);
        thirdResult.Longitude.Should().BeApproximately(thirdFix.Longitude, 0.00001);
    }

    [Fact]
    public void ResetAll_ClearsAllDeviceStates()
    {
        // Arrange
        var device1 = new LocationData
        {
            DeviceId = "device-009",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 10.0
        };

        var device2 = new LocationData
        {
            DeviceId = "device-010",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 10.0
        };

        // Act - process fixes for two devices
        var result1 = _smoother.Smooth(device1);
        var result2 = _smoother.Smooth(device2);
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();

        // Reset all states
        _smoother.ResetAll();

        // Act - process fixes again (should be treated as first fixes)
        var newResult1 = _smoother.Smooth(device1);
        var newResult2 = _smoother.Smooth(device2);

        // Assert
        newResult1.Should().NotBeNull();
        newResult1!.Latitude.Should().BeApproximately(device1.Latitude, 0.00001);
        newResult2.Should().NotBeNull();
        newResult2!.Latitude.Should().BeApproximately(device2.Latitude, 0.00001);
    }

    [Fact]
    public void Reset_NullDeviceId_DoesNotThrow()
    {
        // Act - should not throw
        _smoother.Reset(null);

        // Assert
        _smoother.ResetAll(); // Also test that this doesn't throw
    }

    [Fact]
    public void DistanceMeters_CalculatesCorrectDistance()
    {
        // Arrange
        double lat1 = 52.5200, lon1 = 13.4050; // Berlin
        double lat2 = 48.8566, lon2 = 2.3522; // Paris

        // Act
        var distance = KalmanLocationSmoother.DistanceMeters(lat1, lon1, lat2, lon2);

        // Assert - approximate distance between Berlin and Paris (~878 km)
        distance.Should().BeGreaterThan(800000);
        distance.Should().BeLessThan(900000);
    }

    [Fact]
    public void DistanceMeters_SamePoint_ReturnsZero()
    {
        // Arrange
        double lat = 40.7128, lon = -74.0060;

        // Act
        var distance = KalmanLocationSmoother.DistanceMeters(lat, lon, lat, lon);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void CopyLocationData_CreatesIndependentCopy()
    {
        // Arrange
        var original = new LocationData
        {
            DeviceId = "device-011",
            Latitude = 35.6762,
            Longitude = 139.6503,
            ExtendedData = { { "key1", "value1" }, { "key2", 42 } }
        };

        // Act
        var copy = KalmanLocationSmoother.CopyLocationData(original);

        // Assert - values are the same
        copy.Should().NotBeSameAs(original);
        copy.DeviceId.Should().Be(original.DeviceId);
        copy.Latitude.Should().Be(original.Latitude);
        copy.Longitude.Should().Be(original.Longitude);

        // ExtendedData should be a copy, not the same reference
        copy.ExtendedData.Should().NotBeSameAs(original.ExtendedData);
        copy.ExtendedData["key1"].Should().Be("value1");
        copy.ExtendedData["key2"].Should().Be(42);

        // Modify copy's ExtendedData - original should not be affected
        copy.ExtendedData["key1"] = "modified";
        copy.ExtendedData["key3"] = "new";

        original.ExtendedData.Should().ContainKey("key1");
        original.ExtendedData["key1"].Should().Be("value1");
        original.ExtendedData.Should().NotContainKey("key3");
    }

    [Fact]
    public void CopyLocationData_NullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => KalmanLocationSmoother.CopyLocationData(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Smooth_MultipleDevices_MaintainsSeparateStates()
    {
        // Arrange
        var device1 = new LocationData
        {
            DeviceId = "car-001",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 5.0
        };

        var device2 = new LocationData
        {
            DeviceId = "car-002",
            Latitude = 34.0522,
            Longitude = -118.2437,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 0),
            Accuracy = 5.0
        };

        // Act - process first fix for each device
        var result1 = _smoother.Smooth(device1);
        var result2 = _smoother.Smooth(device2);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();

        // Verify they're different
        result1!.DeviceId.Should().Be("car-001");
        result2!.DeviceId.Should().Be("car-002");

        // Act - process second fix for each device
        var device1Next = new LocationData
        {
            DeviceId = "car-001",
            Latitude = 40.7130,
            Longitude = -74.0062,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 5),
            Accuracy = 5.0
        };

        var device2Next = new LocationData
        {
            DeviceId = "car-002",
            Latitude = 34.0524,
            Longitude = -118.2439,
            Timestamp = new DateTime(2024, 1, 1, 10, 0, 5),
            Accuracy = 5.0
        };

        var result1Next = _smoother.Smooth(device1Next);
        var result2Next = _smoother.Smooth(device2Next);

        // Assert - both should be smoothed independently
        result1Next.Should().NotBeNull();
        result2Next.Should().NotBeNull();
        result1Next!.Latitude.Should().NotBe(device1Next.Latitude);
        result2Next!.Latitude.Should().NotBe(device2Next.Latitude);
    }
}
