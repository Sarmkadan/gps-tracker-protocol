
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

## ResponseMessage

The `ResponseMessage` class represents a response from a GPS tracking device, encapsulating acknowledgment messages, error responses, location updates, and status reports. It provides structured data parsing capabilities through the `Parse()` method which extracts relevant information based on the message type, and includes validation through the `IsValid()` method.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using GpsTrackerProtocol.Domain.Models;

public class ResponseMessageExample
{
  public void ProcessDeviceResponse()
  {
    // Create a successful acknowledgment response
    var ackResponse = new ResponseMessage
    {
      DeviceId = "TRK-001",
      Type = MessageType.Ack,
      IsSuccess = true,
      Content = "ACK,12345"
    };

    ackResponse.Parse();
    Console.WriteLine($"Ack Response: {ackResponse}");
    Console.WriteLine($"Parsed sequence: {ackResponse.ParsedData.GetValueOrDefault("sequence")}");

    // Create a location update response
    var locationResponse = new ResponseMessage
    {
      DeviceId = "TRK-002",
      Type = MessageType.LocationUpdate,
      IsSuccess = true,
      Content = "LAT,51.5074,LON,-0.1278,SPE,45.5,BRG,90,ALT,120.5"
    };

    locationResponse.Parse();
    Console.WriteLine($"Location Response: {locationResponse}");
    Console.WriteLine($"Parsed coordinates: Latitude={locationResponse.ParsedData["latitude"]}, Longitude={locationResponse.ParsedData["longitude"]}");

    // Create an error response
    var errorResponse = new ResponseMessage
    {
      DeviceId = "TRK-003",
      Type = MessageType.Error,
      IsSuccess = false,
      Content = "ERR,2"
    };

    errorResponse.Parse();
    Console.WriteLine($"Error Response: {errorResponse}");
    Console.WriteLine($"Error Code: {errorResponse.ErrorCode}, Message: {errorResponse.ErrorMessage}");

    // Create a status response
    var statusResponse = new ResponseMessage
    {
      DeviceId = "TRK-004",
      Type = MessageType.Status,
      IsSuccess = true,
      Content = "STA,78,-67,8"
    };

    statusResponse.Parse();
    Console.WriteLine($"Status Response: {statusResponse}");
    Console.WriteLine($"Battery: {statusResponse.ParsedData["battery"]}%, Signal: {statusResponse.ParsedData["signal"]}dBm, Satellites: {statusResponse.ParsedData["satellites"]}");
  }
}
```

## DeviceDiagnosticsReport

The `DeviceDiagnosticsReport` class provides comprehensive diagnostic information about a GPS tracking device's status, connectivity, and operational metrics. It tracks device health indicators including battery level, signal strength, online status, packet reception, location data collection, and self-test results to enable proactive monitoring and troubleshooting of fleet devices.

### Usage Example

```csharp
using System;
using GpsTrackerProtocol.Domain.Models;

public class DeviceDiagnosticsReportExample
{
    public void GenerateDeviceDiagnosticsReport()
    {
        // Create a diagnostics report for a device
        var report = new DeviceDiagnosticsReport
        {
            DeviceId = "TRK-001",
            DeviceName = "Vehicle GPS Tracker",
            Imei = "123456789012345",
            Protocol = ProtocolType.GT06,
            Status = DeviceStatus.Active,
            IsOnline = true,
            LastSeen = DateTime.UtcNow.AddMinutes(-2),
            TotalPacketsReceived = 1528,
            IpAddress = "192.168.1.100",
            BatteryLevel = 78,
            SignalStrength = -67,
            SignalQuality = "Good",
            TotalLocationPoints = 452,
            TotalDistanceKm = 1245.8,
            TotalJourneys = 8,
            ActiveJourneys = 2,
            GeneratedAt = DateTime.UtcNow
        };

        // Calculate time since last contact
        report.TimeSinceLastContact = DateTime.UtcNow - report.LastSeen;

        // Set last location
        report.LastLocation = new LocationData
        {
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 45.5,
            Bearing = 90,
            Timestamp = DateTime.UtcNow.AddSeconds(-30),
            Altitude = 120.5
        };

        // Set self-test result
        report.SelfTest = new DeviceSelfTestResult
        {
            TestTimestamp = DateTime.UtcNow.AddHours(-1),
            GpsModule = true,
            BatterySensor = true,
            CellularModule = true,
            MemoryStatus = "OK",
            Result = DeviceTestResult.Success
        };

        // Access report properties
        Console.WriteLine($"Device: {report.DeviceId} - {report.DeviceName}");
        Console.WriteLine($"Status: {report.Status}, Online: {report.IsOnline}");
        Console.WriteLine($"Battery: {report.BatteryLevel}%, Signal: {report.SignalStrength}dBm ({report.SignalQuality})");
        Console.WriteLine($"Last seen: {report.LastSeen:yyyy-MM-dd HH:mm:ss}, Time since contact: {report.TimeSinceLastContact.TotalSeconds:F0}s");
        Console.WriteLine($"Location points: {report.TotalLocationPoints}, Distance: {report.TotalDistanceKm:F1}km");
        Console.WriteLine($"Active journeys: {report.ActiveJourneys}/{report.TotalJourneys}");
    }
}

