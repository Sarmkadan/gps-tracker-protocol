
## ProtocolBenchmarks

The `ProtocolBenchmarks` class provides comprehensive benchmarking and performance testing capabilities for GPS tracker protocol parsing and processing. It includes methods for setting up test environments, parsing various protocol frames (GT06, H02, TK103), protocol detection, frame validation, location data storage and retrieval, batch processing, and generating analytics reports for device and fleet performance.

This class is essential for performance testing and optimization of protocol handling code, allowing developers to measure parsing speed, validate correctness, and identify bottlenecks in GPS tracker data processing pipelines.

Example usage for protocol benchmarking:

```csharp
using GpsTrackerProtocol.Benchmarks;
using GpsTrackerProtocol.Domain.Models;

public class ProtocolBenchmarksExample
{
    private readonly ProtocolBenchmarks _benchmarks;

    public ProtocolBenchmarksExample(ProtocolBenchmarks benchmarks)
    {
        _benchmarks = benchmarks;
    }

    public async Task RunProtocolBenchmarksAsync()
    {
        // Setup benchmark environment
        _benchmarks.Setup();

        // Register a test device
        await _benchmarks.Register_Device("test-device-001", "GT06", "123456789012345");

        // Parse individual protocol frames
        var gt06Frame = new byte[] { 0x78, 0x78, 0x0D, 0x01, 0x02, 0x03, 0x04 };
        await _benchmarks.Parse_GT06_Frame(gt06Frame);

        var h02Frame = new byte[] { 0x79, 0x79, 0x0D, 0xA0, 0x01, 0x02, 0x03 };
        await _benchmarks.Parse_H02_Frame(h02Frame);

        var tk103Frame = new byte[] { 0x24, 0x47, 0x50, 0x52, 0x4D, 0x43, 0x2C };
        await _benchmarks.Parse_TK103_Frame(tk103Frame);

        // Detect protocol automatically
        var protocolType = await _benchmarks.Detect_Protocol(gt06Frame);
        Console.WriteLine($"Detected protocol: {protocolType}");

        // Validate frames
        var isValid = await _benchmarks.Validate_GT06_Frame(gt06Frame);
        Console.WriteLine($"Frame validation result: {isValid}");

        // Store location data
        var locationData = new LocationData
        {
            DeviceId = "test-device-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 45.5,
            Bearing = 90,
            Timestamp = DateTime.UtcNow
        };
        await _benchmarks.Store_Location_Data(locationData);

        // Get latest location
        var latestLocation = await _benchmarks.Get_Latest_Location("test-device-001");
        Console.WriteLine($"Latest location: {latestLocation?.Latitude}, {latestLocation?.Longitude}");

        // Batch processing - parse 100 frames
        var batchFrames = Enumerable.Range(0, 100)
            .Select(i => new byte[] { 0x78, 0x78, 0x0D, (byte)(0x01 + i), (byte)(0x02 + i), (byte)(0x03 + i), (byte)(0x04 + i) })
            .ToList();
        await _benchmarks.Batch_Parse_100_Frames(batchFrames);

        // Generate analytics reports
        var deviceAnalytics = await _benchmarks.Generate_Device_Analytics("test-device-001");
        Console.WriteLine($"Device analytics - Total messages: {deviceAnalytics.TotalMessages}, Avg speed: {deviceAnalytics.AverageSpeed:F2} km/h");

        var fleetAnalytics = await _benchmarks.Generate_Fleet_Analytics(new[] { "test-device-001" });
        Console.WriteLine($"Fleet analytics - Active devices: {fleetAnalytics.ActiveDeviceCount}");

        // Cleanup after benchmark
        _benchmarks.Cleanup();
    }
}
```

## IGeoJsonFormatter

The `IGeoJsonFormatter` interface defines a contract for formatting GPS tracker location data into GeoJSON format, enabling interoperability with mapping applications and geospatial analysis tools. It provides methods to format individual locations, location collections, and tracks as valid GeoJSON objects.

Example usage for formatting location data:

