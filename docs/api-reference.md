# API Reference

Complete reference for all public interfaces and classes.

## Services

### IProtocolParserService

Parses GPS tracker protocol frames and extracts location data.

```csharp
public interface IProtocolParserService
{
    /// Detects the protocol type from raw data
    Task<ProtocolType> DetectProtocolAsync(byte[] data);
    
    /// Validates a frame for integrity and correctness
    Task<bool> ValidateFrameAsync(GpsFrame frame);
    
    /// Parses frame and returns detailed parse result
    Task<ParseResult> ParseFrameAsync(GpsFrame frame);
    
    /// Extracts location data from a frame
    Task<LocationData?> ExtractLocationDataAsync(GpsFrame frame);
    
    /// Extracts command response from a frame
    Task<CommandResponse?> ExtractCommandResponseAsync(GpsFrame frame);
}
```

**Usage**:
```csharp
var parser = services.GetRequiredService<IProtocolParserService>();

// Detect protocol
var protocol = await parser.DetectProtocolAsync(rawData);

// Validate frame
var frame = new GpsFrame { RawData = rawData, Protocol = protocol };
bool valid = await parser.ValidateFrameAsync(frame);

// Extract location
var location = await parser.ExtractLocationDataAsync(frame);
```

---

### IDeviceService

Manages GPS tracker devices - registration, status, queries.

```csharp
public interface IDeviceService
{
    /// Registers a new device
    Task<Device> RegisterDeviceAsync(Device device);
    
    /// Gets a specific device by ID
    Task<Device?> GetDeviceAsync(string deviceId);
    
    /// Gets all registered devices
    Task<IEnumerable<Device>> GetAllDevicesAsync();
    
    /// Unregisters and removes a device
    Task<bool> UnregisterDeviceAsync(string deviceId);
    
    /// Updates device information
    Task<Device> UpdateDeviceAsync(Device device);
    
    /// Gets devices by protocol type
    Task<IEnumerable<Device>> GetDevicesByProtocolAsync(ProtocolType protocol);
    
    /// Gets only active devices
    Task<IEnumerable<Device>> GetActiveDevicesAsync();
}
```

**Usage**:
```csharp
var deviceService = services.GetRequiredService<IDeviceService>();

// Register
var device = new Device 
{ 
    Imei = "358240050447491",
    DeviceName = "Truck #1",
    Protocol = ProtocolType.GT06 
};
var registered = await deviceService.RegisterDeviceAsync(device);

// Query
var allDevices = await deviceService.GetAllDevicesAsync();
var active = await deviceService.GetActiveDevicesAsync();
```

---

### ILocationDataService

Stores and retrieves GPS location data.

```csharp
public interface ILocationDataService
{
    /// Stores a new location record
    Task<LocationData> StoreLocationAsync(LocationData location);
    
    /// Gets the latest location for a device
    Task<LocationData?> GetLatestLocationAsync(string deviceId);
    
    /// Gets location history for a device
    Task<IEnumerable<LocationData>> GetLocationHistoryAsync(
        string deviceId, 
        int limit = 100
    );
    
    /// Gets locations within a date range
    Task<IEnumerable<LocationData>> GetLocationsByDateRangeAsync(
        string deviceId,
        DateTime startTime,
        DateTime endTime
    );
    
    /// Gets locations within a geographic region
    Task<IEnumerable<LocationData>> GetLocationsByRegionAsync(
        double centerLatitude,
        double centerLongitude,
        double radiusKm
    );
}
```

**Usage**:
```csharp
var locationService = services.GetRequiredService<ILocationDataService>();

// Store location
var location = new LocationData
{
    DeviceId = "device-001",
    Latitude = 40.7128,
    Longitude = -74.0060,
    Speed = 55.0
};
await locationService.StoreLocationAsync(location);

// Query latest
var latest = await locationService.GetLatestLocationAsync("device-001");

// Query history
var history = await locationService.GetLocationHistoryAsync("device-001", 100);

// Query by date range
var range = await locationService.GetLocationsByDateRangeAsync(
    "device-001",
    DateTime.Now.AddHours(-24),
    DateTime.Now
);
```

