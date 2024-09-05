# Getting Started with GPS Tracker Protocol Parser

This guide will help you set up and start using the GPS Tracker Protocol Parser library.

## Prerequisites

- **.NET 10 Runtime** or later ([download](https://dotnet.microsoft.com/download))
- **Git** (for cloning the repository)
- **Visual Studio Code** or your preferred C# IDE (optional)
- **256MB RAM** minimum (512MB+ recommended)
- **50MB disk space** for dependencies

## Installation Methods

### Method 1: Clone from GitHub (Recommended for Development)

```bash
# Clone the repository
git clone https://github.com/sarmkadan/gps-tracker-protocol.git
cd gps-tracker-protocol

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the demo
dotnet run
```

### Method 2: Docker (Recommended for Production)

```bash
# Build Docker image
docker build -t gps-tracker-protocol .

# Run container
docker run -it gps-tracker-protocol

# With volume mounting for data persistence
docker run -it -v $(pwd)/data:/app/data gps-tracker-protocol
```

### Method 3: Docker Compose (Multi-Service Setup)

```bash
# Start services
docker-compose up --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Method 4: NuGet Package (When Published)

```bash
dotnet add package GpsTrackerProtocol
```

## Your First Application

### Basic Frame Parsing

Create a new .NET 10 console app:

```bash
dotnet new console -n MyGpsApp
cd MyGpsApp
dotnet add package GpsTrackerProtocol
```

Write minimal code in `Program.cs`:

```csharp
using GpsTrackerProtocol;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGpsTrackerServices();
var provider = services.BuildServiceProvider();

var parser = provider.GetRequiredService<IProtocolParserService>();

// GT06 sample frame
byte[] gpsFrame = new byte[] { 
    0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23, 0x34, 0x02, 
    0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00, 0x00, 
    0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A 
};

var frame = new GpsFrame { RawData = gpsFrame, Protocol = ProtocolType.GT06 };
bool valid = await parser.ValidateFrameAsync(frame);
ProtocolType detected = await parser.DetectProtocolAsync(gpsFrame);

Console.WriteLine($"Valid: {valid}, Protocol: {detected}");
```

Run:
```bash
dotnet run
# Output: Valid: True, Protocol: GT06
```

### Device Registration and Tracking

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGpsTrackerServices();
var provider = services.BuildServiceProvider();

var deviceService = provider.GetRequiredService<IDeviceService>();
var locationService = provider.GetRequiredService<ILocationDataService>();

// Register device
var device = new Device
{
    Imei = "358240050447491",
    DeviceName = "My First Tracker",
    Protocol = ProtocolType.GT06,
    IsActive = true
};

var registered = await deviceService.RegisterDeviceAsync(device);
Console.WriteLine($"Device ID: {registered.Id}");

// Store location
var location = new LocationData
{
    DeviceId = registered.Id,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Speed = 55.0,
    Accuracy = 5.0,
    SatelliteCount = 12,
    Timestamp = DateTime.UtcNow
};

await locationService.StoreLocationAsync(location);

// Query location
var latest = await locationService.GetLatestLocationAsync(registered.Id);
Console.WriteLine($"Location: {latest?.Latitude}, {latest?.Longitude}");
```

## Common Tasks

### Listen for GPS Data Over TCP

See `examples/RealTimeGpsServer.cs`:

```bash
dotnet run --project examples/RealTimeGpsServer.cs 5000 5001
```

Then send data:
```bash
echo -ne '\x78\x78\x1F...' | nc localhost 5000
```

### Import Batch Data from CSV

Create `devices.csv`:
```csv
DeviceId,Latitude,Longitude,Speed,Timestamp
device-001,40.7128,-74.0060,50.5,2026-05-04T10:30:00Z
device-001,40.7129,-74.0061,51.0,2026-05-04T10:31:00Z
```

Run importer:
```bash
dotnet run --project examples/BatchDataImporter.cs csv devices.csv
```

### Export Data to Multiple Formats

```bash
# Export to JSON
dotnet run --project examples/DataExporter.cs json locations.json device-001

# Export to CSV
dotnet run --project examples/DataExporter.cs csv locations.csv device-001

# Export to GeoJSON (for mapping)
dotnet run --project examples/DataExporter.cs geojson map.json device-001
```

### Analyze Journeys

```bash
# Analyze a device's journey history
dotnet run --project examples/JourneyAnalyzer.cs analyze device-001

# Simulate a journey with 50 waypoints
dotnet run --project examples/JourneyAnalyzer.cs simulate device-001 50

# Generate fleet report
dotnet run --project examples/JourneyAnalyzer.cs fleet
```

### Run Performance Tests

```bash
# Benchmark frame validation (10K frames)
dotnet run --project examples/PerformanceBenchmark.cs validation 10000

# Benchmark storage (10K locations)
dotnet run --project examples/PerformanceBenchmark.cs storage 10000

# Run full stress test (100 devices, 100 locations each)
dotnet run --project examples/PerformanceBenchmark.cs stress 100 100

# Run all benchmarks
dotnet run --project examples/PerformanceBenchmark.cs all
```

## Project Structure

```
gps-tracker-protocol/
├── Domain/                 # Business logic entities
├── Data/                   # Data access interfaces
├── Services/               # Business logic services
├── Infrastructure/         # Cross-cutting concerns
├── Integration/            # External service adapters
├── Formatting/             # Output formatters
├── Utilities/              # Helper extensions
├── Configuration/          # DI setup
├── examples/               # Complete example applications
├── docs/                   # Documentation
├── tests/                  # Unit tests (future)
└── README.md              # Main documentation
```

## Configuration

### appsettings.json

Create in your project root:

```json
{
  "GpsTrackerProtocol": {
    "DefaultProtocol": "GT06",
    "MaxDevices": 10000,
    "CacheExpirationMinutes": 60,
    "RateLimitPerMinute": 1000,
    "ValidationEnabled": true
  }
}
```

### Environment Variables

```bash
export GPS_PROTOCOL_DEFAULT=GT06
export GPS_MAX_DEVICES=10000
export GPS_LOG_LEVEL=Information
```

## Development

### Building

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### Code Formatting

```bash
# Format code
dotnet format

# Check formatting
dotnet format --verify-no-changes
```

### Running Tests

```bash
dotnet test
```

## Troubleshooting

### Issue: "No project file found"

Make sure you're in the project directory:
```bash
cd gps-tracker-protocol
```

### Issue: Version mismatch in NuGet packages

```bash
dotnet restore --no-cache
dotnet clean
dotnet build
```

### Issue: Framework not found

Verify .NET 10 is installed:
```bash
dotnet --list-sdks
```

Download if needed: https://dotnet.microsoft.com/download

### Issue: TCP server won't bind to port

Try a different port:
```bash
dotnet run --project examples/RealTimeGpsServer.cs 9000 9001
```

Or check if port is in use:
```bash
# Linux/macOS
lsof -i :5000

# Windows
netstat -ano | findstr :5000
```

## Next Steps

1. **Read the [Architecture Guide](architecture.md)** to understand the system design
2. **Review the [API Reference](api-reference.md)** for all available interfaces
3. **Explore [Examples](../examples/)** for working code samples
4. **Check [Deployment Guide](deployment.md)** for production setup
5. **Browse [FAQ](faq.md)** for common questions

## Getting Help

- Check the [README](../README.md) for features and usage
- Review [examples/](../examples/) for working code
- Read [API Reference](api-reference.md) for interface documentation
- Check [FAQ](faq.md) for common issues
- Create an issue on GitHub for bugs or feature requests

## What's Next?

- Integrate with your GPS tracking hardware
- Set up a real-time server for live tracking
- Deploy to production with Docker
- Build a web dashboard for visualizing locations
- Implement geofencing and alerts
