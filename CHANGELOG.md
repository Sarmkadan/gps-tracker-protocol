# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-07-02

### Added
- Add fleet analytics dashboard with route optimization and fuel tracking
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [1.0.0] - 2025-05-02

### Added
- Docker and Docker Compose support with health checks and restart policies
- Comprehensive documentation suite: getting-started, architecture, api-reference, deployment, faq
- Example applications: RealTimeGpsServer, BatchDataImporter, JourneyAnalyzer, DeviceCommandCenter, DataExporter, PerformanceBenchmark, ProtocolConverter
- Makefile for common build and development tasks
- `.editorconfig` for consistent code formatting across editors
- NuGet packaging configuration and GitHub Actions publish workflow
- CodeQL security scanning workflow
- Dependabot configuration for automated dependency updates
- SECURITY.md, CODE_OF_CONDUCT.md, and CONTRIBUTING.md

### Changed
- Promoted to stable 1.0.0 following successful integration testing
- Finalized all public interfaces — no further breaking changes planned for 1.x

### Fixed
- Frame validation error handling for truncated or corrupted data
- Thread safety for concurrent repository write operations
- Cache invalidation on device updates
- Protocol detection edge case with ambiguous frame headers

## [0.9.0] - 2025-04-10

### Added
- Background worker services: `BackgroundProcessingService`, `JourneyAnalyticsWorker`, `LocationAggregationWorker`
- CLI command-line interface (`CommandLineInterface.cs`) for device management
- Integration layer: `GeocodingService`, `WeatherApiClient`, `NotificationService`, `WebhookClient`, `SimulationService`, `HttpClientFactory`
- `CachingService` with configurable TTL and automatic expiration
- `RateLimitingService` to protect against excessive API calls
- `ErrorHandlingMiddleware` for centralized exception handling
- Geofence event processing extensions and `GeofenceEventProcessor`
- `EventPublisher` for internal domain events

### Changed
- Async/await patterns applied consistently across all service methods
- Validation pipeline extended with protocol-specific frame rules
- Logging pipeline upgraded to structured logging format

### Fixed
- Memory leak in long-running location aggregation loop
- Geofence boundary check for devices at exact perimeter coordinates

## [0.7.0] - 2025-03-20

### Added
- `JourneyService` with waypoint recording and trip lifecycle (start / complete)
- `CommandService` for creating, executing, and tracking device commands
- `AnalyticsService` for per-device and fleet-level metrics
- `GeofenceService` for boundary definitions and entry/exit detection
- `CollectionExtensions` and `DictionaryExtensions` LINQ helpers
- `PerformanceMonitor` utility for timing and throughput measurement

### Changed
- `InMemoryRepository<T>` made thread-safe with `ConcurrentDictionary`
- Domain models (`Journey`, `Command`, `ResponseMessage`) finalised with validation attributes

### Fixed
- Journey distance calculation accuracy at high-latitude coordinates
- Device status tracking consistency when device is updated concurrently

## [0.5.0] - 2025-02-28

### Added
- `DeviceService` with device registration, update, and lifecycle management
- `LocationDataService` with store, latest-query, history, date-range, and region queries
- Generic `IRepository<T>` interface and `InMemoryRepository<T>` implementation
- Specialised repositories in `InMemoryRepositories.cs`
- `ValidationPipeline` for input validation across all service entry points
- `LoggingPipeline` with structured console output and log levels
- `GeoJsonFormatter` output formatter alongside existing JSON and CSV formatters

### Changed
- Dependency injection registration consolidated into `DependencyInjection.cs`
- Domain models (`Device`, `LocationData`) extended with audit timestamps

### Fixed
- Coordinate bounds validation (`±90` lat, `±180` lon) not enforced on store
- Null-reference on empty location history result set

## [0.3.0] - 2025-02-01

### Added
- `ProtocolParserService` supporting GT06, H02, and TK103 frame parsing
- Checksum validation: XOR for GT06, NMEA for H02, sum for TK103
- `DetectProtocolAsync` for automatic protocol identification from raw bytes
- `ExtractLocationDataAsync` to map parsed frames to `LocationData` objects
- `ByteExtensions` for reading multi-byte integers and extracting nibbles
- `StringExtensions` for NMEA sentence field splitting and coordinate conversion
- `DateTimeExtensions` for GPS epoch and UTC formatting helpers
- `GpsUtilities` — Haversine distance, bearing, and bounding-box calculations
- `Constants.cs` with frame delimiters, max/min bounds, and timeout values
- `JsonFormatter` and `CsvFormatter` for structured output

### Fixed
- GT06 frame boundary detection when start marker appears inside payload
- H02 checksum rejection for frames with optional trailing fields

## [0.1.0] - 2025-01-15

### Added
- Initial project scaffolding: solution file, `.csproj`, `.gitignore`
- Domain models: `Device`, `LocationData`, `GpsFrame`, `Journey`, `Command`, `ResponseMessage`
- `Enums.cs` — `ProtocolType`, `DeviceStatus`, `CommandStatus`, `JourneyStatus`
- Custom exception hierarchy in `Exceptions.cs`
- Service interface declarations: `IProtocolParserService`, `IDeviceService`, `ILocationDataService`, `IJourneyService`, `ICommandService`
- `IRepository<T>` interface
- Basic `Program.cs` entry point and `ServiceCollection` wiring
- MIT License, initial README

---

## Versioning Notes

- **Major version**: significant API changes or new major capabilities
- **Minor version**: new features with backward compatibility
- **Patch version**: bug fixes and minor improvements

## Upgrade Guide

### From 0.9.0 to 1.0.0

No breaking API changes. Docker and CI/CD configuration added. Examples and documentation expanded.

### From 0.7.0 to 0.9.0

Background workers are now registered as `IHostedService` via `AddGpsTrackerServices()`. Ensure your host calls `builder.Services.AddHostedService<BackgroundProcessingService>()` if you need them.

### From 0.5.0 to 0.7.0

`IJourneyService` and `ICommandService` added to the DI container. No changes to existing interfaces.

### From 0.3.0 to 0.5.0

`IRepository<T>` is now required for service construction. Replace any direct list usage with `InMemoryRepository<T>` or your own implementation.

---

## Known Issues

### 1.0.0
- In-memory repository is not suitable for 100 K+ devices — use a database-backed implementation for production at scale
- WebSocket real-time push not yet implemented (planned for 1.1.0)
- Geofencing requires manual boundary definition; automatic alert delivery is in progress

---

## Future Roadmap

### 1.1.0 (Planned)
- [ ] Real-time WebSocket event push
- [ ] REST API layer with OpenAPI spec
- [ ] Advanced geofencing with configurable alert channels

### 2.0.0 (Future)
- [ ] EF Core database abstraction layer
- [ ] gRPC service interface
- [ ] Advanced analytics and anomaly detection

---

**Latest Version**: 1.0.0 | **Status**: Production Ready | **License**: MIT