## GeofenceAlertRule

The `GeofenceAlertRule` class defines rules that trigger alerts when GPS tracking devices cross geofence boundaries. Rules are evaluated against each incoming location update to determine if an alert should be raised based on the specified boundary crossing type (entry, exit, or both). Each rule includes configurable properties such as cooldown periods to prevent alert spam, enabling/disabling rules, and descriptive metadata for operational clarity.

### Usage Example

```csharp
using System;
using GpsTrackerProtocol.Domain.Models;

public class GeofenceAlertRuleExample
{
    public void ConfigureGeofenceAlertRules()
    {
        // Create a rule for monitoring entry into a restricted geofence
        var entryRule = new GeofenceAlertRule
        {
            DeviceId = "TRK-001",
            GeofenceId = "restricted-zone-alpha",
            AlertType = GeofenceAlertType.Entered,
            Cooldown = TimeSpan.FromMinutes(10),
            IsEnabled = true,
            Description = "Alert when vehicle enters restricted zone Alpha",
            CreatedAt = DateTime.UtcNow
        };

        // Create a rule for monitoring exit from a secure perimeter
        var exitRule = new GeofenceAlertRule
        {
            DeviceId = "TRK-002",
            GeofenceId = "secure-perimeter-zone",
            AlertType = GeofenceAlertType.Exited,
            Cooldown = TimeSpan.FromMinutes(5),
            IsEnabled = true,
            Description = "Alert when vehicle exits secure perimeter",
            CreatedAt = DateTime.UtcNow
        };

        // Create a rule that monitors both entry and exit
        var boundaryRule = new GeofenceAlertRule
        {
            DeviceId = "TRK-003",
            GeofenceId = "construction-zone",
            AlertType = GeofenceAlertType.Entered | GeofenceAlertType.Exited,
            Cooldown = TimeSpan.FromMinutes(15),
            IsEnabled = true,
            Description = "Alert on both entry and exit from construction zone",
            CreatedAt = DateTime.UtcNow
        };

        // Disable a rule temporarily
        var temporaryRule = new GeofenceAlertRule
        {
            DeviceId = "TRK-004",
            GeofenceId = "maintenance-yard",
            AlertType = GeofenceAlertType.Entered,
            IsEnabled = false, // Rule is currently disabled
            Description = "Temporarily disabled maintenance yard monitoring"
        };

        Console.WriteLine($"Created rule {entryRule.Id} for device {entryRule.DeviceId}");
        Console.WriteLine($"Rule description: {entryRule.Description}");
        Console.WriteLine($"Cooldown period: {entryRule.Cooldown.TotalMinutes} minutes");
    }
}
```

## LocationData

The `LocationData` class represents a GPS location data point from a tracking device, containing comprehensive telemetry information including coordinates, speed, bearing, altitude, accuracy metrics, and protocol-specific details. It provides utility methods for calculating distances between locations, bearing calculations, validity checks, and string representation for logging and debugging purposes.

Example usage for creating and working with location data:

```csharp
using GpsTrackerProtocol.Domain.Models;
using System;
using System.Collections.Generic;

public class LocationDataExample
{
    public void ProcessLocationData()
    {
        // Create a new location data point from a GPS tracker device
        var location = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Altitude = 120.5,
            Speed = 45.5, // km/h
            Bearing = 90, // degrees
            Timestamp = DateTime.UtcNow,
            Accuracy = 5.8, // meters
            SatelliteCount = 12,
            Protocol = ProtocolType.GT06,
            ExtendedData = new Dictionary<string, object>
            {
                { "hdop", 1.2 },
                { "vdop", 1.5 },
                { "fix_type", "3D" },
                { "protocol_version", "2.1" }
            },
            IsValid = true
        };

        // Calculate distance to another location
        var otherLocation = new LocationData
        {
            Latitude = 51.5150,
            Longitude = -0.1300,
            Altitude = 110.0
        };

        double distance = location.DistanceTo(otherLocation);
        Console.WriteLine($"Distance between locations: {distance:F2} meters");

        // Calculate bearing to another location
        double bearing = location.BearingTo(otherLocation);
        Console.WriteLine($"Bearing to destination: {bearing:F1} degrees");

        // Access location information
        Console.WriteLine($"Location for device {location.DeviceId} at {location.Timestamp:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Coordinates: {location.Latitude:F6}, {location.Longitude:F6}");
        Console.WriteLine($"Speed: {location.Speed:F1} km/h, Bearing: {location.Bearing:F1}°");
        Console.WriteLine($"Altitude: {location.Altitude:F1} m, Accuracy: {location.Accuracy:F1} m");
        Console.WriteLine($"Satellites: {location.SatelliteCount}, Protocol: {location.Protocol}");

        // Check location validity
        if (location.IsValid)
        {
            Console.WriteLine("Location data is valid and ready for processing");
        }

        // String representation
        Console.WriteLine($"Location: {location}");
    }
}
```

## Device

The `Device` class represents a GPS tracking device that sends location data and status updates to the tracking system. It manages device identification, connection state, battery levels, signal strength, and metadata for fleet monitoring and management.

Example usage for device management:

```csharp
using GpsTrackerProtocol.Domain.Models;

public class DeviceExample
{
    public void ManageDevice()
    {
        // Create a new GPS tracking device
        var device = new Device
        {
            Id = "TRK-001",
            Imei = "123456789012345",
            DeviceName = "Vehicle GPS Tracker",
            Protocol = ProtocolType.GT06,
            Status = DeviceStatus.Active,
            LastSeen = DateTime.UtcNow.AddMinutes(-2),
            IpAddress = "192.168.1.100",
            Port = 5000,
            IsActive = true,
            BatteryLevel = 78,
            SignalStrength = -67,
            ConnectionCount = 1528,
            Metadata = new Dictionary<string, string>
            {
                { "model", "GT06" },
                { "firmware", "v2.1.4" },
                { "vehicle", "Truck-001" }
            },
            RegistrationDate = new DateTime(2024, 01, 15)
        };

        // Update device heartbeat
        device.UpdateHeartbeat("192.168.1.101", 5001);
        Console.WriteLine($"Device updated: {device}");
        Console.WriteLine($"Battery: {device.BatteryLevel}%, Signal: {device.SignalStrength}dBm");

        // Validate device configuration
        if (device.IsValid())
        {
            Console.WriteLine($"Device {device.Id} is valid and ready for tracking");
        }

        // Check if device is offline
        var isOffline = device.IsOffline(TimeSpan.FromMinutes(5));
        Console.WriteLine($"Device offline status: {isOffline}");
    }
}
```

## GpsFrame

The `GpsFrame` class represents a raw GPS protocol frame before parsing, capturing the complete data packet received from a GPS tracking device. It stores the original binary data along with metadata such as protocol type, source information, and checksum validation status. This class is essential for protocol parsing pipelines and debugging scenarios where raw frame inspection is required.

### Public Members

- `string FrameId` - Unique identifier for the frame
- `ProtocolType Protocol` - GPS protocol type (GT06, H02, TK103, etc.)
- `byte[] RawData` - Raw binary data received from the device
- `DateTime ReceivedAt` - When the frame was received
- `string SourceAddress` - IP address or identifier of the source device
- `int SourcePort` - Port number where frame was received
- `bool IsValidChecksum` - Whether frame checksum validation passed
- `string? ChecksumValue` - The extracted checksum value
- `Dictionary<string, string> Headers` - Additional frame headers/metadata
- `bool IsValid()` - Validates frame structure and integrity
- `string ToHex()` - Gets hex representation of raw data
- `byte[] ExtractBytes(int offset, int length)` - Extracts bytes from raw data
- `string ExtractString(int offset, int length, bool reverseBytes = false)` - Extracts string from raw data
- `override string ToString()` - String representation

