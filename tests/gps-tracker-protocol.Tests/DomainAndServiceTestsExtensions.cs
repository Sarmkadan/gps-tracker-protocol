#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Tests;

public static class DomainAndServiceTestsExtensions
{
    /// <summary>
    /// Creates a valid LocationData instance with default values for testing.
    /// </summary>
    public static LocationData CreateValidLocation(this DomainAndServiceTests _)
    {
        return new LocationData
        {
            DeviceId = "test-device-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 60,
            Bearing = 270,
            SatelliteCount = 9
        };
    }

    /// <summary>
    /// Creates a valid Device instance with default values for testing.
    /// </summary>
    public static Device CreateValidDevice(this DomainAndServiceTests _)
    {
        return new Device
        {
            Id = "test-device-001",
            Imei = "123456789012345",
            Status = DeviceStatus.Online,
            LastSeen = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a valid Device instance with the specified IMEI for testing.
    /// </summary>
    public static Device CreateDeviceWithImei(this DomainAndServiceTests _, string imei)
    {
        return new Device
        {
            Id = "test-device-001",
            Imei = imei,
            Status = DeviceStatus.Online,
            LastSeen = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a valid GpsFrame instance with default values for testing.
    /// </summary>
    public static GpsFrame CreateValidGpsFrame(this DomainAndServiceTests _)
    {
        return new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = new byte[20],
            IsValidChecksum = true
        };
    }

    /// <summary>
    /// Creates a Device instance that is intentionally offline for testing.
    /// </summary>
    public static Device CreateOfflineDevice(this DomainAndServiceTests _, TimeSpan staleThreshold)
    {
        return new Device
        {
            Id = "test-device-001",
            Imei = "123456789012345",
            Status = DeviceStatus.Offline,
            LastSeen = DateTime.UtcNow.Subtract(staleThreshold).AddMinutes(-1)
        };
    }

    /// <summary>
    /// Creates a LocationData instance with invalid coordinates for testing.
    /// </summary>
    public static LocationData CreateInvalidLocation(this DomainAndServiceTests _, double latitude, double longitude)
    {
        return new LocationData
        {
            DeviceId = "test-device-001",
            Latitude = latitude,
            Longitude = longitude,
            Speed = 60,
            Bearing = 270,
            SatelliteCount = 9
        };
    }

    /// <summary>
    /// Creates a LocationData instance with the specified bearing for testing.
    /// </summary>
    public static LocationData CreateLocationWithBearing(this DomainAndServiceTests _, double bearing)
    {
        return new LocationData
        {
            DeviceId = "test-device-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 60,
            Bearing = bearing,
            SatelliteCount = 9
        };
    }

    /// <summary>
    /// Creates a LocationData instance with the specified speed for testing.
    /// </summary>
    public static LocationData CreateLocationWithSpeed(this DomainAndServiceTests _, double speed)
    {
        return new LocationData
        {
            DeviceId = "test-device-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = speed,
            Bearing = 270,
            SatelliteCount = 9
        };
    }

    /// <summary>
    /// Creates a Device instance with the specified network information for testing.
    /// </summary>
    public static Device CreateDeviceWithNetworkInfo(this DomainAndServiceTests _, string ipAddress, int port)
    {
        return new Device
        {
            Id = "test-device-001",
            Imei = "123456789012345",
            Status = DeviceStatus.Online,
            IpAddress = ipAddress,
            Port = port,
            LastSeen = DateTime.UtcNow
        };
    }
}
