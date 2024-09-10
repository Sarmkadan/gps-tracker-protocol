#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute; // Changed from Moq
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Xunit; // Added explicitly

namespace GpsTrackerProtocol.Tests;

public class DomainAndServiceTests
{
    // ── LocationData ──────────────────────────────────────────────────────────

    [Fact]
    public void LocationData_IsValid_WithValidCoordinatesAndDeviceId_ReturnsTrue()
    {
        var location = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 60,
            Bearing = 270,
            SatelliteCount = 9
        };

        location.IsValid().Should().BeTrue();
    }

    [Fact]
    public void LocationData_IsValid_WithLatitudeExceedingNinetyDegrees_ReturnsFalse()
    {
        var location = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 91,
            Longitude = -0.1278,
            Speed = 0,
            Bearing = 0,
            SatelliteCount = 0
        };

        location.IsValid().Should().BeFalse();
    }

    [Fact]
    public void LocationData_IsValid_WithNegativeSpeed_ReturnsFalse()
    {
        var location = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 51.5,
            Longitude = -0.1,
            Speed = -10,
            Bearing = 0,
            SatelliteCount = 5
        };

        location.IsValid().Should().BeFalse();
    }

    [Fact]
    public void LocationData_IsValid_WithEmptyDeviceId_ReturnsFalse()
    {
        var location = new LocationData
        {
            DeviceId = string.Empty,
            Latitude = 51.5,
            Longitude = -0.1,
            Speed = 0,
            Bearing = 0,
            SatelliteCount = 4
        };

        location.IsValid().Should().BeFalse();
    }

    [Fact]
    public void LocationData_IsValid_BearingExceedingThreeSixtyDegrees_ReturnsFalse()
    {
        var location = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 51.5,
            Longitude = -0.1,
            Speed = 0,
            Bearing = 361,
            SatelliteCount = 4
        };

        location.IsValid().Should().BeFalse();
    }

    [Fact]
    public void LocationData_DistanceTo_SameLocation_ReturnsZero()
    {
        var location = new LocationData { Latitude = 51.5074, Longitude = -0.1278 };

        location.DistanceTo(location).Should().BeApproximately(0, 0.0001);
    }

    [Fact]
    public void LocationData_DistanceTo_LondonToManchester_ReturnsExpectedDistance()
    {
        // London to Manchester ≈ 263 km straight-line
        var london = new LocationData { Latitude = 51.5074, Longitude = -0.1278 };
        var manchester = new LocationData { Latitude = 53.4808, Longitude = -2.2426 };

        london.DistanceTo(manchester).Should().BeInRange(250, 280);
    }

    [Fact]
    public void LocationData_BearingTo_DueNorthPoint_ReturnsZeroDegrees()
    {
        var origin = new LocationData { Latitude = 0, Longitude = 0 };
        var north = new LocationData { Latitude = 1, Longitude = 0 };

        origin.BearingTo(north).Should().BeApproximately(0, 0.01);
    }

    // ── Device ────────────────────────────────────────────────────────────────

    [Fact]
    public void Device_IsValid_WithFifteenDigitImei_ReturnsTrue()
    {
        var device = new Device { Id = "device-001", Imei = "123456789012345" };

        device.IsValid().Should().BeTrue();
    }

    [Fact]
    public void Device_IsValid_WithAlphaNumericImei_ReturnsFalse()
    {
        var device = new Device { Id = "device-001", Imei = "ABCDE6789012345" };

        device.IsValid().Should().BeFalse();
    }

    [Fact]
    public void Device_IsValid_WithTooShortImei_ReturnsFalse()
    {
        var device = new Device { Id = "device-001", Imei = "12345" };

        device.IsValid().Should().BeFalse();
    }

    [Fact]
    public void Device_IsValid_WithEmptyId_ReturnsFalse()
    {
        var device = new Device { Id = string.Empty, Imei = "123456789012345" };

        device.IsValid().Should().BeFalse();
    }

    [Fact]
    public void Device_UpdateHeartbeat_SetsStatusToOnline()
    {
        var device = new Device { Id = "dev1", Imei = "123456789012345", Status = DeviceStatus.Offline };

        device.UpdateHeartbeat();

        device.Status.Should().Be(DeviceStatus.Online);
    }

    [Fact]
    public void Device_UpdateHeartbeat_WithIpAndPort_UpdatesNetworkInfo()
    {
        var device = new Device { Id = "dev1", Imei = "123456789012345" };

        device.UpdateHeartbeat("192.168.1.100", 5023);

        device.IpAddress.Should().Be("192.168.1.100");
        device.Port.Should().Be(5023);
    }

    [Fact]
    public void Device_IsOffline_AfterRecentHeartbeat_ReturnsFalse()
    {
        var device = new Device { Id = "dev1", Imei = "123456789012345" };
        device.UpdateHeartbeat();

        device.IsOffline(TimeSpan.FromMinutes(5)).Should().BeFalse();
    }

    [Fact]
    public void Device_IsOffline_WithStaleLastSeen_ReturnsTrue()
    {
        var device = new Device
        {
            Id = "dev1",
            Imei = "123456789012345",
            LastSeen = DateTime.UtcNow.AddHours(-2)
        };

        device.IsOffline(TimeSpan.FromMinutes(10)).Should().BeTrue();
    }

    // ── GpsFrame ──────────────────────────────────────────────────────────────

    [Fact]
    public void GpsFrame_IsValid_ValidGT06FrameWithChecksum_ReturnsTrue()
    {
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = new byte[20],   // ≥ 15 bytes required for GT06
            IsValidChecksum = true
        };

        frame.IsValid().Should().BeTrue();
    }

    [Fact]
    public void GpsFrame_IsValid_GT06FrameTooShort_ReturnsFalse()
    {
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = new byte[10],   // < 15 bytes
            IsValidChecksum = true
        };

        frame.IsValid().Should().BeFalse();
    }

    [Fact]
    public void GpsFrame_IsValid_EmptyRawData_ReturnsFalse()
    {
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = [],
            IsValidChecksum = true
        };

        frame.IsValid().Should().BeFalse();
    }

    [Fact]
    public void GpsFrame_IsValid_InvalidChecksum_ReturnsFalse()
    {
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = new byte[20],
            IsValidChecksum = false
        };

        frame.IsValid().Should().BeFalse();
    }

    [Fact]
    public void GpsFrame_ToHex_ByteArray_ReturnsUppercaseHexString()
    {
        var frame = new GpsFrame { RawData = new byte[] { 0x78, 0x78, 0x0D } };

        frame.ToHex().Should().Be("78780D");
    }

    [Fact]
    public void GpsFrame_ExtractBytes_ValidOffsetAndLength_ReturnsCorrectRange()
    {
        var frame = new GpsFrame { RawData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 } };

        frame.ExtractBytes(1, 3).Should().Equal(new byte[] { 0x02, 0x03, 0x04 });
    }

    [Fact]
    public void GpsFrame_ExtractBytes_OutOfBoundsOffset_ThrowsArgumentException()
    {
        var frame = new GpsFrame { RawData = new byte[] { 0x01, 0x02 } };

        var act = () => frame.ExtractBytes(5, 1);

        act.Should().Throw<ArgumentException>();
    }

    // ── GeofenceService ───────────────────────────────────────────────────────

    [Fact]
    public void GeofenceService_IsInsideGeofence_PointAtExactCenter_ReturnsTrue()
    {
        var mockLogger = Substitute.For<ILogger<GeofenceService>>(); // Changed from Mock
        var service = new GeofenceService(mockLogger); // Changed from mockLogger.Object
        service.AddGeofence("zone-london", 51.5074, -0.1278, 1.0);

        var result = service.IsInsideGeofence("zone-london", 51.5074, -0.1278);

        result.Should().BeTrue();
    }

    [Fact]
    public void GeofenceService_IsInsideGeofence_PointFarOutside_ReturnsFalse()
    {
        var mockLogger = Substitute.For<ILogger<GeofenceService>>(); // Changed from Mock
        var service = new GeofenceService(mockLogger); // Changed from mockLogger.Object
        service.AddGeofence("zone-london", 51.5074, -0.1278, 1.0);

        // Paris is ~340 km from London — far beyond 1 km radius
        var result = service.IsInsideGeofence("zone-london", 48.8566, 2.3522);

        result.Should().BeFalse();
    }

    [Fact]
    public void GeofenceService_IsInsideGeofence_UnknownGeofenceId_ReturnsFalse()
    {
        var mockLogger = Substitute.For<ILogger<GeofenceService>>(); // Changed from Mock
        var service = new GeofenceService(mockLogger); // Changed from mockLogger.Object

        var result = service.IsInsideGeofence("nonexistent-zone", 51.5074, -0.1278);

        result.Should().BeFalse();
    }

    [Fact]
    public void GeofenceService_GetNearbyGeofences_SearchRadiusEncompassesGeofence_ReturnsItsId()
    {
        var mockLogger = Substitute.For<ILogger<GeofenceService>>(); // Changed from Mock
        var service = new GeofenceService(mockLogger); // Changed from mockLogger.Object
        service.AddGeofence("depot", 51.5, -0.1, 0.5);

        // Search from 0.3 km away with 1 km radius — depot is within searchRadius + depot.RadiusKm
        var nearby = service.GetNearbyGeofences(51.503, -0.1, 1.0);

        nearby.Should().Contain("depot");
    }

    [Fact]
    public void GeofenceService_AddGeofence_WithInvalidCoordinates_DoesNotAddGeofence()
    {
        var mockLogger = Substitute.For<ILogger<GeofenceService>>(); // Changed from Mock
        var service = new GeofenceService(mockLogger); // Changed from mockLogger.Object

        // Invalid coordinates: latitude 200, longitude 300
        service.AddGeofence("bad-zone", 200, 300, 1.0);

        service.IsInsideGeofence("bad-zone", 0, 0).Should().BeFalse();
    }
}