### Usage Example

```csharp
using GpsTrackerProtocol.Domain.Models;
using System;

public class GpsFrameExample
{
    public void ProcessGpsFrame()
    {
        // Create a GPS frame from raw device data
        var frame = new GpsFrame
        {
            FrameId = "GT06-20240716-001",
            Protocol = ProtocolType.GT06,
            RawData = new byte[] { 0x78, 0x78, 0x0D, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C },
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = "192.168.1.100",
            SourcePort = 5000,
            IsValidChecksum = true,
            ChecksumValue = "A1B2",
            Headers = new Dictionary<string, string>
            {
                { "device_id", "TRK-001" },
                { "protocol_version", "2.1" },
                { "packet_type", "location_update" }
            }
        };

        // Validate frame structure
        bool isValid = frame.IsValid();
        Console.WriteLine($"Frame validation: {isValid}");

        // Get hex representation of raw data
        string hexData = frame.ToHex();
        Console.WriteLine($"Raw data (hex): {hexData}");

        // Extract specific bytes from the frame
        byte[] extractedBytes = frame.ExtractBytes(3, 4);
        Console.WriteLine($"Extracted bytes: [{string.Join(", ", extractedBytes)}]");

        // Extract string data
        string deviceId = frame.ExtractString(3, 4);
        Console.WriteLine($"Device ID: {deviceId}");

        // Access frame properties
        Console.WriteLine($"Frame received from {frame.SourceAddress}:{frame.SourcePort}");
        Console.WriteLine($"Protocol: {frame.Protocol}");
        Console.WriteLine($"Frame size: {frame.RawData.Length} bytes");
        Console.WriteLine($"Checksum valid: {frame.IsValidChecksum}");

        // String representation
        Console.WriteLine($"Frame info: {frame}");
    }
}
```

## FleetVehicle

The `FleetVehicle` record represents a fleet vehicle linked to a GPS tracking device. It extends basic device telemetry with vehicle-specific fuel consumption metrics, registration details, and manufacturer specifications. This type is central to fleet management operations, enabling fuel tracking, consumption modelling, and vehicle identification across the GPS tracking system.

Example usage for creating and managing fleet vehicles:

```csharp
using GpsTrackerProtocol.Domain.Models;

public class FleetVehicleExample
{
    public void RegisterFleetVehicle()
    {
        // Create a new fleet vehicle record for a diesel truck
        var vehicle = new FleetVehicle
        {
            Id = "FLEET-001",
            DeviceId = "TRK-DIESEL-001",
            RegistrationNumber = "ABC-123-XY",
            Make = "Volvo",
            Model = "FH16",
            Year = 2022,
            FuelType = FuelType.Diesel,
            TankCapacityLiters = 500.0,
            BaseConsumptionLper100km = 28.5,
            RegisteredAt = new DateTime(2024, 03, 15),
            Metadata = new Dictionary<string, object>
            {
                { "driver_id", "DRV-42" },
                { "cost_centre", "LOGISTICS" },
                { "vehicle_class", "Heavy Truck" },
                { "maintenance_due", new DateTime(2024, 08, 15) }
            }
        };

        // Access vehicle properties
        Console.WriteLine($"Vehicle: {vehicle.Make} {vehicle.Model} ({vehicle.Year})");
        Console.WriteLine($"Registration: {vehicle.RegistrationNumber}");
        Console.WriteLine($"Device: {vehicle.DeviceId}");
        Console.WriteLine($"Fuel type: {vehicle.FuelType}");
        Console.WriteLine($"Tank capacity: {vehicle.TankCapacityLiters} L");
        Console.WriteLine($"Base consumption: {vehicle.BaseConsumptionLper100km} L/100km");
        Console.WriteLine($"Registered: {vehicle.RegisteredAt:yyyy-MM-dd}");
        Console.WriteLine($"Metadata: {string.Join(", ", vehicle.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}")}");
    }
}
```

