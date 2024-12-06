
# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## FuelTrackingServiceTests

The `FuelTrackingServiceTests` class provides comprehensive unit tests for the `FuelTrackingService` functionality, covering fuel event recording, retrieval, deletion, reporting, and estimation. These tests validate the correct storage of fuel records, error handling for invalid fuel amounts, filtering of records, deletion of existing records, accurate calculation of fuel reports, and proper handling of invalid inputs for fuel estimation.

Example usage in a test project:

```csharp
using Xunit;
using FluentAssertions;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;

public class FuelTrackingTests
{
    [Fact]
    public async Task TestFuelEventRecording()
    {
        // Arrange
        var service = new FuelTrackingServiceTests();
        
        // Act
        var record = await service.RecordFuelEventAsync(new FuelRecord("vehicle1", "device1", FuelEventType.Refuel, 50.0, DateTime.UtcNow));

        // Assert
        record.Id.Should().NotBeNullOrEmpty();
        record.VehicleId.Should().Be("vehicle1");
        record.FuelAmountLiters.Should().Be(50.0);
    }

    [Fact]
    public async Task TestFuelReportGeneration()
    {
        // Arrange
        var service = new FuelTrackingServiceTests();
        await service.RecordFuelEventAsync(new FuelRecord("vehicle1", "device1", FuelEventType.Consumption, 10.0, DateTime.UtcNow));
        await service.RecordFuelEventAsync(new FuelRecord("vehicle1", "device1", FuelEventType.Consumption, 5.0, DateTime.UtcNow.AddHours(1)));

        // Act
        var report = await service.GetReportAsync("vehicle1", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow);

        // Assert
        report.TotalFuelConsumedLiters.Should().Be(15.0);
        report.TotalDistanceKm.Should().Be(0); // Assuming no odometer data in this example
        report.AverageConsumptionLper100km.Should().Be(0); // Assuming no distance data
    }
}
```

<!-- (rest of README.md remains the same) -->