```csharp
using GpsTrackerProtocol.Formatting;
using GpsTrackerProtocol.Domain.Models;

public class GeoJsonFormatterExample
{
    private readonly IGeoJsonFormatter _formatter;

    public GeoJsonFormatterExample(IGeoJsonFormatter formatter)
    {
        _formatter = formatter;
    }

    public void FormatSampleLocation()
    {
        // Create a sample location
        var location = new Location
        {
            DeviceId = "device-001",
            Latitude = 52.5200,
            Longitude = 13.4050,
            Speed = 35.2,
            Bearing = 45,
            Timestamp = DateTime.UtcNow,
            Altitude = 120.5,
            Satellites = 8,
            HDOP = 1.2
        };

        // Format as GeoJSON location
        var locationJson = _formatter.FormatLocation(location);
        Console.WriteLine(locationJson);

        // Format as GeoJSON location collection
        var locations = new List<Location> { location };
        var collectionJson = _formatter.FormatLocationCollection(locations);
        Console.WriteLine(collectionJson);

        // Format a track as GeoJSON
        var track = new Track
        {
            DeviceId = "device-001",
            Name = "Berlin to Potsdam",
            Points = new List<Location> { location }
        };
        var trackJson = _formatter.FormatTrack(track);
        Console.WriteLine(trackJson);
    }
}
```

## GpsTrackerException

The `GpsTrackerException` class serves as the base exception type for all GPS tracker protocol operations. It provides a unified exception hierarchy for handling errors across protocol parsing, device communication, command execution, and data validation. All specific exception types in the library inherit from this base class, allowing for consistent error handling patterns.

### Public Members

- `GpsTrackerException(string message)` - Initializes a new instance with the specified error message
- `GpsTrackerException(string message, Exception innerException)` - Initializes a new instance with the specified error message and inner exception

### Inherited Exception Types

The library includes several specialized exception types that inherit from `GpsTrackerException`:

- `ParseException` - Thrown when protocol parsing fails, with optional `RawData` and `Protocol` properties
- `ChecksumException` - Thrown when frame checksum validation fails, with `ExpectedChecksum` and `ActualChecksum` properties  
- `DeviceException` - Thrown when device is not found or invalid, with `DeviceId` property
- `CommandException` - Thrown when command execution fails, with `CommandId` and `ErrorCode` properties
- `ValidationException` - Thrown when data validation fails, with `FieldName` and `FieldValue` properties
- `RepositoryException` - Thrown when repository operation fails
- `TimeoutException` - Thrown when communication with device times out, with `DeviceId` and `Duration` properties

### Usage Example

```csharp
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Exceptions;

public class GpsTrackerExceptionExample
{
    private readonly IGpsTrackerProtocolParser _parser;
    private readonly IDeviceRepository _deviceRepository;
    
    public GpsTrackerExceptionExample(IGpsTrackerProtocolParser parser, IDeviceRepository deviceRepository)
    {
        _parser = parser;
        _deviceRepository = deviceRepository;
    }
    
    public async Task HandleDeviceMessageAsync(string deviceId, string rawData)
    {
        try
        {
            // Validate device exists
            var device = await _deviceRepository.GetDeviceAsync(deviceId);
            if (device == null)
            {
                throw new DeviceException($"Device {deviceId} not found", deviceId);
            }
            
            // Parse protocol message
            var message = await _parser.ParseAsync(rawData);
            
            // Validate required fields
            if (string.IsNullOrEmpty(message.Latitude))
            {
                throw new ValidationException(
                    "Latitude is required",
                    nameof(message.Latitude),
                    message.Latitude
                );
            }
            
            // Process command
            var result = await _deviceRepository.ExecuteCommandAsync(deviceId, message.Command);
            if (!result.Success)
            {
                throw new CommandException(
                    $"Command {result.CommandId} failed with error code {result.ErrorCode}",
                    result.CommandId,
                    result.ErrorCode
                );
            }
        }
        catch (ChecksumException ex)
        {
            // Handle checksum validation failures
            Console.WriteLine($"Checksum validation failed for device {ex.DeviceId}: expected {ex.ExpectedChecksum}, got {ex.ActualChecksum}");
            await LogErrorAsync(ex);
        }
        catch (ParseException ex)
        {
            // Handle parsing errors
            Console.WriteLine($"Failed to parse protocol {ex.Protocol} data: {ex.RawData}");
            await LogErrorAsync(ex);
        }
        catch (GpsTrackerException ex) when (ex is not DeviceException and not ValidationException and not CommandException and not ChecksumException)
        {
            // Handle other GPS tracker exceptions
            Console.WriteLine($"GPS tracker protocol error: {ex.Message}");
            await LogErrorAsync(ex);
        }
    }
    
    private async Task LogErrorAsync(Exception ex)
    {
        // Implementation for error logging
    }
}
```