## LocationAggregationWorker

The `LocationAggregationWorker` class processes and aggregates GPS location data from tracking devices over specified time periods. It collects location points, calculates key metrics such as speed statistics and total distance traveled, and provides comprehensive aggregation results for fleet management and analytics purposes.

This worker is essential for generating periodic reports and insights from raw GPS data streams.

### Usage Example

```csharp
using GpsTrackerProtocol.BackgroundWorkers;
using GpsTrackerProtocol.Domain.Models;
using System;

public class LocationAggregationWorkerExample
{
    public void AggregateLocationData()
    {
        // Create a location aggregation worker for a specific device and time period
        var worker = new LocationAggregationWorker
        {
            DeviceId = "TRK-001",
            AggregationTime = DateTime.UtcNow,
            TimeSpan = TimeSpan.FromHours(1)
        };

        // Add location data points to aggregate
        worker.AddLocation(new LocationData
        {
            DeviceId = "TRK-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 45.5,
            Bearing = 90,
            Timestamp = DateTime.UtcNow.AddMinutes(-55),
            Altitude = 120.5
        });

        worker.AddLocation(new LocationData
        {
            DeviceId = "TRK-001",
            Latitude = 51.5150,
            Longitude = -0.1300,
            Speed = 52.3,
            Bearing = 120,
            Timestamp = DateTime.UtcNow.AddMinutes(-50),
            Altitude = 115.0
        });

        worker.AddLocation(new LocationData
        {
            DeviceId = "TRK-001",
            Latitude = 51.5220,
            Longitude = -0.1250,
            Speed = 38.7,
            Bearing = 150,
            Timestamp = DateTime.UtcNow.AddMinutes(-45),
            Altitude = 118.0
        });

        // Calculate aggregation metrics
        worker.CalculateMetrics();

        // Access aggregated results
        Console.WriteLine($"Device: {worker.DeviceId}");
        Console.WriteLine($"Aggregation time: {worker.AggregationTime}");
        Console.WriteLine($"Time span: {worker.TimeSpan}");
        Console.WriteLine($"Location count: {worker.LocationCount}");
        Console.WriteLine($"Max speed: {worker.MaxSpeed:F1} km/h");
        Console.WriteLine($"Min speed: {worker.MinSpeed:F1} km/h");
        Console.WriteLine($"Average speed: {worker.AverageSpeed:F1} km/h");
        Console.WriteLine($"Total distance: {worker.TotalDistance:F2} km");
    }
}
```

## ReplayOptions

The `ReplayOptions` class provides configuration for replaying recorded GPS tracker route data. It allows customization of replay behavior including speed adjustments, time rebasing, frame selection, and tracking metadata preservation. This is useful for testing, debugging, and demonstrating route playback scenarios.

### Usage Example

