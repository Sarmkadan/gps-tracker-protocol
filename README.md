# GPS Tracker Protocol

## GpsUtilitiesTests

The `GpsUtilitiesTests` class provides a comprehensive set of unit tests for the `GpsUtilities` class, covering various methods such as calculating distances, bearings, and coordinates, as well as converting between different units. These tests ensure that the `GpsUtilities` class behaves correctly and accurately.

Example usage in a test project:

```csharp
using Xunit;
using GpsTrackerProtocol.Utilities;

public class GpsUtilitiesTestsExample
{
[Fact]
public void TestCalculateDistanceKm()
{
// Arrange
var lat1 = 51.5074;
var lon1 = -0.1278;
var lat2 = 51.5074;
var lon2 = -0.1278;

// Act
var distance = GpsUtilities.CalculateDistanceKm(lat1, lon1, lat2, lon2);

// Assert
distance.Should().Be(0);
}

[Fact]
public void TestIsValidCoordinate()
{
// Arrange
var lat = 90;
var lon = 180;

// Act
var isValid = GpsUtilities.IsValidCoordinate(lat, lon);

// Assert
isValid.Should().BeTrue();
}
}
```

## ExtensionsTests

The `ExtensionsTests` class provides comprehensive unit tests for extension methods in the `GpsTrackerProtocol.Utilities` namespace. It covers byte array conversions, string manipulations, and date/time operations, ensuring that extension methods behave correctly across different scenarios.

These tests validate functionality for hexadecimal conversions, big-endian parsing, checksum calculations, string validations, and date/time utilities.

Example usage in a test project:

```csharp
using Xunit;
using FluentAssertions;
using GpsTrackerProtocol.Utilities;

public class ExtensionsExample
{
[Fact]
public void Example_ByteArrayConversions()
{
// Arrange
var data = new byte[] { 0x78, 0x78, 0x0D };

// Act & Assert
// Convert byte array to hex string
data.ToHexString().Should().Be("78780D");

// Convert byte array to hex string with spaces
data.ToHexString(addSpaces: true).Should().Be("78-78-0D");

// Parse big-endian 16-bit unsigned integer from byte array
var bigEndianData = new byte[] { 0x01, 0x02, 0x03 };
bigEndianData.ToUInt16BigEndian(0).Should().Be(258); // (0x01 << 8) | 0x02

// Calculate XOR checksum over byte range
var checksumData = new byte[] { 0x01, 0x02, 0x03 };
checksumData.CalculateXorChecksum(0, 3).Should().Be(0x00); // 0x01 ^ 0x02 ^ 0x03

// Find sequence in byte array
var searchData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
searchData.IndexOfSequence(new byte[] { 0x02, 0x03 }).Should().Be(1);

// Copy byte range
searchData.CopyRange(1, 2).Should().Equal(new byte[] { 0x02, 0x03 });

// Convert byte range to ASCII string
var asciiData = System.Text.Encoding.ASCII.GetBytes("GT06");
asciiData.ToAsciiString(0, 4).Should().Be("GT06");
}

[Fact]
public void Example_StringValidations()
{
// Arrange
var imei = "123456789012345";
var deviceId = "device-001_test";
var invalidDeviceId = "device@001";

// Act & Assert
// Validate IMEI (15 digits)
imei.IsValidImei().Should().BeTrue();

// Validate device ID (alphanumeric with dashes and underscores)
deviceId.IsValidDeviceId().Should().BeTrue();

// Sanitize device ID (removes invalid characters)
invalidDeviceId.SanitizeDeviceId().Should().Be("device001");

// Convert hex string to byte array
"7878".HexToByteArray().Should().Equal(new byte[] { 0x78, 0x78 });
}

[Fact]
public void Example_DateTimeUtilities()
{
// Arrange
var now = DateTime.UtcNow;
var fiveMinutesAgo = now.AddMinutes(-5);

// Act & Assert
// Convert to Unix timestamp
now.ToUnixTimestamp().Should().BeGreaterThan(0);

// Convert from Unix timestamp
var timestamp = now.ToUnixTimestamp();
DateTimeExtensions.FromUnixTimestamp(timestamp).Should().BeCloseTo(now, TimeSpan.FromSeconds(1));

// Round date/time to nearest boundary
var rounded = now.RoundDown(TimeSpan.FromMinutes(5));
rounded.Minute % 5.Should().Be(0);

// Get relative time string
fiveMinutesAgo.ToRelativeTime().Should().Be("5 minutes ago");
}
}
```

