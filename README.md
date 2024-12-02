# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## GpsUtilitiesTestsValidation

The `GpsUtilitiesTestsValidation` class provides a set of utility methods for validating GPS-related data. It includes methods for checking the validity of coordinates, distance, bearing, speed, zoom level, and bounding box coordinates. These methods can be used to ensure that GPS data is correct and consistent.

## GpsUtilitiesTestsExtensions

The `GpsUtilitiesTestsExtensions` class provides a set of extension methods that simplify common GPS-related assertions and calculations in unit tests. It offers helpers for creating bounding boxes, validating coordinate ranges, comparing coordinates with tolerance, computing midpoints, and checking approximate validity of coordinates.

Example usage:

```csharp
using GpsTrackerProtocol.Tests;

public class Example
{
    public void Demo()
    {
        var fixture = new GpsUtilitiesTests();

        // Create a bounding box around San Francisco
        var box = fixture.CreateBoundingBox(37.7749, -122.4194, 0.5, 0.5);
        Console.WriteLine($"Box: {box.minLat}..{box.maxLat}, {box.minLon}..{box.maxLon}");

        // Validate coordinates
        var coord = fixture.CreateCoordinate(37.7749, -122.4194);
        fixture.ShouldBeApproximately(coord.lat, coord.lon, 37.7749, -122.4194);

        // Calculate midpoint
        var mid = fixture.CalculateMidpoint(37.7749, -122.4194, 37.7849, -122.4094);
        Console.WriteLine($"Midpoint: {mid.midpointLat}, {mid.midpointLon}");

        // Check validity
        bool isValid = fixture.IsApproximatelyValidCoordinate(coord.lat, coord.lon);
        Console.WriteLine($"Coordinate valid: {isValid}");
    }
}

## FuelTrackingServiceTestsExtensions

`FuelTrackingServiceTestsExtensions` provides fluent helper methods for configuring `FuelTrackingServiceTests` instances and asserting the state of fuel records. It includes methods to create a default test instance, seed it with a collection of `FuelRecord`s, generate records for a specific vehicle over a date range, and verify the existence or non‑existence of a particular record.

Example usage:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GpsTrackerProtocol.Tests;
using GpsTrackerProtocol.Domain.Models;

public class FuelTrackingExample
{
    public static async Task DemoAsync()
    {
        // Start with a default test instance
        var test = new FuelTrackingServiceTests().CreateDefault();

        // Seed the test service with specific fuel records
        var records = new[]
        {
            new FuelRecord(
                vehicleId: "vehicle1",
                deviceId: "deviceA",
                eventType: FuelEventType.Refuel,
                amount: 30.5,
                timestamp: DateTime.UtcNow.AddHours(-2),
                OdometerKm: 1200),

            new FuelRecord(
                vehicleId: "vehicle1",
                deviceId: "deviceA",
                eventType: FuelEventType.Consumption,
                amount: 5.0,
                timestamp: DateTime.UtcNow.AddHours(-1),
                OdometerKm: 1250)
        };
        test = await test.WithRecordsAsync(records);

        // Or generate a range of records for a vehicle between two dates
        test = await test.WithVehicleRecordsAsync(
            vehicleId: "vehicle2",
            startDate: DateTime.UtcNow.AddDays(-1),
            endDate: DateTime.UtcNow,
            recordCount: 3);

        // Assert that a known record exists
        await test.AssertRecordExistsAsync(records[0].Id);

        // Assert that a non‑existent record does not exist
        await test.AssertRecordDoesNotExistAsync("nonexistent-id");
    }
}

## BatchDataImporterValidation

The `BatchDataImporterValidation` class provides a set of static methods for validating data before import operations. It includes methods for validating file paths, CSV location data, and device import parameters. These methods can be used to ensure that data is correct and consistent before importing it into the system.

Example usage:

```csharp
using System;

public class BatchDataImporterExample
{
    public void Demo()
    {
        string filePath = "/path/to/import/file.csv";
        if (BatchDataImporterValidation.IsValid(filePath))
        {
            Console.WriteLine("File path is valid");
        }
        else
        {
            var errors = BatchDataImporterValidation.Validate(filePath);
            Console.WriteLine("File path is not valid: " + string.Join(", ", errors));
        }

        string deviceId = "device123";
        double latitude = 37.7749;
        double longitude = -122.4194;
        double speed = 50.0;
        DateTime timestamp = DateTime.UtcNow;
        if (BatchDataImporterValidation.IsValid(deviceId, latitude, longitude, speed, timestamp))
        {
            Console.WriteLine("Location data is valid");
        }
        else
        {
            var errors = BatchDataImporterValidation.Validate(deviceId, latitude, longitude, speed, timestamp);
            Console.WriteLine("Location data is not valid: " + string.Join(", ", errors));
        }

        string imei = "imei1234567890";
        string deviceName = "Device Name";
        string protocol = "Protocol Name";
        if (BatchDataImporterValidation.IsValid(imei, deviceName, protocol))
        {
            Console.WriteLine("Device import parameters are valid");
        }
        else
        {
            var errors = BatchDataImporterValidation.Validate(imei, deviceName, protocol);
            Console.WriteLine("Device import parameters are not valid: " + string.Join(", ", errors));
        }
    }
}

## ILoggingPipeline

The `ILoggingPipeline` interface provides a structured logging mechanism for tracking frame processing, parsing, and storage. It allows for the creation of a logging context and the logging of various events throughout the processing pipeline.

Example usage:

```csharp
using GpsTrackerProtocol.Infrastructure;
using GpsTrackerProtocol.Domain.Models;

public class LoggingExample
{
    public void Demo(ILoggingPipeline pipeline, GpsFrame frame)
    {
        var context = pipeline.CreateContext(frame.Id, frame.DeviceId);
        pipeline.LogFrameReceived(context, frame);
        pipeline.LogParsingStarted(context, frame.Protocol);
        var location = new LocationData { Latitude = 37.7749, Longitude = -122.4194, Speed = 50.0 };
        pipeline.LogParsingCompleted(context, location);
        pipeline.LogStorageStarted(context);
        pipeline.LogStorageCompleted(context, "stored-id");
    }
}
```
```