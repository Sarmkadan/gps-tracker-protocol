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

/// <summary>
/// Contains unit tests for domain models (LocationData, Device, GpsFrame) and
/// services (GeofenceService). Each test validates a specific behavior or
/// edge‑case of the corresponding model or service method.
/// </summary>
public class DomainAndServiceTests
{
    // ── LocationData ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="LocationData.IsValid"/> returns <c>true</c>
    /// when all required properties contain valid values, including a
    /// non‑empty <c>DeviceId</c>, latitude within ±90°, longitude within
    /// ±180°, non‑negative speed, bearing within 0‑360°, and a positive
    /// satellite count.
    /// </summary>
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

    /// <summary>
    /// Ensures that <see cref="LocationData.IsValid"/> returns <c>false</c>
    /// when the latitude exceeds the allowed range of ±90 degrees.
    /// </summary>
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

    /// <summary>
    /// Checks that <see cref="LocationData.IsValid"/> returns <c>false</c>
    /// when the speed is negative.
    /// </summary>
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

    /// <summary>
    /// Confirms that <see cref="LocationData.IsValid"/> returns <c>false</c>
    /// when the <c>DeviceId</c> is an empty string.
    /// </summary>
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

    /// <summary>
    /// Validates that <see cref="LocationData.IsValid"/> returns <c>false</c>
    /// when the bearing exceeds 360 degrees.
    /// </summary>
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

    /// <summary>
    /// Asserts that <see cref="LocationData.DistanceTo"/> returns zero when the
    /// two locations are identical.
    /// </summary>
    [Fact]
    public void LocationData_DistanceTo_SameLocation_ReturnsZero()
    {
        var location = new LocationData { Latitude = 51.5074, Longitude = -0.1278 };

        location.DistanceTo(location).Should().BeApproximately(0, 0.0001);
    }

    /// <summary>
    /// Verifies that the distance calculated between London and Manchester
    /// falls within the expected range of 250‑280 km.
    /// </summary>
    [Fact]
    public void LocationData_DistanceTo_LondonToManchester_ReturnsExpectedDistance()
    {
        // London to Manchester ≈ 263 km straight-line
        var london = new LocationData { Latitude = 51.5074, Longitude = -0.1278 };
        var manchester = new LocationData { Latitude = 53.4808, Longitude = -2.2426 };

        london.DistanceTo(manchester).Should().BeInRange(250, 280);
    }

    /// <summary>
    /// Checks that <see cref="LocationData.BearingTo"/> returns a bearing of
    /// zero degrees when the target point lies due north of the origin.
    /// </summary>
    [Fact]
    public void LocationData_BearingTo_DueNorthPoint_ReturnsZeroDegrees()
    {
        var origin = new LocationData { Latitude = 0, Longitude = 0 };
        var north = new LocationData { Latitude = 1, Longitude = 0 };

        origin.BearingTo(north).Should().BeApproximately(0, 0.01);
    }

    // ── Device ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Ensures that <see cref="Device.IsValid"/> returns <c>true</c> when the
    /// IMEI consists of exactly fifteen numeric digits.
    /// </summary>
    [Fact]
    public void Device_IsValid_WithFifteenDigitImei_ReturnsTrue()
    {
        var device = new Device { Id = "device-001", Imei = "123456789012345" };

        device.IsValid().Should().BeTrue();
    }

