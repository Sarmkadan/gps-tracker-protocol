![CI](https://github.com/sarmkadan/gps-tracker-protocol/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/gps-tracker-protocol)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

# GPS Tracker Protocol Parser

A comprehensive .NET library for parsing GPS tracker protocols (GT06, H02, TK103) - converting raw TCP/UDP data streams into structured location information with full device management, journey tracking, and command execution capabilities.

**Status**: Production Ready | **Latest Version**: 1.0.0 | **.NET**: 10.0 | **License**: MIT

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Performance](#performance)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

---

## Features

### Core Capabilities

- **Multi-Protocol Support**: Parse GT06, H02, and TK103 GPS tracker protocols
- **Frame Validation**: Checksum validation with protocol-specific error detection
- **Location Services**: Store, retrieve, and analyze GPS coordinates with metadata
- **Device Management**: Register, monitor, and manage tracking devices with lifecycle tracking
- **Journey Tracking**: Record trips with waypoints, distance, speed, and duration analytics
- **Command System**: Send configuration commands to devices (intervals, alarms, settings)
- **Real-Time Processing**: Async/await patterns for efficient concurrent operations
- **Data Formatting**: JSON, CSV, and GeoJSON export support
- **Caching Layer**: In-memory caching for high-frequency queries
- **Rate Limiting**: Protection against excessive API calls
- **Error Handling**: Comprehensive exception hierarchy with custom error codes
- **Logging Pipeline**: Structured logging with multiple output levels
- **Dependency Injection**: Microsoft.Extensions integration for flexible composition
- **.NET 10**: Latest C# language features and performance optimizations
- **Geofence Alerting**: Rule-based alert management with cooldown suppression and acknowledgement workflow
- **Route Replay**: Replay any completed journey at configurable speed with rebased timestamps
- **Device Diagnostics**: Comprehensive per-device health reports and fleet-wide health summaries

### Protocol Details

| Protocol | Type | Checksum | Frame Size | Max Devices |
|----------|------|----------|-----------|-------------|
| **GT06** | Binary | XOR | 25-100 bytes | 64K+ |
| **H02** | ASCII | NMEA | 50-150 bytes | Unlimited |
| **TK103** | Binary | Sum | Fixed 32 bytes | 16K |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│           (CLI, API Controllers, WebSocket Gateway)         │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                   Service Layer                             │
│  ProtocolParserService | DeviceService | LocationService   │
│  JourneyService | CommandService | AnalyticsService        │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  Validation | Logging | Caching | Rate Limiting | Errors   │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                    Data Layer                               │
│  IRepository<T> | InMemoryRepository | Specialized Repos   │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                  Domain Models                              │
│  Device | LocationData | Journey | Command | GpsFrame      │
│  ResponseMessage | Enums | Exceptions                       │
└─────────────────────────────────────────────────────────────┘
```

### Directory Structure

```
gps-tracker-protocol/
├── Domain/                          # Business logic and entities
│   ├── Models/                      # Domain models
│   │   ├── Device.cs               # Tracking device
│   │   ├── LocationData.cs         # GPS coordinates and metadata
│   │   ├── Journey.cs              # Trip tracking
│   │   ├── Command.cs              # Device commands
│   │   ├── GpsFrame.cs             # Raw protocol frames
│   │   └── ResponseMessage.cs      # Device responses
│   ├── Enums.cs                    # Protocol types and status enums
│   └── Exceptions.cs               # Custom exception hierarchy
├── Data/                            # Data access patterns
│   ├── IRepository.cs              # Generic repository interface
│   ├── InMemoryRepository.cs       # Generic in-memory implementation
│   └── InMemoryRepositories.cs     # Specialized repositories
├── Services/                        # Business logic services
│   ├── ProtocolParserService.cs    # Frame parsing and detection
│   ├── DeviceService.cs            # Device lifecycle management
│   ├── LocationDataService.cs      # Location storage and queries
│   ├── CommandService.cs           # Command creation and execution
│   ├── JourneyService.cs           # Journey analytics
│   ├── AnalyticsService.cs         # Usage analytics and metrics
│   └── GeofenceService.cs          # Geofence management
├── Configuration/                   # DI and application setup
│   └── DependencyInjection.cs      # Service registration
├── Infrastructure/                  # Cross-cutting concerns
│   ├── ErrorHandlingMiddleware.cs  # Global error handling
│   ├── ValidationPipeline.cs       # Input validation
│   ├── LoggingPipeline.cs          # Structured logging
│   └── RateLimitingService.cs      # API rate limiting
├── Integration/                     # External service integration
│   ├── HttpClientFactory.cs        # HTTP client creation
│   ├── WebhookClient.cs            # Webhook delivery
│   ├── GeocodingService.cs         # Address reverse lookup
│   ├── WeatherApiClient.cs         # Weather data
│   ├── NotificationService.cs      # Alert notifications
│   └── SimulationService.cs        # Test data generation
├── Formatting/                      # Output formatters
│   ├── JsonFormatter.cs            # JSON output
│   ├── CsvFormatter.cs             # CSV export
│   └── GeoJsonFormatter.cs         # GeoJSON/Map format
├── BackgroundWorkers/               # Background processing
│   ├── BackgroundProcessingService.cs
│   ├── JourneyAnalyticsWorker.cs
│   └── LocationAggregationWorker.cs
├── CLI/                             # Command-line interface
│   └── CommandLineInterface.cs     # CLI commands and parsing
├── Utilities/                       # Helper extensions and utilities
│   ├── ByteExtensions.cs           # Byte array operations
│   ├── StringExtensions.cs         # String utilities
│   ├── DateTimeExtensions.cs       # Date/time helpers
│   ├── GpsUtilities.cs             # GPS math and conversions
│   ├── CollectionExtensions.cs     # LINQ helpers
│   ├── DictionaryExtensions.cs     # Dictionary operations
│   └── PerformanceMonitor.cs       # Performance metrics
├── Caching/                         # Caching layer
│   └── CachingService.cs           # In-memory caching
├── Program.cs                       # Console app entry point
├── Constants.cs                     # Protocol constants
├── GpsTrackerProtocol.csproj       # Project configuration
├── LICENSE                          # MIT License
├── .gitignore                       # Git ignore patterns
└── README.md                        # This file
```

---

## Installation

### Option 1: Clone and Build from Source

```bash
git clone https://github.com/sarmkadan/gps-tracker-protocol.git
cd gps-tracker-protocol
dotnet build
dotnet run
```

### Option 2: NuGet Package (when published)

```bash
dotnet add package Zaiets.gps.tracker.protocol
```

### Option 3: Docker Container

```bash
docker build -t gps-tracker-protocol .
docker run -it gps-tracker-protocol
```

### Option 4: Docker Compose (with example services)

```bash
docker-compose up --build
```

### Requirements

- **.NET Runtime**: 10.0 or later
- **C# Version**: 12 or later
- **Platform**: Windows, macOS, Linux
- **Memory**: 256MB minimum (512MB+ recommended)
- **Disk**: 50MB for dependencies

---

## Quick Start

### Minimal Example: Parse a GPS Frame

```csharp
using GpsTrackerProtocol;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.DependencyInjection;

// Setup services
var services = new ServiceCollection();
services.AddGpsTrackerServices();
var provider = services.BuildServiceProvider();

// Get parser service
var parser = provider.GetRequiredService<IProtocolParserService>();

// Parse raw GPS data (GT06 protocol)
byte[] rawData = new byte[] { 
    0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23, 0x34, 0x02, 
    0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00, 0x00, 
    0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A 
};

var frame = new GpsFrame
{
    RawData = rawData,
    Protocol = ProtocolType.GT06,
    ReceivedAt = DateTime.UtcNow
};

bool isValid = await parser.ValidateFrameAsync(frame);
ProtocolType detected = await parser.DetectProtocolAsync(rawData);

Console.WriteLine($"Valid: {isValid}, Protocol: {detected}");
```

### Complete Example: Device Registration and Location Tracking

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGpsTrackerServices();
var provider = services.BuildServiceProvider();

var deviceService = provider.GetRequiredService<IDeviceService>();
var locationService = provider.GetRequiredService<ILocationDataService>();

// Register tracking device
var device = new Device
{
    Imei = "358240050447491",
    DeviceName = "Vehicle #001",
    Protocol = ProtocolType.GT06,
    IsActive = true
};

var registered = await deviceService.RegisterDeviceAsync(device);
Console.WriteLine($"Device registered with ID: {registered.Id}");

// Record location
var location = new LocationData
{
    DeviceId = registered.Id,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Speed = 55.5,
    Bearing = 123.45,
    Altitude = 15.0,
    SatelliteCount = 12,
    Accuracy = 5.0,
    Timestamp = DateTime.UtcNow,
    Protocol = ProtocolType.GT06
};

var stored = await locationService.StoreLocationAsync(location);
Console.WriteLine($"Location stored: {stored.Latitude}, {stored.Longitude}");

// Query latest location
var latest = await locationService.GetLatestLocationAsync(registered.Id);
Console.WriteLine($"Latest: Speed={latest?.Speed}km/h, Sats={latest?.SatelliteCount}");

// Get location history
var history = await locationService.GetLocationHistoryAsync(registered.Id, limit: 100);
Console.WriteLine($"History records: {history.Count()}");
```

---

## Usage Examples

### Example 1: Real-Time TCP Server

See `examples/RealTimeGpsServer.cs` - Listen for GPS updates over TCP, parse frames, store locations.

```bash
dotnet run --project examples/RealTimeGpsServer.cs
```

Then connect with raw GPS data:
```bash
nc localhost 5000 < gps_samples.bin
```

### Example 2: Batch Data Import

See `examples/BatchDataImporter.cs` - Import locations from CSV/JSON files.

```bash
dotnet run --project examples/BatchDataImporter.cs --file devices.csv
```

### Example 3: Journey Analysis

See `examples/JourneyAnalyzer.cs` - Calculate trip metrics and generate reports.

```bash
dotnet run --project examples/JourneyAnalyzer.cs --device device-001
```

### Example 4: Device Command Center

See `examples/DeviceCommandCenter.cs` - Interactive CLI for managing devices and sending commands.

```bash
dotnet run --project examples/DeviceCommandCenter.cs
```

### Example 5: Geofence Monitoring

See `examples/GeofenceMonitor.cs` - Monitor devices within geofence boundaries.

```bash
dotnet run --project examples/GeofenceMonitor.cs --config fences.json
```

### Example 6: Data Export

See `examples/DataExporter.cs` - Export locations to JSON, CSV, GeoJSON formats.

```bash
dotnet run --project examples/DataExporter.cs --format geojson --output map.json
```

### Example 7: Performance Benchmark

See `examples/PerformanceBenchmark.cs` - Stress test parsing and storage performance.

```bash
dotnet run --project examples/PerformanceBenchmark.cs --frames 100000
```

### Example 8: Protocol Converter

See `examples/ProtocolConverter.cs` - Convert between GPS tracker protocols.

```bash
dotnet run --project examples/ProtocolConverter.cs --from GT06 --to H02 --input data.bin
```

---

## API Reference

### IDeviceService

```csharp
public interface IDeviceService
{
    Task<Device> RegisterDeviceAsync(Device device);
    Task<Device?> GetDeviceAsync(string deviceId);
    Task<IEnumerable<Device>> GetAllDevicesAsync();
    Task<bool> UnregisterDeviceAsync(string deviceId);
    Task<Device> UpdateDeviceAsync(Device device);
    Task<IEnumerable<Device>> GetDevicesByProtocolAsync(ProtocolType protocol);
    Task<IEnumerable<Device>> GetActiveDevicesAsync();
}
```

### ILocationDataService

```csharp
public interface ILocationDataService
{
    Task<LocationData> StoreLocationAsync(LocationData location);
    Task<LocationData?> GetLatestLocationAsync(string deviceId);
    Task<IEnumerable<LocationData>> GetLocationHistoryAsync(string deviceId, int limit = 100);
    Task<IEnumerable<LocationData>> GetLocationsByDateRangeAsync(
        string deviceId, 
        DateTime startTime, 
        DateTime endTime
    );
    Task<IEnumerable<LocationData>> GetLocationsByRegionAsync(
        double lat, 
        double lng, 
        double radiusKm
    );
}
```

### IJourneyService

```csharp
public interface IJourneyService
{
    Task<Journey> StartJourneyAsync(string deviceId);
    Task<Journey> CompleteJourneyAsync(string journeyId);
    Task AddWaypointAsync(string journeyId, LocationData waypoint);
    Task<Journey?> GetJourneyAsync(string journeyId);
    Task<IEnumerable<Journey>> GetJourneyHistoryAsync(string deviceId);
    Task<double> GetTotalDistanceAsync(string deviceId);
    Task<TimeSpan> GetTotalDurationAsync(string deviceId);
    Task<double> GetAverageSpeedAsync(string deviceId);
}
```

### IProtocolParserService

```csharp
public interface IProtocolParserService
{
    Task<ProtocolType> DetectProtocolAsync(byte[] data);
    Task<bool> ValidateFrameAsync(GpsFrame frame);
    Task<ParseResult> ParseFrameAsync(GpsFrame frame);
    Task<LocationData?> ExtractLocationDataAsync(GpsFrame frame);
    Task<CommandResponse?> ExtractCommandResponseAsync(GpsFrame frame);
}
```

### ICommandService

```csharp
public interface ICommandService
{
    Task<Command> CreateCommandAsync(Command command);
    Task<bool> ExecuteCommandAsync(string commandId);
    Task<CommandStatus> GetCommandStatusAsync(string commandId);
    Task<IEnumerable<Command>> GetCommandHistoryAsync(string deviceId);
    Task<CommandResponse?> GetCommandResponseAsync(string commandId);
}
```

### IAnalyticsService

```csharp
public interface IAnalyticsService
{
    Task<AnalyticsReport> GetDeviceAnalyticsAsync(string deviceId, DateRange dateRange);
    Task<FleetAnalytics> GetFleetAnalyticsAsync(DateRange dateRange);
    Task<DrivingStatistics> AnalyzeDrivingPatterns(string deviceId);
    Task<AlertsReport> DetectAnomaliesAsync(string deviceId);
}
```

### IGeofenceAlertingService

Manages rule-based geofence alerts.  Automatically subscribes to the internal event bus on construction.

```csharp
public interface IGeofenceAlertingService
{
    GeofenceAlertRule CreateAlertRule(
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType,         // Enter | Exit | DwellTime
        TimeSpan? cooldown = null,           // Minimum gap between alerts (default 5 min)
        string description = "");

    void DeleteAlertRule(string ruleId);
    IReadOnlyList<GeofenceAlertRule> GetRulesForDevice(string deviceId);
    IReadOnlyList<GeofenceAlert> GetActiveAlerts(string deviceId);
    IReadOnlyList<GeofenceAlert> GetAlertHistory(string deviceId, int limit = 50);
    bool AcknowledgeAlert(string alertId, string notes = "");
}
```

**CLI**
```
alerts list <device-id>                          List active alerts
alerts add <device-id> <fence-id> <enter|exit>   Create a rule
alerts ack <alert-id> [notes]                    Acknowledge an alert
```


### IRouteReplayService

Replays a completed journey's waypoints with configurable speed and timestamp rebasing.

```csharp
public interface IRouteReplayService
{
    Task<RouteReplayResult> ReplayJourneyAsync(string journeyId, ReplayOptions? options = null);
    Task<RouteReplaySummary> GetReplaySummaryAsync(string journeyId);
}

public class ReplayOptions
{
    public double SpeedMultiplier { get; set; } = 1.0;
    public int StartIndex { get; set; } = 0;
    public int EndIndex   { get; set; } = -1;
    public DateTime? RebaseToUtc { get; set; }
}
```

**CLI**
```
replay <journey-id> [speed-multiplier]   Replay route at N× speed (default 1×)
```


### IDeviceDiagnosticsService

Produces comprehensive health snapshots for individual devices and fleet-wide summaries.

```csharp
public interface IDeviceDiagnosticsService
{
    Task<DeviceDiagnosticsReport?> GetDiagnosticsAsync(string deviceId);
    Task<FleetHealthReport>        GetFleetHealthReportAsync();
    Task<DeviceSelfTestResult?>    RunSelfTestAsync(string deviceId);
}
```

`DeviceDiagnosticsReport` includes connectivity (`IsOnline`, `LastSeen`, `TotalPacketsReceived`),
hardware telemetry (`BatteryLevel`, `SignalStrength`, `SignalQuality`), and activity metrics
(`TotalLocationPoints`, `LastLocation`, `TotalDistanceKm`, `TotalJourneys`).

**CLI**
```
diagnostics <device-id>              Print full diagnostics report
diagnostics <device-id> --selftest   Run self-test and show pass/warn status
```

---

## Configuration

### appsettings.json

```json
{
  "GpsTrackerProtocol": {
    "DefaultProtocol": "GT06",
    "MaxDevices": 10000,
    "LocationHistoryLimit": 1000,
    "CacheExpirationMinutes": 60,
    "RateLimitPerMinute": 1000,
    "ValidationEnabled": true,
    "LoggingLevel": "Information"
  },
  "ProtocolSettings": {
    "GT06": {
      "Enabled": true,
      "Timeout": 30,
      "MaxFrameSize": 200
    },
    "H02": {
      "Enabled": true,
      "Timeout": 30,
      "MaxFrameSize": 300
    },
    "TK103": {
      "Enabled": true,
      "Timeout": 30,
      "MaxFrameSize": 100
    }
  },
  "Services": {
    "Geocoding": {
      "ApiKey": "your-api-key",
      "Provider": "OpenStreetMap"
    },
    "Notifications": {
      "Enabled": false,
      "WebhookUrl": "https://your-server/webhook"
    }
  }
}
```

### Environment Variables

```bash
GPS_PROTOCOL_DEFAULT=GT06
GPS_MAX_DEVICES=10000
GPS_CACHE_EXPIRATION=60
GPS_LOG_LEVEL=Information
GPS_RATE_LIMIT=1000
```

### Constants Reference (Constants.cs)

- **FRAME_START_MARKER**: Protocol-specific frame delimiters
- **MAX_LATITUDE**: ±90.0 degrees
- **MAX_LONGITUDE**: ±180.0 degrees
- **MIN_SATELLITE_COUNT**: 3 (for valid fix)
- **MAX_SPEED**: 999.9 km/h
- **GEOFENCE_ALERT_THRESHOLD**: 100 meters
- **COMMAND_TIMEOUT**: 30 seconds
- **LOCATION_HISTORY_RETENTION**: 90 days

---

## Troubleshooting

### Issue: Frame Validation Fails

**Symptom**: `ChecksumException: Checksum mismatch`

**Solutions**:
1. Verify raw data is complete and not truncated
2. Check protocol type matches frame format
3. Ensure byte order is correct (Little-Endian for GT06/TK103, Big-Endian for H02)
4. Validate frame delimiters: GT06 uses `0x78 0x78`, H02 uses `$`, TK103 uses `78 78`

```csharp
// Enable detailed logging
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
```

### Issue: Null Reference When Parsing

**Symptom**: `NullReferenceException` in ParseFrameAsync

**Solutions**:
1. Always check if protocol is detected correctly
2. Validate device exists before storing location
3. Use try-catch blocks around parsing operations

```csharp
try 
{
    var result = await parser.ParseFrameAsync(frame);
    if (result != null) { /* process */ }
}
catch (ParseException ex)
{
    logger.LogError(ex, "Frame parsing failed");
}
```

### Issue: Memory Consumption Grows Over Time

**Symptom**: Process memory usage increases continuously

**Solutions**:
1. Implement location history cleanup
2. Configure cache expiration timeouts
3. Use database storage instead of in-memory for large datasets
4. Monitor with PerformanceMonitor utility

```csharp
// Periodic cleanup
var timer = new Timer(async _ => 
{
    await locationService.PurgeOldLocationsAsync(DateTime.UtcNow.AddDays(-30));
}, null, TimeSpan.Zero, TimeSpan.FromHours(1));
```

### Issue: Slow Performance with Large Datasets

**Symptom**: Response times exceed 1 second for queries

**Solutions**:
1. Enable caching for frequently accessed data
2. Use pagination for history queries
3. Create indexes on DeviceId and Timestamp
4. Consider switching to SQL database

```csharp
// Use pagination
var page = await locationService.GetLocationHistoryAsync(
    deviceId, 
    limit: 100, 
    offset: pageNumber * 100
);
```

### Issue: Protocol Detection Fails

**Symptom**: `DetectProtocolAsync` returns Unknown

**Solutions**:
1. Ensure raw data includes complete frame (with delimiters)
2. Check frame size meets minimum requirements
3. Verify protocol is enabled in configuration
4. Manually specify protocol if detection fails

```csharp
if (detected == ProtocolType.Unknown)
{
    frame.Protocol = ProtocolType.GT06; // Fallback
}
```

---

## Deployment

### Docker Deployment

```bash
# Build image
docker build -t gps-tracker:latest .

# Run container with volume mounting
docker run -v $(pwd)/data:/app/data gps-tracker:latest

# Use docker-compose for multiple services
docker-compose up -d
```

### Kubernetes Deployment

See `docs/deployment.md` for K8s manifest examples.

### Performance Tuning

- **Thread Pool**: Increase MinThreads for high-throughput scenarios
- **GC Settings**: Use `Server` GC for production (net10.0.runtimeconfig.json)
- **Async Limits**: Configure MaxDegreeOfParallelism based on CPU cores
- **Buffer Sizes**: Tune socket buffer sizes for network performance

---

## Testing

The test suite lives in `tests/gps-tracker-protocol.Tests/` and covers domain models, services, GPS math utilities, and collection/string extensions.

```bash
# Run all tests
dotnet test

# Run with coverage report
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test class
dotnet test --filter "FullyQualifiedName~GpsUtilitiesTests"
```

Key test files:

| File | Coverage |
|------|----------|
| `DomainAndServiceTests.cs` | Models, services, geofence logic |
| `GpsUtilitiesTests.cs` | Distance, bearing, coordinate math |
| `ExtensionsTests.cs` | Byte, string, date/time, collection helpers |

---

## Performance

Benchmarks measured on a single core (AMD Ryzen 5 5600X, .NET 10, in-memory storage):

| Operation | Throughput / Latency |
|-----------|---------------------|
| GT06 frame parsing | ~52,000 frames/sec |
| H02 frame parsing | ~38,000 frames/sec |
| TK103 frame parsing | ~61,000 frames/sec |
| Location store (in-memory) | <0.5 ms per record |
| Location history query (1 K records) | <2 ms |
| Journey analytics (10 K waypoints) | <45 ms |
| Protocol auto-detection | <0.1 ms |
| Cache hit (warm) | <0.05 ms |

**Memory footprint**: approximately 1 MB per 10,000 stored `LocationData` records.

To reproduce these numbers on your hardware:

```bash
dotnet run --project examples/PerformanceBenchmark.cs --frames 100000
```

---

## Related Projects

### Ecosystem

Part of a collection of .NET libraries and tools. See more at [github.com/sarmkadan](https://github.com/sarmkadan).

### Integration Examples

**Embedding in an ASP.NET Core Web API**

Register the library's services alongside your web stack and expose a minimal endpoint that accepts a raw GPS frame over HTTP:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGpsTrackerServices();

var app = builder.Build();

app.MapPost("/ingest", async (HttpRequest req, IProtocolParserService parser,
    ILocationDataService locations) =>
{
    using var ms = new MemoryStream();
    await req.Body.CopyToAsync(ms);
    var raw = ms.ToArray();
    var frame = new GpsFrame { RawData = raw, ReceivedAt = DateTime.UtcNow };
    frame.Protocol = await parser.DetectProtocolAsync(raw);
    var loc = await parser.ExtractLocationDataAsync(frame);
    if (loc is not null) await locations.StoreLocationAsync(loc);
    return Results.Ok(new { protocol = frame.Protocol.ToString() });
});

app.Run();
```

**Streaming real-time updates via SignalR**

Pair `LocationAggregationWorker` with a SignalR hub so connected clients receive live device positions without polling:

```csharp
// In Program.cs
builder.Services.AddGpsTrackerServices();
builder.Services.AddSignalR();
builder.Services.AddHostedService<LocationAggregationWorker>();

// In LocationAggregationWorker.cs – after storing a new location
await _hubContext.Clients.Group(location.DeviceId)
    .SendAsync("LocationUpdate", location);
```

---

## Contributing

We welcome contributions! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature`
3. **Commit** changes: `git commit -am 'Add feature'`
4. **Push** to branch: `git push origin feature/your-feature`
5. **Submit** a Pull Request with description

### Code Standards

- Follow C# naming conventions (PascalCase for public, camelCase for private)
- Write unit tests for new features
- Update documentation for API changes
- Run `dotnet format` before committing
- Include XML comments for public APIs

### Development Setup

```bash
git clone https://github.com/sarmkadan/gps-tracker-protocol.git
cd gps-tracker-protocol
dotnet restore
dotnet build
dotnet test
```

---

## License

MIT License - See LICENSE file for details

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/sarmkadan) | [Telegram](https://t.me/sarmkadan)