```csharp
using System;
using GpsTrackerProtocol.Domain.Models;

public class ReplayOptionsExample
{
public void ConfigureRouteReplay()
{
// Create replay options for a vehicle journey
var options = new ReplayOptions
{
SpeedMultiplier = 2.0, // Play at 2x speed
StartIndex = 0, // Start from first frame
EndIndex = 100, // Play first 100 frames
RebaseToUtc = DateTime.UtcNow, // Align replay to current time
Index = 42, // Current frame index
Location = new LocationData
{
DeviceId = "TRK-001",
Latitude = 51.5074,
Longitude = -0.1278,
Speed = 45.5,
Bearing = 90,
Timestamp = DateTime.UtcNow
},
ReplayTimestamp = DateTime.UtcNow,
ElapsedReplay = TimeSpan.FromMinutes(30), // 30 minutes of replay time
CumulativeDistanceKm = 15.25, // Total distance covered
JourneyId = "JOURNEY-2024-07-16-001",
DeviceId = "TRK-001",
Options = new ReplayOptions(), // Additional nested options
Frames = new List<ReplayFrame>(), // Frames to replay
TotalDistanceKm = 45.8, // Total journey distance
OriginalDuration = TimeSpan.FromHours(2), // Original journey duration
ReplayDuration = TimeSpan.FromMinutes(60), // Replay duration
GeneratedAt = DateTime.UtcNow
};

// Access replay properties
Console.WriteLine($"Journey: {options.JourneyId}");
Console.WriteLine($"Device: {options.DeviceId}");
Console.WriteLine($"Speed multiplier: {options.SpeedMultiplier}x");
Console.WriteLine($"Total distance: {options.TotalDistanceKm:F1} km");
Console.WriteLine($"Original duration: {options.OriginalDuration.TotalMinutes} minutes");
Console.WriteLine($"Replay duration: {options.ReplayDuration.TotalMinutes} minutes");
Console.WriteLine($"Frames to replay: {options.Frames.Count}");
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

## Journey

The `Journey` class represents a trip or journey containing multiple location data points collected from a GPS tracking device. It tracks the complete lifecycle of a vehicle's movement from start to finish, including waypoints, timing information, and calculated metrics like distance traveled, average speed, and duration. Journeys are useful for route analysis, fleet management, and historical reporting.

### Public Members

- `string Id` - Unique identifier for the journey
- `string DeviceId` - Identifier of the GPS device
- `DateTime StartTime` - When the journey started
- `DateTime? EndTime` - When the journey ended (null if ongoing)
- `List<LocationData> Waypoints` - Collection of location points in the journey
- `int Status` - Journey status (0: ongoing, 1: completed, 2: abandoned)
- `Dictionary<string, object> Metadata` - Additional custom data storage
- `void AddWaypoint(LocationData location)` - Adds a location to the journey
- `double GetTotalDistance()` - Calculates total distance traveled in kilometers
- `double GetAverageSpeed()` - Calculates average speed in km/h
- `double GetMaxSpeed()` - Gets maximum speed recorded in km/h
- `TimeSpan GetDuration()` - Gets journey duration
- `void Complete()` - Marks journey as completed and calculates metrics
- `override string ToString()` - String representation

### Usage Example

```csharp
using System;
using GpsTrackerProtocol.Domain.Models;

public class JourneyExample
{
  public void TrackVehicleJourney()
  {
    // Create a new journey for a vehicle
    var journey = new Journey
    {
      DeviceId = "TRK-001",
      StartTime = DateTime.UtcNow.AddMinutes(-30),
      Status = 0 // ongoing
    };

    // Add waypoints to the journey
    journey.AddWaypoint(new LocationData
    {
      DeviceId = "TRK-001",
      Latitude = 51.5074,
      Longitude = -0.1278,
      Speed = 45.5,
      Bearing = 90,
      Timestamp = DateTime.UtcNow.AddMinutes(-25),
      Altitude = 120.5,
      Accuracy = 5.8,
      SatelliteCount = 12,
      IsValid = true
    });

    journey.AddWaypoint(new LocationData
    {
      DeviceId = "TRK-001",
      Latitude = 51.5150,
      Longitude = -0.1300,
      Speed = 52.3,
      Bearing = 120,
      Timestamp = DateTime.UtcNow.AddMinutes(-20),
      Altitude = 115.0,
      Accuracy = 4.2,
      SatelliteCount = 14,
      IsValid = true
    });

    journey.AddWaypoint(new LocationData
    {
      DeviceId = "TRK-001",
      Latitude = 51.5220,
      Longitude = -0.1250,
      Speed = 0.0, // stopped
      Bearing = 0,
      Timestamp = DateTime.UtcNow.AddMinutes(-15),
      Altitude = 118.0,
      Accuracy = 3.5,
      SatelliteCount = 13,
      IsValid = true
    });

    // Calculate journey metrics
    Console.WriteLine($"Journey: {journey}");
    Console.WriteLine($"Total distance: {journey.GetTotalDistance():F2} km");
    Console.WriteLine($"Average speed: {journey.GetAverageSpeed():F1} km/h");
    Console.WriteLine($"Max speed: {journey.GetMaxSpeed():F1} km/h");
    Console.WriteLine($"Duration: {journey.GetDuration().TotalMinutes:F1} minutes");

    // Complete the journey
    journey.Complete();
    Console.WriteLine($"Journey completed at: {journey.EndTime}");
    Console.WriteLine($"Journey status: {journey.Status}");
    Console.WriteLine($"Metadata: {string.Join(", ", journey.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
  }
}
```
