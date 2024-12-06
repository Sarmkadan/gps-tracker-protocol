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

## RouteReplayServiceTests

The `RouteReplayServiceTests` class provides comprehensive unit tests for the `RouteReplayService` class, focusing on journey replay functionality. It tests various scenarios including frame counting, timestamp compression with speed multipliers, validation of journey states, waypoint slicing, and timestamp rebase operations.

These tests ensure that the route replay service correctly handles journey data and produces accurate replay frames and summaries.

Example usage in a test project:

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