---

### IJourneyService

Tracks trips and calculates journey metrics.

```csharp
public interface IJourneyService
{
    /// Starts a new journey for a device
    Task<Journey> StartJourneyAsync(string deviceId);
    
    /// Completes a journey
    Task<Journey> CompleteJourneyAsync(string journeyId);
    
    /// Adds a waypoint to a journey
    Task AddWaypointAsync(string journeyId, LocationData waypoint);
    
    /// Gets a specific journey
    Task<Journey?> GetJourneyAsync(string journeyId);
    
    /// Gets all journeys for a device
    Task<IEnumerable<Journey>> GetJourneyHistoryAsync(string deviceId);
    
    /// Calculates total distance traveled
    Task<double> GetTotalDistanceAsync(string deviceId);
    
    /// Calculates total time on journeys
    Task<TimeSpan> GetTotalDurationAsync(string deviceId);
    
    /// Calculates average speed
    Task<double> GetAverageSpeedAsync(string deviceId);
}
```

**Usage**:
```csharp
var journeyService = services.GetRequiredService<IJourneyService>();

// Start journey
var journey = await journeyService.StartJourneyAsync("device-001");

// Add waypoints
for (int i = 0; i < 10; i++)
{
    var waypoint = new LocationData { /* ... */ };
    await journeyService.AddWaypointAsync(journey.Id, waypoint);
}

// Complete journey
var completed = await journeyService.CompleteJourneyAsync(journey.Id);

// Get analytics
var distance = await journeyService.GetTotalDistanceAsync("device-001");
var avgSpeed = await journeyService.GetAverageSpeedAsync("device-001");
```

---

### ICommandService

Manages device commands and responses.

```csharp
public interface ICommandService
{
    /// Creates a new command
    Task<Command> CreateCommandAsync(Command command);
    
    /// Executes a command
    Task<bool> ExecuteCommandAsync(string commandId);
    
    /// Gets the status of a command
    Task<CommandStatus> GetCommandStatusAsync(string commandId);
    
    /// Gets all commands for a device
    Task<IEnumerable<Command>> GetCommandHistoryAsync(string deviceId);
    
    /// Gets the response to a command
    Task<CommandResponse?> GetCommandResponseAsync(string commandId);
}
```

**Usage**:
```csharp
var commandService = services.GetRequiredService<ICommandService>();

// Create and execute
var command = new Command
{
    DeviceId = "device-001",
    Type = CommandType.SetGpsInterval,
    Parameters = new Dictionary<string, object> { { "interval", 60 } }
};

var created = await commandService.CreateCommandAsync(command);
bool executed = await commandService.ExecuteCommandAsync(created.Id);

// Get history
var history = await commandService.GetCommandHistoryAsync("device-001");
```

---

### IAnalyticsService

Analyzes device and fleet statistics.

```csharp
public interface IAnalyticsService
{
    /// Gets detailed analytics for a device
    Task<AnalyticsReport> GetDeviceAnalyticsAsync(
        string deviceId, 
        DateRange dateRange
    );
    
    /// Gets fleet-wide analytics
    Task<FleetAnalytics> GetFleetAnalyticsAsync(DateRange dateRange);
    
    /// Analyzes driving patterns
    Task<DrivingStatistics> AnalyzeDrivingPatterns(string deviceId);
    
    /// Detects anomalies and alerts
    Task<AlertsReport> DetectAnomaliesAsync(string deviceId);
}
```

---

## Domain Models

### Device

Represents a GPS tracker device.

