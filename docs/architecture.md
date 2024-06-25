# Architecture Guide

This document describes the internal architecture of the GPS Tracker Protocol Parser.

## High-Level Architecture

```
┌───────────────────────────────────────────────────────────────┐
│                   Presentation Layer                          │
│   (CLI, REST API, WebSocket, Real-time Servers, Examples)    │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                   Service Layer                              │
│  ┌─────────────┐  ┌────────────────┐  ┌──────────────────┐ │
│  │ProtocolParser  │DeviceService    │LocationService   │ │
│  └─────────────┘  └────────────────┘  └──────────────────┘ │
│  ┌─────────────┐  ┌────────────────┐  ┌──────────────────┐ │
│  │JourneyService  │CommandService   │AnalyticsService  │ │
│  └─────────────┘  └────────────────┘  └──────────────────┘ │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│              Infrastructure Layer                            │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌───────────┐  │
│  │Validation│  │Logging   │  │Caching   │  │RateLimit  │  │
│  └──────────┘  └──────────┘  └──────────┘  └───────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          Error Handling Middleware                   │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                   Data Layer                                 │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │IRepository<T>  │Generic In-Mem │Specialized Repos  │   │
│  └─────────────┘  └──────────────┘  └──────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Specialized: DeviceRepository, LocationRepository    │   │
│  │             JourneyRepository, CommandRepository     │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                  Domain Layer                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │Device        │  │LocationData  │  │Journey           │   │
│  │GpsFrame      │  │Command       │  │ResponseMessage   │   │
│  │Enums         │  │Exceptions    │  │Constants         │   │
│  └──────────────┘  └──────────────┘  └──────────────────┘   │
└──────────────────────────────────────────────────────────────┘
```

## Layered Architecture Pattern

### 1. Domain Layer

**Purpose**: Pure business logic and data models - no framework dependencies.

**Components**:
- **Models**: `Device`, `LocationData`, `Journey`, `Command`, `GpsFrame`, `ResponseMessage`
- **Enums**: `ProtocolType`, `DeviceStatus`, `CommandType`, `CommandStatus`
- **Exceptions**: Custom exception hierarchy for proper error handling

**Characteristics**:
- No external dependencies
- High cohesion, low coupling
- Easy to test in isolation

### 2. Data Layer

**Purpose**: Abstraction of data persistence mechanisms.

**Components**:
- **Generic Repository**: `IRepository<T>` interface with CRUD operations
- **Specialized Repositories**: Type-safe interfaces for each domain model
- **In-Memory Implementations**: Suitable for testing and demos
- **Thread Safety**: `ReaderWriterLockSlim` for concurrent access

**Interfaces**:
```csharp
public interface IRepository<T>
{
    Task<T> AddAsync(T entity);
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
}
```

**Specialized Repositories**:
- `IDeviceRepository` - Device management
- `ILocationDataRepository` - Location history
- `IJourneyRepository` - Journey tracking
- `ICommandRepository` - Command management

### 3. Infrastructure Layer

**Purpose**: Cross-cutting concerns and framework utilities.

**Components**:

#### Validation Pipeline
- Validates input before processing
- Protocol-specific validation rules
- Extensible for custom validations

#### Logging Pipeline
- Structured logging support
- Multiple log levels
- Integration with Microsoft.Extensions.Logging

#### Caching Service
- In-memory cache for frequent queries
- Configurable TTL
- Thread-safe operations

#### Rate Limiting
- Protect from excessive API calls
- Per-device or global limits
- Configurable thresholds

#### Error Handling Middleware
- Centralized exception handling
- Custom error responses
- Logging of errors

### 4. Service Layer

**Purpose**: Business logic orchestration and workflows.

**Key Services**:

#### ProtocolParserService
- Detects protocol from raw data
- Validates frame structure
- Extracts location data
- Handles protocol-specific parsing

#### DeviceService
- Device registration and lifecycle
- Status tracking
- Device queries and filtering

#### LocationDataService
- Store location updates
- Query location history
- Spatial queries (region-based)
- Temporal queries (date range)

#### JourneyService
- Journey creation and completion
- Waypoint management
- Distance and duration calculations
- Route analytics

#### CommandService
- Command creation and scheduling
- Command execution
- Response handling
- History tracking

#### AnalyticsService
- Device analytics (distance, speed, duration)
- Fleet analytics
- Anomaly detection
- Usage patterns

### 5. Integration Layer

**Purpose**: External service integration points.

**Components**:
- **HttpClientFactory**: Configurable HTTP client creation
- **WebhookClient**: Send notifications to external systems
- **GeocodingService**: Reverse geocoding (address lookup)
- **WeatherApiClient**: Weather data integration
- **NotificationService**: Alert delivery
- **SimulationService**: Test data generation

### 6. Formatting Layer

**Purpose**: Convert domain models to various output formats.

**Formatters**:
- **JsonFormatter**: JSON serialization
- **CsvFormatter**: CSV export
- **GeoJsonFormatter**: GeoJSON for mapping

## Dependency Injection

The system uses Microsoft.Extensions.DependencyInjection for composition:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddGpsTrackerServices(
        this IServiceCollection services)
    {
        // Repositories
        services.AddSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
        services.AddSingleton<IDeviceRepository, DeviceRepository>();
        services.AddSingleton<ILocationDataRepository, LocationDataRepository>();

        // Services
        services.AddSingleton<IProtocolParserService, ProtocolParserService>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<ILocationDataService, LocationDataService>();
        services.AddSingleton<IJourneyService, JourneyService>();
        services.AddSingleton<ICommandService, CommandService>();

        // Infrastructure
        services.AddSingleton<ICachingService, CachingService>();
        services.AddSingleton<IRateLimitingService, RateLimitingService>();

        return services;
    }
}
```

## Protocol Parsing Flow

```
Raw Data (TCP/UDP)
       │
       ▼
