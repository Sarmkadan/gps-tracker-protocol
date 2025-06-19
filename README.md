# GPS Tracker Protocol Parser

A comprehensive .NET library for parsing GPS tracker protocols (GT06, H02, TK103) - converting raw TCP/UDP data streams into structured location information.

## Features

- **Multi-Protocol Support**: GT06, H02, TK103 protocol parsing
- **Frame Validation**: Checksum validation and frame integrity checking
- **Location Services**: Store, query, and analyze GPS location data
- **Device Management**: Register, monitor, and manage tracking devices
- **Journey Tracking**: Record and analyze trips with distance/speed metrics
- **Command System**: Send and manage commands to tracking devices
- **In-Memory Storage**: Demo repository implementation with full CRUD operations
- **Dependency Injection**: Built with Microsoft.Extensions.DependencyInjection
- **Async/Await**: Fully asynchronous API design
- **.NET 10**: Latest C# language features and performance optimizations

## Project Structure

```
gps-tracker-protocol/
├── Domain/
│   ├── Models/              # Domain entities (LocationData, Device, Journey, etc.)
│   ├── Enums.cs            # Protocol enums and types
│   └── Exceptions.cs        # Custom exceptions
├── Data/
│   ├── IRepository.cs       # Repository interfaces
│   ├── InMemoryRepository.cs    # In-memory implementations
│   └── InMemoryRepositories.cs  # Specialized repository implementations
├── Services/
│   ├── ProtocolParserService.cs # Protocol parsing
│   ├── DeviceService.cs         # Device management
│   ├── LocationDataService.cs   # Location data handling
│   ├── CommandService.cs        # Command management
│   └── JourneyService.cs        # Journey tracking
├── Configuration/
│   └── DependencyInjection.cs   # DI setup
├── Program.cs               # Console application with demo
├── Constants.cs             # Protocol and configuration constants
├── GpsTrackerProtocol.csproj
├── LICENSE                  # MIT License
├── .gitignore
└── README.md
```

## Core Components

### Domain Models (8 entities)

1. **LocationData** - GPS coordinates, speed, bearing, satellite count
2. **Device** - Tracking device with IMEI, protocol, status
3. **GpsFrame** - Raw protocol frame with validation
4. **Journey** - Trip data with waypoints and analytics
5. **Command** - Commands to send to devices
6. **ResponseMessage** - Device responses and status updates
7. Plus type definitions: ProtocolType, DeviceStatus, CommandType, etc.

### Services (5 implementations)

1. **ProtocolParserService** - Frame parsing and protocol detection
2. **DeviceService** - Device registration and lifecycle management
3. **LocationDataService** - Location storage and querying
4. **CommandService** - Command creation and execution
5. **JourneyService** - Journey creation and analytics

### Data Layer

- Generic `IRepository<T>` interface with CRUD operations
- Specialized repository interfaces for each domain model
- In-memory implementations suitable for testing and demos
- Thread-safe operations with `ReaderWriterLockSlim`
- Unit of Work pattern for transaction management

## Quick Start

### Building

```bash
dotnet build
```

### Running the Demo

```bash
dotnet run
```

The demo application demonstrates:
- Device registration with IMEI validation
- GPS frame parsing and validation
- Location data storage and queries
- Journey creation and metrics calculation
- Command creation and execution
- Device status tracking

## Usage Example

```csharp
var services = new ServiceCollection();
services.AddGpsTrackerServices();
var provider = services.BuildServiceProvider();

var deviceService = provider.GetRequiredService<IDeviceService>();
var locationService = provider.GetRequiredService<ILocationDataService>();

// Register device
var device = new Device { Imei = "358240050447491", Protocol = ProtocolType.GT06 };
await deviceService.RegisterDeviceAsync(device);

// Store location
var location = new LocationData 
{ 
    DeviceId = device.Id,
    Latitude = 40.7128,
    Longitude = -74.0060,
    Speed = 50.0
};
await locationService.StoreLocationAsync(location);

// Query location
var latest = await locationService.GetLatestLocationAsync(device.Id);
var history = await locationService.GetLocationHistoryAsync(device.Id, limit: 100);
```

## Protocol Support

### GT06
- Binary protocol with XOR checksum
- Support for extended data fields
- Device-specific implementations

### H02
- ASCII-based GPRMC format
- NMEA compatible parsing
- Standard coordinate formats

### TK103
- Fixed-size frame structure
- Compact binary representation
- Efficient transmission for low-bandwidth scenarios

## Exception Handling

Custom exception hierarchy for proper error handling:
- `ParseException` - Protocol parsing failures
- `ChecksumException` - Validation failures
- `DeviceException` - Device-related errors
- `CommandException` - Command execution failures
- `ValidationException` - Data validation errors
- `RepositoryException` - Data access errors

## Configuration

Protocol-specific constants in `Constants.cs`:
- Frame sizes and markers
- GPS intervals and timeouts
- Measurement bounds
- Alarm thresholds
- Error codes and patterns

## Testing

The in-memory repository implementations are suitable for:
- Unit testing services
- Integration testing workflows
- Demo and prototype development
- Performance testing without I/O overhead

## Future Enhancements

- Database storage implementations (SQL Server, PostgreSQL)
- Real-time WebSocket/gRPC support
- Message queue integration
- Geofencing and alert system
- Analytics and reporting engine
- Mobile companion app support

## License

MIT License - See LICENSE file for details

## Author

Vladyslav Zaiets  
https://sarmkadan.com