```csharp
public class Device
{
    public string Id { get; set; }           // Unique identifier
    public string Imei { get; set; }         // International Mobile Equipment ID
    public string DeviceName { get; set; }   // User-friendly name
    public ProtocolType Protocol { get; set; }  // GPS protocol (GT06, H02, TK103)
    public DeviceStatus Status { get; set; }    // Current status
    public bool IsActive { get; set; }      // Is device actively tracking
    public DateTime RegisteredAt { get; set; }  // Registration timestamp
    public DateTime? LastUpdateAt { get; set; } // Last location update
}
```

---

### LocationData

Represents a GPS location reading.

```csharp
public class LocationData
{
    public string Id { get; set; }          // Unique identifier
    public string DeviceId { get; set; }    // Associated device
    public double Latitude { get; set; }    // -90 to 90
    public double Longitude { get; set; }   // -180 to 180
    public double Speed { get; set; }       // km/h
    public double Bearing { get; set; }     // 0-360 degrees
    public double Altitude { get; set; }    // meters
    public int SatelliteCount { get; set; } // GPS satellites
    public double Accuracy { get; set; }    // meters (GPS accuracy)
    public DateTime Timestamp { get; set; } // When this reading was taken
    public ProtocolType Protocol { get; set; }  // Protocol source
}
```

---

### Journey

Represents a trip with waypoints.

```csharp
public class Journey
{
    public string Id { get; set; }
    public string DeviceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<LocationData> Waypoints { get; set; }  // Trip waypoints
    
    // Helper methods
    public double GetTotalDistance()    // Distance in km
    public TimeSpan GetDuration()       // Trip duration
    public double GetAverageSpeed()     // km/h
    public double GetMaxSpeed()         // km/h
}
```

---

### GpsFrame

Raw protocol frame data.

```csharp
public class GpsFrame
{
    public string FrameId { get; set; }
    public ProtocolType Protocol { get; set; }
    public byte[] RawData { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? SourceAddress { get; set; }  // IP address
    public int? SourcePort { get; set; }        // Port number
    public bool IsValidChecksum { get; set; }
}
```

---

### Command

Device command.

```csharp
public class Command
{
    public string Id { get; set; }
    public string DeviceId { get; set; }
    public CommandType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; }  // Command arguments
    public CommandStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
}
```

---

## Enumerations

### ProtocolType

```csharp
public enum ProtocolType
{
    Unknown,  // Unknown protocol
    GT06,     // Quectel GT06 protocol
    H02,      // H02 protocol (NMEA/ASCII)
    TK103     // TK103 compact protocol
}
```

### DeviceStatus

```csharp
public enum DeviceStatus
{
    Unknown,       // Status unknown
    Normal,        // Device operational
    LowBattery,    // Battery low
    Offline,       // Not communicating
    Moving,        // Currently in motion
    Stationary,    // Not moving
    Alert          // Alert/alarm condition
}
```

### CommandType

```csharp
public enum CommandType
{
    Unknown,
    SetGpsInterval,      // Change GPS reporting interval
    SetAlarmThreshold,   // Set alarm trigger threshold
    EnableTracking,      // Start tracking
    DisableTracking,     // Stop tracking
    RestoreFactory,      // Reset to factory settings
    Reboot,              // Restart device
    GetStatus            // Request current status
}
```

### CommandStatus

```csharp
public enum CommandStatus
{
    Created,       // Created but not sent
    Sent,          // Sent to device
    Acknowledged,  // Device acknowledged
    Executed,      // Command executed
    Failed         // Command failed
}
```

---

## Configuration

### Constants

Access via `Constants` class:

```csharp
// Protocol markers
public const byte GT06_FRAME_START_MARKER_1 = 0x78;
public const byte GT06_FRAME_START_MARKER_2 = 0x78;

// GPS bounds
public const double MAX_LATITUDE = 90.0;
public const double MAX_LONGITUDE = 180.0;

// Timing
public const int GPS_INTERVAL_MIN = 5;
public const int GPS_INTERVAL_MAX = 3600;
public const int COMMAND_TIMEOUT_SECONDS = 30;

// Geofencing
public const double GEOFENCE_ALERT_THRESHOLD = 100.0;  // meters

// Location retention
public const int LOCATION_HISTORY_RETENTION_DAYS = 90;

// Device limits
public const int MAX_DEVICE_COUNT = 10000;
```