[1] Protocol Detection
    ├─ Check frame markers (0x78 0x78 for GT06/TK103, $ for H02)
    ├─ Analyze data pattern
    └─ Return ProtocolType
       │
       ▼
[2] Frame Validation
    ├─ Check frame delimiters
    ├─ Verify checksum
    ├─ Validate length
    └─ Ensure data integrity
       │
       ▼
[3] Data Extraction
    ├─ Parse coordinates
    ├─ Extract timestamp
    ├─ Read speed/bearing
    └─ Get satellite count
       │
       ▼
[4] Model Mapping
    └─ Convert to LocationData
       │
       ▼
[5] Storage
    └─ Save to repository
       │
       ▼
LocationData (Structured)
```

## Data Flow Examples

### Device Registration Flow

```
User Input
    │
    ▼
CreateDevice (Domain Model)
    │
    ▼
ValidateDevice
    ├─ Check IMEI format
    ├─ Validate protocol type
    └─ Check for duplicates
    │
    ▼
StoreDevice (Repository)
    │
    ▼
RegisteredDevice (with ID)
```

### Location Update Flow

```
GPS Frame (Raw Data)
    │
    ▼
ParseFrame (ProtocolParserService)
    │
    ▼
LocationData (Extracted)
    │
    ▼
ValidateLocation
    ├─ Check coordinates bounds
    ├─ Verify speed limits
    └─ Validate timestamp
    │
    ▼
StoreLocation (Repository)
    │
    ▼
UpdateDeviceLastUpdate
    │
    ▼
InvalidateCache (for this device)
    │
    ▼
Stored Successfully
```

### Journey Tracking Flow

```
StartJourney
    │
    ▼
Create Journey Entity
    │
    ├─ AddWaypoint (LocationData)
    │   │
    │   ▼
    │   ValidateWaypoint
    │   │
    │   ▼
    │   StoreWaypoint
    │
    └─ (Repeat for each waypoint)
       │
       ▼
CompleteJourney
    │
    ▼
Calculate Metrics
    ├─ Total Distance (haversine formula)
    ├─ Duration (end time - start time)
    ├─ Average Speed (distance / duration)
    └─ Max Speed (from waypoints)
    │
    ▼
StoreJourney
    │
    ▼
JourneyAnalytics
```

## Thread Safety

The system ensures thread-safe operations:

1. **Repository-Level Locking**
   - Each repository uses `ReaderWriterLockSlim`
   - Allows concurrent reads, exclusive writes

2. **Service-Level Coordination**
   - Services compose multiple repository operations
   - Maintains consistency through layered locking

3. **Cache Thread Safety**
   - Caching service handles concurrent access
   - Atomic updates and invalidations

## Performance Considerations

### Memory Optimization
- In-memory repository with indexed collections
- Lazy loading where applicable
- Automatic cache expiration

### Query Optimization
- Index by DeviceId for fast lookups
- Temporal indexes for date-range queries
- Spatial indexing for region queries

### Concurrency
- Non-blocking async/await patterns
- Reader-writer locks for high-read scenarios
- Thread pool optimization

## Error Handling Strategy

```
Exception
    │
    ▼
Caught at Service/Repository Level
    │
    ├─ Check Exception Type
    │
    ├─ ParseException
    │   └─ Frame parsing error
    │
    ├─ ChecksumException
    │   └─ Validation failure
    │
    ├─ DeviceException
    │   └─ Device-related error
    │
    ├─ ValidationException
    │   └─ Data validation failure
    │
    └─ RepositoryException
        └─ Data access error
    │
    ▼
Log Error (with context)
    │
    ▼
Return Null/Default or Throw
    │
    ▼
Caller Handles Result
```

## Extensibility Points

### Adding a New Protocol

1. Add enum value to `ProtocolType`
2. Add protocol constants to `Constants.cs`
3. Implement protocol parsing logic in `ProtocolParserService`
4. Add protocol-specific validation rules

### Adding a New Service

1. Define interface in `Services/IMyService.cs`
2. Implement service: `Services/MyService.cs`
3. Register in `DependencyInjection.cs`
4. Use via dependency injection

### Custom Storage Backend

1. Implement `IRepository<T>` for each domain model
2. Add to service collection in `DependencyInjection.cs`
3. Inject and use in services

## Future Architecture Enhancements

1. **Database Integration** - SQL Server/PostgreSQL repositories
2. **Event Sourcing** - Immutable event log for audit trail
3. **CQRS** - Separate read/write models for optimization
4. **Microservices** - Distributed protocol parsing
5. **Real-time Updates** - WebSocket/SignalR notifications
6. **Message Queue** - RabbitMQ/Kafka integration
7. **Analytics Engine** - Advanced analytics and reporting

## Design Patterns Used

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Dependency Injection** | Global | Loose coupling, testability |
| **Repository** | Data Layer | Data access abstraction |
| **Service** | Service Layer | Business logic encapsulation |
| **Factory** | Integration | Creating complex objects |
| **Adapter** | Integration | External service integration |
| **Decorator** | Infrastructure | Adding behaviors (caching, logging) |
| **Strategy** | Services | Protocol-specific implementations |
| **Pipeline** | Infrastructure | Validation and logging flows |

## Code Organization Principles

1. **Single Responsibility**: Each class has one reason to change
2. **Open/Closed**: Open for extension, closed for modification
3. **Liskov Substitution**: Derived classes substitute base classes
4. **Interface Segregation**: Specific interfaces for specific contracts
5. **Dependency Inversion**: Depend on abstractions, not concretions