## RouteReplayServiceTests

The `RouteReplayServiceTests` class provides comprehensive unit tests for the `RouteReplayService` class, focusing on journey replay functionality. It tests various scenarios including frame counting, timestamp compression with speed multipliers, validation of journey states, waypoint slicing, and timestamp rebase operations.

These tests ensure that the route replay service correctly handles journey data and produces accurate replay frames and summaries.

Example usage in a test project:

## IGeocodingService

The `IGeocodingService` interface provides functionality for reverse geocoding, enabling the conversion of geographic coordinates (latitude and longitude) into readable addresses. It supports looking up location details, such as the city and country, and can check if specific coordinates fall within a given region.

Example usage in a service:

```csharp
using GpsTrackerProtocol.Integration;

public class GeocodingServiceExample
{
    private readonly IGeocodingService _geocodingService;

    public GeocodingServiceExample(IGeocodingService geocodingService)
    {
        _geocodingService = geocodingService;
    }

    public async Task ProcessLocationAsync(double lat, double lon)
    {
        // Perform reverse geocoding
        GeocodingResult result = await _geocodingService.ReverseGeocodeAsync(lat, lon);

        if (result.Success)
        {
            Console.WriteLine($"Location: {result.DisplayName}");
            Console.WriteLine($"Address: {result.Address}, {result.City}, {result.Country}");
        }

        // Check if coordinates are in the 'UK' region
        bool isInRegion = await _geocodingService.IsInRegionAsync(lat, lon, "UK");
        Console.WriteLine($"Is in UK: {isInRegion}");
    }
}
```

## INotificationService

The `INotificationService` interface defines a contract for sending and managing notifications related to GPS tracker device events such as speeding alerts, geofence boundary breaches, and device offline status. It provides methods to send different types of alerts and retrieve notification history for devices.

Example usage in a service:

```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Integration;

public class NotificationServiceExample
{
    private readonly INotificationService _notificationService;

    public NotificationServiceExample(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task HandleSpeedingAlertAsync(string deviceId, double currentSpeed, double speedLimit)
    {
        // Send speeding alert notification
        await _notificationService.SendSpeedingAlertAsync(
            deviceId,
            currentSpeed,
            speedLimit,
            "Speeding detected on device!"
        );

        // Retrieve recent notifications for this device
        var recentNotifications = _notificationService
            .GetNotifications(deviceId)
            .Where(n => !n.IsRead && n.Type == NotificationType.SpeedingAlert)
            .OrderByDescending(n => n.Timestamp)
            .ToList();

        // Mark notification as read
        if (recentNotifications.Any())
        {
            _notificationService.MarkAsRead(recentNotifications.First().Id);
        }
    }

    public async Task HandleGeofenceAlertAsync(string deviceId, string geofenceName, bool isInside)
    {
        // Send geofence alert notification
        await _notificationService.SendGeofenceAlertAsync(
            deviceId,
            geofenceName,
            isInside,
            "Geofence boundary crossed"
        );
    }

    public async Task HandleDeviceOfflineAsync(string deviceId, TimeSpan offlineDuration)
    {
        // Send offline alert notification
        await _notificationService.SendOfflineAlertAsync(
            deviceId,
            offlineDuration,
            "Device has been offline for an extended period"
        );
    }
}
```

