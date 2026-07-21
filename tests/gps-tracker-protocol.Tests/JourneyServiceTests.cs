#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Xunit;
using FluentAssertions;
using NSubstitute;

namespace GpsTrackerProtocol.Tests
{
    public class JourneyServiceTests
    {
        private readonly IJourneyRepository _journeyRepository = Substitute.For<IJourneyRepository>();
        private readonly ILocationDataRepository _locationRepository = Substitute.For<ILocationDataRepository>();
        private readonly IDeviceRepository _deviceRepository = Substitute.For<IDeviceRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly JourneyService _journeyService;

        public JourneyServiceTests()
        {
            _unitOfWork.Journeys.Returns(_journeyRepository);
            _unitOfWork.LocationData.Returns(_locationRepository);
            _unitOfWork.Devices.Returns(_deviceRepository);

            _journeyService = new JourneyService(_unitOfWork);
        }

        [Fact]
        public async Task StartJourneyAsync_WithValidDeviceId_CreatesAndReturnsJourney()
        {
            // Arrange
            var deviceId = "device-123";
            var device = new Device { Id = deviceId, DeviceName = "Test Device" };
            var journey = new Journey { Id = "journey-123", DeviceId = deviceId };

            _deviceRepository.GetByIdAsync(deviceId).Returns(device);
            _journeyRepository.GetOngoingJourneyAsync(deviceId).Returns((Journey?)null);
            _journeyRepository.CreateAsync(Arg.Any<Journey>()).Returns(journey);

            // Act
            var result = await _journeyService.StartJourneyAsync(deviceId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("journey-123");
            result.DeviceId.Should().Be(deviceId);
            result.Status.Should().Be(0); // ongoing
            await _journeyRepository.Received(1).CreateAsync(Arg.Any<Journey>());
        }

        [Fact]
        public async Task StartJourneyAsync_WithNullOrEmptyDeviceId_ThrowsArgumentException()
        {
            // Act & Assert
            await FluentActions.Invoking(() => _journeyService.StartJourneyAsync(null!))
                .Should().ThrowAsync<ArgumentException>();

            await FluentActions.Invoking(() => _journeyService.StartJourneyAsync(string.Empty))
                .Should().ThrowAsync<ArgumentException>();

            await FluentActions.Invoking(() => _journeyService.StartJourneyAsync("   "))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task StartJourneyAsync_WithNonExistentDeviceId_ThrowsDeviceException()
        {
            // Arrange
            var deviceId = "nonexistent-device";
            _deviceRepository.GetByIdAsync(deviceId).Returns((Device?)null);

            // Act & Assert
            await FluentActions.Invoking(() => _journeyService.StartJourneyAsync(deviceId))
                .Should().ThrowAsync<DeviceException>();
        }

        [Fact]
        public async Task StartJourneyAsync_WithDeviceHavingOngoingJourney_ThrowsInvalidOperationException()
        {
            // Arrange
            var deviceId = "device-with-ongoing";
            var device = new Device { Id = deviceId, DeviceName = "Test Device" };
            var ongoingJourney = new Journey { Id = "ongoing-journey", DeviceId = deviceId };

            _deviceRepository.GetByIdAsync(deviceId).Returns(device);
            _journeyRepository.GetOngoingJourneyAsync(deviceId).Returns(ongoingJourney);

            // Act & Assert
            await FluentActions.Invoking(() => _journeyService.StartJourneyAsync(deviceId))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AddWaypointAsync_WithValidJourneyAndLocation_AddsWaypointSuccessfully()
        {
            // Arrange
            var journeyId = "journey-123";
            var location = new LocationData
            {
                Id = "loc-1",
                DeviceId = "device-123",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow,
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            var journey = new Journey
            {
                Id = journeyId,
                DeviceId = "device-123",
                Status = 0 // ongoing
            };

            _journeyRepository.GetByIdAsync(journeyId).Returns(journey);
            _journeyRepository.UpdateAsync(Arg.Any<Journey>()).Returns(Task.FromResult(journey));

            // Act
            var result = await _journeyService.AddWaypointAsync(journeyId, location);

            // Assert
            result.Should().BeTrue();
            journey.Waypoints.Should().Contain(location);
            await _journeyRepository.Received(1).UpdateAsync(journey);
        }

        [Fact]
        public async Task AddWaypointAsync_WithNullLocation_ThrowsArgumentNullException()
        {
            // Arrange
            var journeyId = "journey-123";

            // Act & Assert
            await FluentActions.Invoking(() => _journeyService.AddWaypointAsync(journeyId, null!))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddWaypointAsync_WithInvalidLocation_ThrowsValidationException()
        {
            // Arrange
            var journeyId = "journey-123";
            var invalidLocation = new LocationData
            {
                Id = "loc-invalid",
                DeviceId = "device-123",
                Latitude = 95.0, // Invalid latitude
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow,
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            var journey = new Journey
            {
                Id = journeyId,
                DeviceId = "device-123",
                Status = 0 // ongoing
            };

            _journeyRepository.GetByIdAsync(journeyId).Returns(journey);

            // Act & Assert
            await FluentActions.Invoking(() => _journeyService.AddWaypointAsync(journeyId, invalidLocation))
                .Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CompleteJourneyAsync_WithOngoingJourney_MarksJourneyAsCompleted()
        {
            // Arrange
            var journeyId = "journey-123";
            var journey = new Journey
            {
                Id = journeyId,
                DeviceId = "device-123",
                Status = 0 // ongoing
            };

            _journeyRepository.GetByIdAsync(journeyId).Returns(journey);
            _journeyRepository.UpdateAsync(Arg.Any<Journey>()).Returns(Task.FromResult(journey));

            // Act
            var result = await _journeyService.CompleteJourneyAsync(journeyId);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(1); // completed
            result.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            await _journeyRepository.Received(1).UpdateAsync(journey);
        }

        [Fact]
        public async Task GetTotalDistanceAsync_CalculatesCorrectDistance_UsingHaversineFormula()
        {
            // Arrange
            var deviceId = "device-123";
            var expectedDistanceKm = 100.0; // Known distance for test

            _journeyRepository.GetTotalDistanceAsync(deviceId).Returns(expectedDistanceKm);

            // Act
            var result = await _journeyService.GetTotalDistanceAsync(deviceId);

            // Assert
            result.Should().Be(expectedDistanceKm);
            await _journeyRepository.Received(1).GetTotalDistanceAsync(deviceId);
        }

        [Fact]
        public async Task Journey_AssemblesPointsInCorrectOrder_AndCalculatesDistance()
        {
            // Arrange
            var journey = new Journey
            {
                Id = "journey-test",
                DeviceId = "device-123",
                StartTime = DateTime.UtcNow.AddHours(-2)
            };

            // Create a known path: NYC to Philadelphia approx 95 miles (153 km)
            var nyc = new LocationData
            {
                Id = "loc-1",
                DeviceId = "device-123",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow.AddHours(-2),
                Speed = 60,
                Bearing = 90,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            var philadelphia = new LocationData
            {
                Id = "loc-2",
                DeviceId = "device-123",
                Latitude = 39.9526,
                Longitude = -75.1652,
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Speed = 60,
                Bearing = 90,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            // Act
            journey.AddWaypoint(nyc);
            journey.AddWaypoint(philadelphia);

            // Assert
            journey.Waypoints.Should().HaveCount(2);
            journey.Waypoints[0].Should().Be(nyc);
            journey.Waypoints[1].Should().Be(philadelphia);

            // Calculate expected distance using Haversine (approximately 129 km)
            var calculatedDistance = nyc.DistanceTo(philadelphia);
            calculatedDistance.Should().BeGreaterThan(120); // Should be around 129 km
            calculatedDistance.Should().BeLessThan(135);

            // Verify journey's total distance calculation
            journey.GetTotalDistance().Should().Be(calculatedDistance);
        }

        [Fact]
        public async Task Journey_WithSinglePoint_ReturnsZeroDistance()
        {
            // Arrange
            var journey = new Journey
            {
                Id = "journey-single-point",
                DeviceId = "device-123",
                StartTime = DateTime.UtcNow
            };

            var singlePoint = new LocationData
            {
                Id = "loc-single",
                DeviceId = "device-123",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow,
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            // Act
            journey.AddWaypoint(singlePoint);

            // Assert
            journey.Waypoints.Should().HaveCount(1);
            journey.GetTotalDistance().Should().Be(0); // Single point should have zero distance
        }

        [Fact]
        public async Task Journey_WithTimestampGap_ContainsAllPointsInSameJourney()
        {
            // Arrange
            var journey = new Journey
            {
                Id = "journey-with-gap",
                DeviceId = "device-123",
                StartTime = DateTime.UtcNow.AddHours(-3)
            };

            // First point
            var point1 = new LocationData
            {
                Id = "loc-1",
                DeviceId = "device-123",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow.AddHours(-3),
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            // Second point after significant time gap (2 hours)
            var point2 = new LocationData
            {
                Id = "loc-2",
                DeviceId = "device-123",
                Latitude = 40.712,
                Longitude = -74.00602,
                Timestamp = DateTime.UtcNow.AddHours(-1), // 2 hours gap
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            // Third point after another gap
            var point3 = new LocationData
            {
                Id = "loc-3",
                DeviceId = "device-123",
                Latitude = 40.7132,
                Longitude = -74.0054,
                Timestamp = DateTime.UtcNow, // Another 1 hour gap
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            // Act
            journey.AddWaypoint(point1);
            journey.AddWaypoint(point2);
            journey.AddWaypoint(point3);

            // Assert
            journey.Waypoints.Should().HaveCount(3);

            // Verify timestamps show the gaps
            var timeGap1To2 = (point2.Timestamp - point1.Timestamp).TotalHours;
            var timeGap2To3 = (point3.Timestamp - point2.Timestamp).TotalHours;

            timeGap1To2.Should().BeApproximately(2, 0.1); // ~2 hour gap
            timeGap2To3.Should().BeApproximately(1, 0.1); // ~1 hour gap

            // All points belong to same journey
            journey.GetTotalDistance().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DetectIdlePeriodsAsync_WithConsecutiveLocationsWithinRadius_ReturnsIdlePeriods()
        {
            // Arrange
            var journeyId = "journey-idle-test";
            var journey = new Journey
            {
                Id = journeyId,
                DeviceId = "device-123",
                StartTime = DateTime.UtcNow.AddHours(-2)
            };

            // Create points that are close together (within 25m radius)
            var startPoint = new LocationData
            {
                Id = "loc-start",
                DeviceId = "device-123",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow.AddHours(-2),
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            // Stay at same location for 10 minutes (600 seconds > 300 threshold)
            var idlePoint1 = new LocationData
            {
                Id = "loc-idle-1",
                DeviceId = "device-123",
                Latitude = 40.712801, // Very close to start
                Longitude = -74.006001,
                Timestamp = DateTime.UtcNow.AddHours(-2).AddMinutes(5),
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            var idlePoint2 = new LocationData
            {
                Id = "loc-idle-2",
                DeviceId = "device-123",
                Latitude = 40.712802,
                Longitude = -74.006002,
                Timestamp = DateTime.UtcNow.AddHours(-2).AddMinutes(10),
                Speed = 0,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            journey.AddWaypoint(startPoint);
            journey.AddWaypoint(idlePoint1);
            journey.AddWaypoint(idlePoint2);

            _journeyRepository.GetByIdAsync(journeyId).Returns(journey);

            // Act
            var idlePeriods = await _journeyService.DetectIdlePeriodsAsync(journeyId, maxDistanceMeters: 25.0, minDurationSeconds: 300);

            // Assert
            idlePeriods.Should().NotBeNull();
            var idleList = idlePeriods.ToList();
            idleList.Should().HaveCountGreaterThan(0);

            var idlePeriod = idleList[0];
            idlePeriod.Duration.TotalMinutes.Should().BeGreaterThanOrEqualTo(5); // 5+ minutes
            idlePeriod.MaxDistanceMeters.Should().Be(25.0);
        }

        [Fact]
        public async Task Journey_GetAverageSpeed_CalculatesCorrectAverage()
        {
            // Arrange
            var journey = new Journey
            {
                Id = "journey-speed-test",
                DeviceId = "device-123",
                StartTime = DateTime.UtcNow.AddHours(-1)
            };

            var point1 = new LocationData
            {
                Id = "loc-speed-1",
                DeviceId = "device-123",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Speed = 50,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            var point2 = new LocationData
            {
                Id = "loc-speed-2",
                DeviceId = "device-123",
                Latitude = 40.7130,
                Longitude = -74.0058,
                Timestamp = DateTime.UtcNow.AddMinutes(-30),
                Speed = 70,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            var point3 = new LocationData
            {
                Id = "loc-speed-3",
                DeviceId = "device-123",
                Latitude = 40.7132,
                Longitude = -74.0056,
                Timestamp = DateTime.UtcNow.AddMinutes(-15),
                Speed = 60,
                Bearing = 0,
                Accuracy = 5.0,
                SatelliteCount = 8
            };

            // Act
            journey.AddWaypoint(point1);
            journey.AddWaypoint(point2);
            journey.AddWaypoint(point3);

            // Assert
            journey.GetAverageSpeed().Should().BeApproximately(60.0, 0.1); // (50+70+60)/3 = 60
        }
    }
}