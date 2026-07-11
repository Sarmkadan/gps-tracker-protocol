#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Tests;

/// <summary>
/// Provides extension methods for creating test entities with realistic default values.
/// All methods validate inputs and generate properly initialized objects suitable for unit testing.
/// </summary>
public static class DomainAndServiceTestsExtensions
{
	/// <summary>
	/// Creates a valid LocationData instance with default values for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <returns>A valid LocationData instance with realistic default values.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	public static LocationData CreateValidLocation(this DomainAndServiceTests _)
	{
		ArgumentNullException.ThrowIfNull(_);

		return new LocationData
		{
			DeviceId = "test-device-001",
			Latitude = 51.5074,
			Longitude = -0.1278,
			Altitude = 100.5,
			Speed = 60,
			Bearing = 270,
			Accuracy = 15.2,
			SatelliteCount = 9,
			Protocol = ProtocolType.GT06,
			Timestamp = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Creates a valid Device instance with default values for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <returns>A valid Device instance with realistic default values.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	public static Device CreateValidDevice(this DomainAndServiceTests _)
	{
		ArgumentNullException.ThrowIfNull(_);

		return new Device
		{
			Id = "test-device-001",
			Imei = "123456789012345",
			DeviceName = "Test Device 001",
			Protocol = ProtocolType.GT06,
			Status = DeviceStatus.Online,
			IpAddress = "192.168.1.100",
			Port = 5000,
			IsActive = true,
			BatteryLevel = 85,
			SignalStrength = -67,
			ConnectionCount = 42,
			RegistrationDate = DateTime.UtcNow.AddDays(-7),
			LastSeen = DateTime.UtcNow,
			Metadata = new Dictionary<string, string> { ["test-key"] = "test-value" }
		};
	}

	/// <summary>
	/// Creates a valid Device instance with the specified IMEI for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <param name="imei">The IMEI number to assign to the device (must be 14-16 digits).</param>
	/// <returns>A Device instance with the specified IMEI.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	/// <exception cref="ArgumentException">Thrown if the IMEI is null or empty.</exception>
	public static Device CreateDeviceWithImei(this DomainAndServiceTests _, string imei)
	{
		ArgumentNullException.ThrowIfNull(_);
		ArgumentException.ThrowIfNullOrEmpty(imei, nameof(imei));

		return new Device
		{
			Id = "test-device-001",
			Imei = imei,
			DeviceName = "Test Device 001",
			Protocol = ProtocolType.GT06,
			Status = DeviceStatus.Online,
			IpAddress = "192.168.1.100",
			Port = 5000,
			IsActive = true,
			BatteryLevel = 85,
			SignalStrength = -67,
			ConnectionCount = 42,
			RegistrationDate = DateTime.UtcNow.AddDays(-7),
			LastSeen = DateTime.UtcNow,
			Metadata = new Dictionary<string, string> { ["imei"] = imei }
		};
	}

	/// <summary>
	/// Creates a valid GpsFrame instance with default values for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <returns>A valid GpsFrame instance with realistic default values.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	public static GpsFrame CreateValidGpsFrame(this DomainAndServiceTests _)
	{
		ArgumentNullException.ThrowIfNull(_);

		var frame = new GpsFrame
		{
			FrameId = Guid.NewGuid().ToString("N"),
			Protocol = ProtocolType.GT06,
			RawData = new byte[20],
			ReceivedAt = DateTime.UtcNow,
			SourceAddress = "192.168.1.100",
			SourcePort = 5000,
			IsValidChecksum = true,
			ChecksumValue = "A1B2C3",
			Headers = new Dictionary<string, string> { ["Content-Type"] = "application/octet-stream" }
		};

		// Fill with some realistic test data
		for (int i = 0; i < frame.RawData.Length; i++)
		{
			frame.RawData[i] = (byte)(i % 256);
		}

		return frame;
	}

	/// <summary>
	/// Creates a Device instance that is intentionally offline for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <param name="staleThreshold">The time threshold after which the device is considered stale/offline.</param>
	/// <returns>A Device instance with offline status and last seen time set to stale threshold.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if staleThreshold is negative.</exception>
	public static Device CreateOfflineDevice(this DomainAndServiceTests _, TimeSpan staleThreshold)
	{
		ArgumentNullException.ThrowIfNull(_);

		if (staleThreshold < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException(nameof(staleThreshold), "Stale threshold cannot be negative.");
		}

		return new Device
		{
			Id = "test-device-001",
			Imei = "123456789012345",
			DeviceName = "Test Device 001",
			Protocol = ProtocolType.GT06,
			Status = DeviceStatus.Offline,
			IpAddress = "192.168.1.100",
			Port = 5000,
			IsActive = true,
			BatteryLevel = 85,
			SignalStrength = -67,
			ConnectionCount = 42,
			RegistrationDate = DateTime.UtcNow.AddDays(-7),
			LastSeen = DateTime.UtcNow.Subtract(staleThreshold).AddMinutes(-1),
			Metadata = new Dictionary<string, string> { ["offline-since"] = DateTime.UtcNow.ToString("O") }
		};
	}

	/// <summary>
	/// Creates a LocationData instance with invalid coordinates for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <param name="latitude">The latitude value to test with.</param>
	/// <param name="longitude">The longitude value to test with.</param>
	/// <returns>A LocationData instance with the specified coordinates.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	public static LocationData CreateInvalidLocation(this DomainAndServiceTests _, double latitude, double longitude)
	{
		ArgumentNullException.ThrowIfNull(_);

		return new LocationData
		{
			DeviceId = "test-device-001",
			Latitude = latitude,
			Longitude = longitude,
			Speed = 60,
			Bearing = 270,
			SatelliteCount = 9,
			Protocol = ProtocolType.GT06,
			Timestamp = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Creates a LocationData instance with the specified bearing for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <param name="bearing">The bearing value in degrees (0-360).</param>
	/// <returns>A LocationData instance with the specified bearing.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if bearing is outside valid range (0-360).</exception>
	public static LocationData CreateLocationWithBearing(this DomainAndServiceTests _, double bearing)
	{
		ArgumentNullException.ThrowIfNull(_);

		if (bearing < 0 || bearing > 360)
		{
			throw new ArgumentOutOfRangeException(nameof(bearing), "Bearing must be between 0 and 360 degrees.");
		}

		return new LocationData
		{
			DeviceId = "test-device-001",
			Latitude = 51.5074,
			Longitude = -0.1278,
			Altitude = 100.5,
			Speed = 60,
			Bearing = bearing,
			Accuracy = 15.2,
			SatelliteCount = 9,
			Protocol = ProtocolType.GT06,
			Timestamp = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Creates a LocationData instance with the specified speed for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <param name="speed">The speed value in km/h (must be non-negative).</param>
	/// <returns>A LocationData instance with the specified speed.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the test fixture parameter is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if speed is negative.</exception>
	public static LocationData CreateLocationWithSpeed(this DomainAndServiceTests _, double speed)
	{
		ArgumentNullException.ThrowIfNull(_);

		if (speed < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(speed), "Speed cannot be negative.");
		}

		return new LocationData
		{
			DeviceId = "test-device-001",
			Latitude = 51.5074,
			Longitude = -0.1278,
			Altitude = 100.5,
			Speed = speed,
			Bearing = 270,
			Accuracy = 15.2,
			SatelliteCount = 9,
			Protocol = ProtocolType.GT06,
			Timestamp = DateTime.UtcNow
		};
	}

	/// <summary>
	/// Creates a Device instance with the specified network information for testing.
	/// </summary>
	/// <param name="_">The test fixture instance (unused parameter for extension method syntax).</param>
	/// <param name="ipAddress">The IP address for the device connection.</param>
	/// <param name="port">The port number for the device connection.</param>
	/// <returns>A Device instance with the specified network configuration.</returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown if the test fixture parameter or ipAddress is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// Thrown if ipAddress is null or empty, or if port is outside valid range (1-65535).
	/// </exception>
	public static Device CreateDeviceWithNetworkInfo(this DomainAndServiceTests _, string ipAddress, int port)
	{
		ArgumentNullException.ThrowIfNull(_);
		ArgumentException.ThrowIfNullOrEmpty(ipAddress, nameof(ipAddress));

		if (port < 1 || port > 65535)
		{
			throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
		}

		return new Device
		{
			Id = "test-device-001",
			Imei = "123456789012345",
			DeviceName = "Test Device 001",
			Protocol = ProtocolType.GT06,
			Status = DeviceStatus.Online,
			IpAddress = ipAddress,
			Port = port,
			IsActive = true,
			BatteryLevel = 85,
			SignalStrength = -67,
			ConnectionCount = 42,
			RegistrationDate = DateTime.UtcNow.AddDays(-7),
			LastSeen = DateTime.UtcNow,
			Metadata = new Dictionary<string, string> { ["network"] = $"{ipAddress}:{port}" }
		};
	}
}