```csharp
using Xunit;
using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Services;

public class RouteReplayServiceTestsExample
{
[Fact]
public async Task Example_ReplayJourneyAsync_WithBasicJourney()
{
// Arrange
var journey = new Journey
{
Id = "journey-123",
DeviceId = "device-abc",
Status = 1, // Completed journey
StartTime = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc),
EndTime = new DateTime(2024, 6, 1, 11, 0, 0, DateTimeKind.Utc)
};

// Add waypoints every 5 minutes
var baseTime = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
for (var i = 0; i < 5; i++)
{
journey.Waypoints.Add(new LocationData
{
DeviceId = "device-abc",
Latitude = 51.5 + i * 0.01,
Longitude = -0.1 + i * 0.01,
Speed = 50,
Bearing = 90,
Timestamp = baseTime.AddMinutes(i * 5)
});
}

var repo = Substitute.For<IJourneyRepository>();
repo.GetByIdAsync("journey-123").Returns(journey);

var uow = Substitute.For<IUnitOfWork>();
uow.Journeys.Returns(repo);

var logger = Substitute.For<ILogger<RouteReplayService>>();
var service = new RouteReplayService(uow, logger);

// Act
var result = await service.ReplayJourneyAsync("journey-123");

// Assert
result.Frames.Should().HaveCount(5);
result.JourneyId.Should().Be("journey-123");
result.OriginalDuration.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(1));
}

[Fact]
public async Task Example_ReplayJourneyAsync_WithSpeedMultiplier()
{
// Arrange
var journey = new Journey
{
Id = "journey-456",
DeviceId = "device-xyz",
Status = 1,
StartTime = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc),
EndTime = new DateTime(2024, 6, 1, 11, 30, 0, DateTimeKind.Utc)
};

var baseTime = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
for (var i = 0; i < 4; i++)
{
journey.Waypoints.Add(new LocationData
{
DeviceId = "device-xyz",
Latitude = 52.0 + i * 0.01,
Longitude = 0.5 + i * 0.01,
Speed = 60,
Bearing = 45,
Timestamp = baseTime.AddMinutes(i * 10)
});
}

var repo = Substitute.For<IJourneyRepository>();
repo.GetByIdAsync("journey-456").Returns(journey);

var uow = Substitute.For<IUnitOfWork>();
uow.Journeys.Returns(repo);

var logger = Substitute.For<ILogger<RouteReplayService>>();
var service = new RouteReplayService(uow, logger);

// Act - replay at 3x speed
var result = await service.ReplayJourneyAsync("journey-456", new ReplayOptions { SpeedMultiplier = 3.0 });

// Assert - 60 minutes of original data at 3x speed = 20 minutes
result.ReplayDuration.Should().BeCloseTo(TimeSpan.FromMinutes(20), TimeSpan.FromSeconds(1));
result.OriginalDuration.Should().BeCloseTo(TimeSpan.FromMinutes(60), TimeSpan.FromSeconds(1));
}

[Fact]
public async Task Example_GetReplaySummaryAsync()
{
// Arrange
var journey = new Journey
{
Id = "journey-789",
DeviceId = "device-def",
Status = 1,
StartTime = new DateTime(2024, 6, 1, 14, 0, 0, DateTimeKind.Utc),
EndTime = new DateTime(2024, 6, 1, 15, 0, 0, DateTimeKind.Utc)
};

var baseTime = new DateTime(2024, 6, 1, 14, 0, 0, DateTimeKind.Utc);
for (var i = 0; i < 10; i++)
{
journey.Waypoints.Add(new LocationData
{
DeviceId = "device-def",
Latitude = 48.8 + i * 0.005,
Longitude = 2.3 + i * 0.005,
Speed = 40,
Bearing = 180,
Timestamp = baseTime.AddMinutes(i * 6)
});
}

var repo = Substitute.For<IJourneyRepository>();
repo.GetByIdAsync("journey-789").Returns(journey);

var uow = Substitute.For<IUnitOfWork>();
uow.Journeys.Returns(repo);

var logger = Substitute.For<ILogger<RouteReplayService>>();
var service = new RouteReplayService(uow, logger);

// Act
var summary = await service.GetReplaySummaryAsync("journey-789");

// Assert
summary.JourneyId.Should().Be("journey-789");
summary.WaypointCount.Should().Be(10);
summary.Duration.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(1));
}
}
```
