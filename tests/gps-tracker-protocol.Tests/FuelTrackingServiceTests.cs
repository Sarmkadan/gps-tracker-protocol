namespace GpsTrackerProtocol.Tests;

using Xunit;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Domain;

public class FuelTrackingServiceTests
{
    private readonly FuelTrackingService _service;
    private readonly ILogger<FuelTrackingService> _logger;

    public FuelTrackingServiceTests()
    {
        _logger = Substitute.For<ILogger<FuelTrackingService>>();
        _service = new FuelTrackingService(_logger);
    }

    [Fact]
    public async Task RecordFuelEventAsync_ShouldStoreRecordSuccessfully()
    {
        // Arrange
        var record = new FuelRecord("vehicle1", "device1", FuelEventType.Refuel, 50.0, DateTime.UtcNow);

        // Act
        var result = await _service.RecordFuelEventAsync(record);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
        result.VehicleId.Should().Be("vehicle1");
        result.FuelAmountLiters.Should().Be(50.0);
    }

    [Fact]
    public async Task RecordFuelEventAsync_ShouldThrowException_WhenFuelAmountIsZeroOrNegative()
    {
        // Arrange
        var record = new FuelRecord("vehicle1", "device1", FuelEventType.Consumption, 0.0, DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.RecordFuelEventAsync(record));
    }

    [Fact]
    public async Task GetRecordsAsync_ShouldReturnFilteredRecords()
    {
        // Arrange
        var vehicleId = "vehicle1";
        await _service.RecordFuelEventAsync(new FuelRecord(vehicleId, "device1", FuelEventType.Refuel, 50.0, DateTime.UtcNow));
        await _service.RecordFuelEventAsync(new FuelRecord(vehicleId, "device1", FuelEventType.Consumption, 10.0, DateTime.UtcNow));
        await _service.RecordFuelEventAsync(new FuelRecord("otherVehicle", "device1", FuelEventType.Consumption, 10.0, DateTime.UtcNow));

        // Act
        var records = await _service.GetRecordsAsync(vehicleId, FuelEventType.Refuel);

        // Assert
        records.Should().ContainSingle();
        records.First().EventType.Should().Be(FuelEventType.Refuel);
    }

    [Fact]
    public async Task DeleteRecordAsync_ShouldReturnTrue_WhenRecordExists()
    {
        // Arrange
        var record = await _service.RecordFuelEventAsync(new FuelRecord("vehicle1", "device1", FuelEventType.Refuel, 50.0, DateTime.UtcNow));

        // Act
        var result = await _service.DeleteRecordAsync(record.Id);

        // Assert
        result.Should().BeTrue();
        var records = await _service.GetRecordsAsync("vehicle1");
        records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReportAsync_ShouldCalculateCorrectTotals()
    {
        // Arrange
        var vehicleId = "vehicle1";
        var now = DateTime.UtcNow;
        await _service.RecordFuelEventAsync(new FuelRecord(vehicleId, "device1", FuelEventType.Consumption, 10.0, now.AddHours(-2), OdometerKm: 100));
        await _service.RecordFuelEventAsync(new FuelRecord(vehicleId, "device1", FuelEventType.Consumption, 5.0, now.AddHours(-1), OdometerKm: 200));

        // Act
        var report = await _service.GetReportAsync(vehicleId, now.AddHours(-3), now);

        // Assert
        report.TotalFuelConsumedLiters.Should().Be(15.0);
        report.TotalDistanceKm.Should().Be(100.0);
        report.AverageConsumptionLper100km.Should().Be(15.0); // (15 / 100) * 100
    }

    [Fact]
    public void EstimateFuelLiters_ShouldReturnZero_WhenInputsAreInvalid()
    {
        // Act
        var result = _service.EstimateFuelLiters(0, 10);
        var result2 = _service.EstimateFuelLiters(100, 0);

        // Assert
        result.Should().Be(0);
        result2.Should().Be(0);
    }
}