---

## Exception Types

```csharp
// Base exception
public class GpsTrackerException : Exception { }

// Protocol parsing errors
public class ParseException : GpsTrackerException { }
public class ChecksumException : GpsTrackerException { }

// Device-related errors
public class DeviceException : GpsTrackerException { }
public class DeviceNotFoundException : DeviceException { }

// Command errors
public class CommandException : GpsTrackerException { }

// Validation errors
public class ValidationException : GpsTrackerException { }

// Repository/data access errors
public class RepositoryException : GpsTrackerException { }
```

---

## Dependency Injection

Register services in `Program.cs` or startup:

```csharp
var services = new ServiceCollection();
services.AddGpsTrackerServices();           // Add all services
services.AddGpsTrackerLogging();            // Add logging
var provider = services.BuildServiceProvider();
```

Or register individually:

```csharp
services.AddSingleton<IProtocolParserService, ProtocolParserService>();
services.AddSingleton<IDeviceService, DeviceService>();
services.AddSingleton<ILocationDataService, LocationDataService>();
services.AddSingleton<IJourneyService, JourneyService>();
services.AddSingleton<ICommandService, CommandService>();
```

---

## Extension Methods

### ByteExtensions

```csharp
// Convert bytes to hex string
byte[] data = new byte[] { 0x78, 0x78 };
string hex = data.ToHexString();  // "7878"

// Convert hex string to bytes
byte[] decoded = "7878".FromHexString();
```

### StringExtensions

```csharp
// Check if string matches protocol pattern
bool isGps = "40.7128,-74.0060".IsValidCoordinate();

// Safe coordinate parsing
(double lat, double lng) = "40.7128,-74.0060".ParseCoordinates();
```

### DateTimeExtensions

```csharp
// UTC conversion
var utcTime = someTime.ToUtcIfNeeded();

// Format for protocols
string formatted = dateTime.FormatForGT06();
```

### GpsUtilities

```csharp
// Calculate distance between two points
double km = GpsUtilities.CalculateDistance(lat1, lng1, lat2, lng2);

// Check if point is within radius
bool inRadius = GpsUtilities.IsInRadius(centerLat, centerLng, 
    targetLat, targetLng, radiusKm);

// Convert bearing
double compassBearing = GpsUtilities.NormalizeBearing(bearing);
```

---

## Performance Tips

1. **Use pagination** for location history queries
2. **Enable caching** for frequently accessed data
3. **Use rate limiting** to protect from overload
4. **Batch operations** when possible
5. **Monitor memory** for long-running processes
6. **Use connection pooling** for database backends

---

## Migration Guide

### From Version 0.x to 1.x

- Interface names unchanged
- All methods remain compatible
- Add logging: `services.AddGpsTrackerLogging()`
- Enable caching: `services.AddCaching()`

---

## Common Patterns

### Get Location and Format

```csharp
var location = await locationService.GetLatestLocationAsync(deviceId);
if (location != null)
{
    Console.WriteLine($"{location.Latitude:F4}, {location.Longitude:F4}");
}
```

### Track a Journey

```csharp
var journey = await journeyService.StartJourneyAsync(deviceId);
// ... add waypoints ...
var completed = await journeyService.CompleteJourneyAsync(journey.Id);
var distance = completed.GetTotalDistance();
```

### Send Command to Device

```csharp
var cmd = new Command
{
    DeviceId = deviceId,
    Type = CommandType.SetGpsInterval,
    Parameters = new Dictionary<string, object> { { "interval", 60 } }
};
var created = await commandService.CreateCommandAsync(cmd);
await commandService.ExecuteCommandAsync(created.Id);
```