    /// <summary>
    /// Confirms that <see cref="Device.IsValid"/> returns <c>false</c> when the
    /// IMEI contains non‑numeric characters.
    /// </summary>
    [Fact]
    public void Device_IsValid_WithAlphaNumericImei_ReturnsFalse()
    {
        var device = new Device { Id = "device-001", Imei = "ABCDE6789012345" };

        device.IsValid().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="Device.IsValid"/> returns <c>false</c> when the
    /// IMEI length is shorter than the required fifteen digits.
    /// </summary>
    [Fact]
    public void Device_IsValid_WithTooShortImei_ReturnsFalse()
    {
        var device = new Device { Id = "device-001", Imei = "12345" };

        device.IsValid().Should().BeFalse();
    }

    /// <summary>
    /// Checks that <see cref="Device.IsValid"/> returns <c>false</c> when the
    /// device identifier is an empty string.
    /// </summary>
    [Fact]
    public void Device_IsValid_WithEmptyId_ReturnsFalse()
    {
        var device = new Device { Id = string.Empty, Imei = "123456789012345" };

        device.IsValid().Should().BeFalse();
    }

    /// <summary>
    /// Validates that calling <see cref="Device.UpdateHeartbeat"/> without
    /// network parameters sets the device status to <see cref="DeviceStatus.Online"/>.
    /// </summary>
    [Fact]
    public void Device_UpdateHeartbeat_SetsStatusToOnline()
    {
        var device = new Device { Id = "dev1", Imei = "123456789012345", Status = DeviceStatus.Offline };

        device.UpdateHeartbeat();

        device.Status.Should().Be(DeviceStatus.Online);
    }

    /// <summary>
    /// Ensures that <see cref="Device.UpdateHeartbeat(string,int)"/> updates the
    /// device's IP address and port fields with the supplied values.
    /// </summary>
    [Fact]
    public void Device_UpdateHeartbeat_WithIpAndPort_UpdatesNetworkInfo()
    {
        var device = new Device { Id = "dev1", Imei = "123456789012345" };

        device.UpdateHeartbeat("192.168.1.100", 5023);

        device.IpAddress.Should().Be("192.168.1.100");
        device.Port.Should().Be(5023);
    }

    /// <summary>
    /// Confirms that <see cref="Device.IsOffline(TimeSpan)"/> returns
    /// <c>false</c> when the device has a recent heartbeat (i.e., the last
    /// seen timestamp is within the supplied timeout).
    /// </summary>
    [Fact]
    public void Device_IsOffline_AfterRecentHeartbeat_ReturnsFalse()
    {
        var device = new Device { Id = "dev1", Imei = "123456789012345" };
        device.UpdateHeartbeat();

        device.IsOffline(TimeSpan.FromMinutes(5)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="Device.IsOffline(TimeSpan)"/> returns
    /// <c>true</c> when the <c>LastSeen</c> timestamp is older than the
    /// specified timeout interval.
    /// </summary>
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

    /// <summary>
    /// Checks that <see cref="GpsFrame.IsValid"/> returns <c>true</c> for a GT06
    /// frame that meets the minimum length requirement and has a valid checksum.
    /// </summary>
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

    /// <summary>
    /// Ensures that <see cref="GpsFrame.IsValid"/> returns <c>false</c> when a GT06
    /// frame's raw data length is below the required 15 bytes.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="GpsFrame.IsValid"/> returns <c>false</c> when the
    /// raw data array is empty.
    /// </summary>
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

    /// <summary>
    /// Confirms that <see cref="GpsFrame.IsValid"/> returns <c>false</c> when the
    /// checksum validation flag is set to <c>false</c>.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="GpsFrame.ToHex"/> converts a byte array to an
    /// uppercase hexadecimal string.
    /// </summary>
    [Fact]
    public void GpsFrame_ToHex_ByteArray_ReturnsUppercaseHexString()
    {
        var frame = new GpsFrame { RawData = new byte[] { 0x78, 0x78, 0x0D } };

        frame.ToHex().Should().Be("78780D");
    }

    /// <summary>
    /// Validates that <see cref="GpsFrame.ExtractBytes"/> returns the correct
    /// sub‑range when provided with a valid offset and length.
    /// </summary>
    [Fact]
    public void GpsFrame_ExtractBytes_ValidOffsetAndLength_ReturnsCorrectRange()
    {
        var frame = new GpsFrame { RawData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 } };

        frame.ExtractBytes(1, 3).Should().Equal(new byte[] { 0x02, 0x03, 0x04 });
    }

    /// <summary>
    /// Ensures that <see cref="GpsFrame.ExtractBytes"/> throws an
    /// <see cref="ArgumentException"/> when the offset is outside the bounds of
    /// the raw data array.
    /// </summary>
    [Fact]
    public void GpsFrame_ExtractBytes_OutOfBoundsOffset_ThrowsArgumentException()
    {
        var frame = new GpsFrame { RawData = new byte[] { 0x01, 0x02 } };

        var act = () => frame.ExtractBytes(5, 1);

        act.Should().Throw<ArgumentException>();
    }

    // ── GeofenceService ───────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="GeofenceService.IsInsideGeofence"/> returns
    /// <c>true</c> when the queried point lies exactly at the centre of a
    /// geofence that has been added to the service.
    /// </summary>
    [Fact]
    public void GeofenceService_IsInsideGeofence_PointAtExactCenter_ReturnsTrue()
    {
        var mockLogger = Substitute.For<ILogger<GeofenceService>>(); // Changed from Mock
        var service = new GeofenceService(mockLogger); // Changed from mockLogger.Object
        service.AddGeofence("zone-london", 51.5074, -0.1278, 1.0);

        var result = service.IsInsideGeofence("zone-london", 51.5074, -0.1278);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Confirms that <see cref="GeofenceService.IsInsideGeofence"/> returns
    /// <c>false</c> for a point that is far outside the radius of the specified
    /// geofence.
    /// </summary>
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

    /// <summary>
    /// Ensures that <see cref="GeofenceService.IsInsideGeofence"/> returns
    /// <c>false</c> when the supplied geofence identifier does not exist.
    /// </summary>
    [Fact]
    public void GeofenceService_IsInsideGeofence_UnknownGeofenceId_ReturnsFalse()
    {
        var mockLogger = Substitute.For<ILogger<GeofenceService>>(); // Changed from Mock
        var service = new GeofenceService(mockLogger); // Changed from mockLogger.Object

        var result = service.IsInsideGeofence("nonexistent-zone", 51.5074, -0.1278);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="GeofenceService.GetNearbyGeofences"/> includes a
    /// geofence identifier when the search radius plus the geofence's own radius
    /// encompasses the geofence's centre point.
    /// </summary>
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

    /// <summary>
    /// Verifies that attempting to add a geofence with invalid latitude or
    /// longitude values does not result in a geofence that can be queried.
    /// </summary>
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
