# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## IValidator

The `IValidator` interface defines a contract for validating domain models like `GpsFrame`, `LocationData`, and `Device`. It works with the `ValidationPipeline` class to perform type-specific validation and collect errors. The pipeline supports validating GPS frames, location data, and device records, returning structured validation results.

Example usage:

```csharp
using GpsTrackerProtocol.Infrastructure;
using GpsTrackerProtocol.Domain.Models;

public class ValidationExample
{
    public void Demo()
    {
        // Create a validation pipeline with concrete validators
        var pipeline = new ValidationPipeline(
            new FrameValidator(),
            new LocationValidator(),
            new DeviceValidator());

        // Validate a GPS frame
        var frame = new GpsFrame
        {
            FrameId = "frame123",
            RawData = new byte[] { 0x01, 0x02, 0x03 },
            Protocol = ProtocolType.GPRMC,
            ReceivedAt = DateTime.UtcNow
        };

        var frameResult = pipeline.ValidateFrame(frame);
        if (!frameResult.IsValid)
        {
            Console.WriteLine("Frame validation failed: " + frameResult.GetErrorMessage());
        }

        // Validate a location record
        var location = new LocationData
        {
            DeviceId = "device456",
            Latitude = 37.7749,
            Longitude = -122.4194,
            Speed = 50.0,
            Bearing = 90.0,
            SatelliteCount = 8,
            Timestamp = DateTime.UtcNow
        };

        var locationResult = pipeline.ValidateLocation(location);
        if (!locationResult.IsValid)
        {
            Console.WriteLine("Location validation failed: " + locationResult.GetErrorMessage());
        }

        // Validate a device
        var device = new Device
        {
            Id = "device789",
            Imei = "123456789012345",
            Protocol = ProtocolType.GPRMC
        };

        var deviceResult = pipeline.ValidateDevice(device);
        if (!deviceResult.IsValid)
        {
            Console.WriteLine("Device validation failed: " + deviceResult.GetErrorMessage());
        }
    }
}
```

The `ValidationResult` class provides `IsValid` to check validation status and `Errors` to access detailed validation messages. The `GetErrorMessage()` method returns a semicolon-separated string of all validation errors.
