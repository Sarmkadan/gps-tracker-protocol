#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using NSubstitute;
using FluentAssertions;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace gps_tracker_protocol.Tests
{
    /// <summary>
    /// Tests for the AnalyticsService class.
    /// </summary>
    public class AnalyticsServiceTests
    {
        private readonly IRepository<Journey> _journeyRepository;
        private readonly IRepository<LocationData> _locationDataRepository;
        private readonly AnalyticsService _sut; // System Under Test

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsServiceTests"/> class.
        /// </summary>
        public AnalyticsServiceTests()
        {
            _journeyRepository = Substitute.For<IRepository<Journey>>();
            _locationDataRepository = Substitute.For<IRepository<LocationData>>();
            _sut = new AnalyticsService(_journeyRepository, _locationDataRepository);
        }

        /// <summary>
        /// Verifies that the GetTotalJourneysAsync method returns the correct count.
        /// </summary>
        [Fact]
        public async Task GetTotalJourneys_ShouldReturnCorrectCount()
        {
            // Arrange
            _journeyRepository.GetAllAsync().Returns(new List<Journey>
            {
                new Journey { Id = "journey1", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-2), EndTime = DateTime.UtcNow.AddHours(-1) },
                new Journey { Id = "journey2", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-4), EndTime = DateTime.UtcNow.AddHours(-3) }
            });

            // Act
            var totalJourneys = await _sut.GetTotalJourneysAsync().ConfigureAwait(false);

            // Assert
            totalJourneys.Should().Be(2);
        }

        /// <summary>
        /// Verifies that the GetTotalJourneysAsync method returns 0 when no journeys exist.
        /// </summary>
        [Fact]
        public async Task GetTotalJourneys_ShouldReturnZero_WhenNoJourneysExist()
        {
            // Arrange
            _journeyRepository.GetAllAsync().Returns(new List<Journey>());

            // Act
            var totalJourneys = await _sut.GetTotalJourneysAsync().ConfigureAwait(false);

            // Assert
            totalJourneys.Should().Be(0);
        }

        /// <summary>
        /// Verifies that the GetAverageJourneyDurationAsync method returns the correct average duration.
        /// </summary>
        [Fact]
        public async Task GetAverageJourneyDuration_ShouldReturnCorrectAverage()
        {
            // Arrange
            _journeyRepository.GetAllAsync().Returns(new List<Journey>
            {
                new Journey { Id = "journey1", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-2), EndTime = DateTime.UtcNow.AddHours(-1) }, // 1 hour
                new Journey { Id = "journey2", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-4), EndTime = DateTime.UtcNow.AddHours(-2) }  // 2 hours
            });

            // Act
            var averageDuration = await _sut.GetAverageJourneyDurationAsync().ConfigureAwait(false);

            // Assert
            averageDuration.Should().Be(TimeSpan.FromHours(1.5));
        }

        /// <summary>
        /// Verifies that the GetAverageJourneyDurationAsync method returns 0 when no journeys exist.
        /// </summary>
        [Fact]
        public async Task GetAverageJourneyDuration_ShouldReturnZero_WhenNoJourneysExist()
        {
            // Arrange
            _journeyRepository.GetAllAsync().Returns(new List<Journey>());

            // Act
            var averageDuration = await _sut.GetAverageJourneyDurationAsync().ConfigureAwait(false);

            // Assert
            averageDuration.Should().Be(TimeSpan.Zero);
        }

        /// <summary>
        /// Verifies that the GetMostActiveDeviceAsync method returns the correct device ID.
        /// </summary>
        [Fact]
        public async Task GetMostActiveDevice_ShouldReturnCorrectDeviceId()
        {
            // Arrange
            _journeyRepository.GetAllAsync().Returns(new List<Journey>
            {
                new Journey { Id = "j1", DeviceId = "deviceA", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(30) },
                new Journey { Id = "j2", DeviceId = "deviceB", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(30) },
                new Journey { Id = "j3", DeviceId = "deviceA", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(30) }
            });

            // Act
            var mostActiveDevice = await _sut.GetMostActiveDeviceAsync().ConfigureAwait(false);

            // Assert
            mostActiveDevice.Should().Be("deviceA");
        }

        /// <summary>
        /// Verifies that the GetMostActiveDeviceAsync method returns null when no journeys exist.
        /// </summary>
        [Fact]
        public async Task GetMostActiveDevice_ShouldReturnNull_WhenNoJourneysExist()
        {
            // Arrange
            _journeyRepository.GetAllAsync().Returns(new List<Journey>());

            // Act
            var mostActiveDevice = await _sut.GetMostActiveDeviceAsync().ConfigureAwait(false);

            // Assert
            mostActiveDevice.Should().BeNull();
        }
    }
}