## IJsonFormatter

The `IJsonFormatter` interface provides methods for serializing and deserializing GPS tracker protocol messages into JSON format. It supports formatting protocol frames, raw data, and device messages for logging, storage, or transmission over APIs.

Example usage for JSON formatting and parsing:

```csharp
using GpsTrackerProtocol.Formatting;
using GpsTrackerProtocol.Domain.Models;

public class JsonFormatterExample
{
    private readonly IJsonFormatter _formatter;

    public JsonFormatterExample(IJsonFormatter formatter)
    {
        _formatter = formatter;
    }

    public void FormatProtocolMessage()
    {
        // Create a protocol message with location data
        var message = new ProtocolMessage
        {
            DeviceId = "TRK-001",
            Timestamp = DateTime.UtcNow.ToString("o"),
            Latitude = 40.7128,
            Longitude = -74.0060,
            Speed = 25.5,
            Bearing = 180,
            Altitude = 10.2,
            Accuracy = 5.8,
            SatelliteCount = 12,
            Protocol = "GT06",
            FrameId = "GT06-20240716-001",
            RawDataHex = "78780D010203040506070809",
            ReceivedAt = DateTime.UtcNow.ToString("o")
        };

        // Serialize to JSON
        string jsonOutput = _formatter.Format(message);
        Console.WriteLine(jsonOutput);

        // Deserialize back from JSON
        string jsonInput = @"{
            \"DeviceId\": \"TRK-001\",
            \"Timestamp\": \"2024-07-16T14:30:00.0000000Z\",
            \"Latitude\": 40.7128,
            \"Longitude\": -74.0060,
            \"Speed\": 25.5,
            \"Bearing\": 180,
            \"Altitude\": 10.2,
            \"Accuracy\": 5.8,
            \"SatelliteCount\": 12,
            \"Protocol\": \"GT06\",
            \"FrameId\": \"GT06-20240716-001\",
            \"RawDataHex\": \"78780D010203040506070809\",
            \"ReceivedAt\": \"2024-07-16T14:30:00.0000000Z\"
        }";

        ProtocolMessage deserialized = _formatter.Deserialize<ProtocolMessage>(jsonInput);
        Console.WriteLine($"Deserialized device: {deserialized.DeviceId}");
    }
}
```

## Command

The `Command` class represents a specific instruction or request intended for a GPS tracking device, managing its lifecycle from creation to execution and potential retries. It encapsulates all necessary metadata, parameters, and state information, including tracking acknowledgment and transmission status, to ensure reliable delivery of commands to remote hardware.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using GpsTrackerProtocol.Domain.Models;

public class CommandExample
{
    public void ManageDeviceCommand()
    {
        // Create a new command
        var command = new Command
        {
            DeviceId = "device-001",
            Type = GpsTrackerProtocol.Domain.CommandType.RequestLocation,
            Parameters = new Dictionary<string, object>(),
            Priority = 1,
            MaxRetries = 3
        };

        // Validate command structure
        if (command.IsValid())
        {
            // Execute the command
            command.Execute();
            Console.WriteLine($"Command Executed: {command.Id} at {command.ExecutedAt}");
            
            // Format command for transmission
            string formatted = command.ToFormattedCommand();
            Console.WriteLine($"Formatted Command for Device: {formatted}");
        }

        // Handle retries
        if (!command.IsAcknowledged && command.CanRetry())
        {
            Console.WriteLine($"Retrying command. Current retry count: {command.RetryCount}");
        }
    }
}
```
