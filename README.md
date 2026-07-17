## PerformanceBenchmark

The `PerformanceBenchmark` class provides comprehensive benchmarking and performance testing capabilities for GPS tracker protocol parsing and storage operations. It includes methods for setting up test environments, parsing various protocol frames (GT06, H02, TK103), protocol detection, frame validation, location data collection, and batch processing.

Example usage for performance benchmarking:
```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

public class PerformanceBenchmarkExample
{
    public async Task RunBenchmarkAsync()
    {
        var benchmark = new PerformanceBenchmark();
        await benchmark.BenchmarkFrameValidationAsync(10000);
        await benchmark.BenchmarkLocationStorageAsync(10000);
        await benchmark.BenchmarkLocationQueryAsync(5000, 500);
        await benchmark.RunStressTestAsync(50, 100);
    }
}
```

## BatchDataImporter

The `BatchDataImporter` class provides functionality for importing large volumes of GPS tracker data from CSV and JSON formats, as well as importing device configuration data. It supports asynchronous batch processing with progress tracking and error handling for efficient data ingestion into the GPS tracker protocol system.

Example usage for importing GPS tracker data:
```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

public class BatchDataImporterExample
{
    public async Task ImportSampleDataAsync()
    {
        // Import CSV data from file
        var csvImporter = new BatchDataImporter();
        await csvImporter.ImportCsvAsync("gps_data.csv");

        // Import JSON data from file
        var jsonImporter = new BatchDataImporter();
        await jsonImporter.ImportJsonAsync("gps_data.json");

        // Import device configurations
        var deviceImporter = new BatchDataImporter();
        await deviceImporter.ImportDevicesAsync("devices_config.csv");
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting batch data import...");
        var importer = new BatchDataImporter();

        // Import sample data
        await importer.ImportCsvAsync("sample_data.csv");

        Console.WriteLine("Batch data import completed successfully!");
    }
}
```

To run the importer from command line:
```bash
dotnet run -- csv gps_data.csv
dotnet run -- json locations.json
dotnet run -- devices devices.csv
```

## ILocationDataService

The `ILocationDataService` interface provides functionality for storing, retrieving, and analyzing GPS location data from tracking devices. It handles operations such as storing new location points, retrieving historical data, finding nearby locations, calculating travel distances, and cleaning up old data records.

Example usage in code:

```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

public class LocationDataServiceExample
{
    private readonly ILocationDataService _locationDataService;

    public LocationDataServiceExample(ILocationDataService locationDataService)
    {
        _locationDataService = locationDataService;
    }

    public async Task ManageLocationDataAsync(string deviceId)
    {
        // Store a new location
        var currentLocation = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 65.5,
            Altitude = 100.0,
            Satellites = 8,
            HDOP = 1.2
        };

        var storedLocation = await _locationDataService.StoreLocationAsync(currentLocation);
        Console.WriteLine($"Stored location at {storedLocation.Timestamp:u}");

        // Get the latest location for a device
        var latestLocation = await _locationDataService.GetLatestLocationAsync(deviceId);
        Console.WriteLine(latestLocation != null
            ? $"Latest location: Lat={latestLocation.Latitude:F4}, Lon={latestLocation.Longitude:F4}"
            : "No location data found");

        // Get location history with limit
        var locationHistory = await _locationDataService.GetLocationHistoryAsync(deviceId, limit: 50);
        Console.WriteLine($"Retrieved {locationHistory.Count()} historical locations");

        // Get locations within a time range
        var startTime = DateTime.UtcNow.AddHours(-24);
        var endTime = DateTime.UtcNow;
        var timeRangeLocations = await _locationDataService.GetLocationsByTimeRangeAsync(deviceId, startTime, endTime);
        Console.WriteLine($"Locations in time range: {timeRangeLocations.Count()} points");

        // Get locations nearby a specific point
        var nearbyLocations = await _locationDataService.GetLocationsNearbyAsync(
            latitude: 40.7128,
            longitude: -74.0060,
            radiusKm: 5.0);
        Console.WriteLine($"Nearby locations (5km radius): {nearbyLocations.Count()} points");

        // Calculate travel distance for a time period
        var distanceKm = await _locationDataService.CalculateTravelDistanceAsync(
            deviceId: deviceId,
            start: DateTime.UtcNow.AddDays(-7),
            end: DateTime.UtcNow);
        Console.WriteLine($"Total travel distance: {distanceKm:F2} km");

        // Cleanup old data
        var cleanupCount = await _locationDataService.CleanupOldDataAsync(
            olderThan: DateTime.UtcNow.AddDays(-90));
        Console.WriteLine($"Cleaned up {cleanupCount} old location records");
    }

    public static async Task Main(string[] args)
    {
        // Example with direct service instantiation
        var unitOfWork = new UnitOfWork();
        var locationDataService = new LocationDataService(unitOfWork);

        Console.WriteLine("Starting location data service example...");
        var example = new LocationDataServiceExample(locationDataService);
        await example.ManageLocationDataAsync("device-001");

        Console.WriteLine("Location data service example completed!");
    }
}
```

## DomainAndServiceTestsValidation

The `DomainAndServiceTestsValidation` class provides validation extension methods for domain models and services used in `DomainAndServiceTests`. It includes validation methods for `LocationData`, `Device`, `GpsFrame`, and `GeofenceService` entities, along with convenience methods to check validity and throw exceptions when validation fails.

This validation helper ensures that domain models meet expected constraints before being used in tests or production code, helping to catch issues early and provide clear error messages when invalid data is encountered.

Example usage for validating domain models and services:

```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Tests;

public class DomainAndServiceTestsValidationExample
{
    public void ValidateDomainModels()
    {
        // Create and validate a LocationData instance
        var location = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 65.5,
            Bearing = 90.0,
            SatelliteCount = 8
        };

        // Validate and get errors
        var locationErrors = location.Validate();
        Console.WriteLine($"Location validation errors: {locationErrors.Count}");

        // Check if valid using IsValid
        bool isValid = location.IsValid();
        Console.WriteLine($"Location is valid: {isValid}");

        // Validate and throw if invalid using EnsureValid
        try
        {
            location.EnsureValid();
            Console.WriteLine("Location passed validation");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }

        // Create and validate a Device instance
        var device = new Device
        {
            Id = "device-001",
            Imei = "123456789012345",
            Name = "GPS Tracker Unit #1",
            Model = "GT06",
            Status = DeviceStatus.Active
        };

        var deviceErrors = device.Validate();
        Console.WriteLine($"Device validation errors: {deviceErrors.Count}");

        // Validate a GpsFrame instance
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = new byte[] { 0x78, 0x78, 0x01, 0x02, 0x03, 0x04 },
            IsValidChecksum = true
        };

        var frameErrors = frame.Validate();
        Console.WriteLine($"GpsFrame validation errors: {frameErrors.Count}");

        // Validate a GeofenceService instance
        var geofenceService = new GeofenceService();
        var serviceErrors = geofenceService.Validate();
        Console.WriteLine($"GeofenceService validation errors: {serviceErrors.Count}");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting DomainAndServiceTestsValidation example...");
        var example = new DomainAndServiceTestsValidationExample();
        example.ValidateDomainModels();
        Console.WriteLine("DomainAndServiceTestsValidation example completed!");
    }
}
```

## DataExporter

The `DataExporter` class provides functionality for exporting GPS location data to JSON, CSV, and GeoJSON formats. It supports exporting location history for specific devices or all devices, making it ideal for data analysis, reporting, and integration with mapping applications.

Example usage for exporting GPS tracker data:
```csharp
using GpsTrackerProtocol.Services;

public class DataExporterExample
{
public async Task ExportSampleDataAsync()
{
    // Create exporter instance
    var exporter = new DataExporter();

    // Export locations to JSON format
    await exporter.ExportToJsonAsync("device-001", "locations.json");

    // Export locations to CSV format
    await exporter.ExportToCsvAsync("device-002", "track.csv");

    // Export locations to GeoJSON format (suitable for mapping libraries)
    await exporter.ExportToGeoJsonAsync("device-003", "map.geojson");

    // Export all devices to JSON
    await exporter.ExportDevicesToJsonAsync("devices.json");
}

public static async Task Main(string[] args)
{
    Console.WriteLine("Starting data export...");
    var exporter = new DataExporter();

    // Export device locations to JSON
    await exporter.ExportToJsonAsync("device-001", "output.json");

    Console.WriteLine("Data export completed successfully!");
}
}
```

To run the exporter from command line:
```bash
```

## JourneyAnalyzer

The `JourneyAnalyzer` class is a command‑line tool that analyzes device journeys, simulates new journeys, and generates fleet‑wide reports. It leverages the GPS tracker services to retrieve device data, compute distances, durations, and speeds, and logs detailed summaries.

Example usage in code:
```csharp
using GpsTrackerProtocol.Services;

var analyzer = new JourneyAnalyzer();

// Analyze a single device
await analyzer.AnalyzeDeviceAsync("device-001");

// Simulate a journey with 15 waypoints
await analyzer.SimulateJourneyAsync("device-001", 15);

// Generate a fleet report for all devices
await analyzer.GenerateFleetReportAsync();
```

Example usage from the command line (the `Main` method parses arguments):
```bash
dotnet run -- analyze device-001
dotnet run -- simulate device-001 20
dotnet run -- fleet
```

## IAnalyticsService

The `IAnalyticsService` interface provides functionality for computing analytics and statistics for GPS tracker devices and fleets. It aggregates location data, journey information, and device metrics to generate comprehensive reports including device-specific analytics, fleet-wide statistics, and route analysis.

Example usage in code:
```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;

public class AnalyticsServiceExample
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsServiceExample(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task ComputeDeviceAnalyticsAsync(string deviceId)
    {
        // Get analytics for a specific device
        var deviceAnalytics = await _analyticsService.GetDeviceAnalyticsAsync(deviceId);
        
        Console.WriteLine($"Device Analytics for {deviceAnalytics.DeviceId}:");
        Console.WriteLine($"  Total Journeys: {deviceAnalytics.TotalJourneys}");
        Console.WriteLine($"  Total Distance: {deviceAnalytics.TotalDistance:F2} km");
        Console.WriteLine($"  Total Duration: {deviceAnalytics.TotalDurationHours:F2} hours");
        Console.WriteLine($"  Average Speed: {deviceAnalytics.AverageSpeed:F2} km/h");
        Console.WriteLine($"  Max Speed: {deviceAnalytics.MaxSpeed:F2} km/h");
        Console.WriteLine($"  Min Speed: {deviceAnalytics.MinSpeed:F2} km/h");
        Console.WriteLine($"  Total Location Points: {deviceAnalytics.TotalLocationPoints}");
        Console.WriteLine($"  Average Satellites: {deviceAnalytics.AverageSatellites:F1}");
        Console.WriteLine($"  Computed At: {deviceAnalytics.ComputedAt:u}");
    }

    public async Task ComputeFleetAnalyticsAsync()
    {
        // Get analytics for the entire fleet
        var fleetAnalytics = await _analyticsService.GetFleetAnalyticsAsync();
        
        Console.WriteLine("Fleet Analytics:");
        Console.WriteLine($"  Total Devices: {fleetAnalytics.TotalDevices}");
        Console.WriteLine($"  Active Devices: {fleetAnalytics.ActiveDevices}");
        Console.WriteLine($"  Total Distance: {fleetAnalytics.TotalDistance:F2} km");
        Console.WriteLine($"  Total Locations: {fleetAnalytics.TotalLocations}");
        Console.WriteLine($"  Average Fleet Speed: {fleetAnalytics.AverageFleetSpeed:F2} km/h");
        Console.WriteLine($"  Computed At: {fleetAnalytics.ComputedAt:u}");
        Console.WriteLine($"  Device Analytics Count: {fleetAnalytics.DeviceAnalytics.Count}");
    }

    public async Task ComputeRouteAnalyticsAsync(string journeyId)
    {
        // Get analytics for a specific route/journey
        var routeAnalytics = await _analyticsService.GetRouteAnalyticsAsync(journeyId);
        
        Console.WriteLine($"Route Analytics for Journey {routeAnalytics.JourneyId}:");
        Console.WriteLine($"  Device: {routeAnalytics.DeviceId}");
        Console.WriteLine($"  Duration: {routeAnalytics.Duration.TotalMinutes:F0} minutes");
        Console.WriteLine($"  Distance: {routeAnalytics.TotalDistance:F2} km");
        Console.WriteLine($"  Waypoints: {routeAnalytics.WaypointCount}");
        Console.WriteLine($"  Average Speed: {routeAnalytics.AverageSpeed:F2} km/h");
        Console.WriteLine($"  Max Speed: {routeAnalytics.MaxSpeed:F2} km/h");
        Console.WriteLine($"  Min Speed: {routeAnalytics.MinSpeed:F2} km/h");
        Console.WriteLine($"  Altitude Range: {routeAnalytics.MinAltitude:F0}m - {routeAnalytics.MaxAltitude:F0}m");
    }

    public static async Task Main(string[] args)
    {
        // Example with direct service instantiation
        var analyticsService = new AnalyticsService(
            new LocationDataService(new UnitOfWork()),
            new JourneyService(new UnitOfWork()),
            new DeviceService(new UnitOfWork()),
            new Logger<AnalyticsService>(new LoggerFactory())
        );

        Console.WriteLine("Starting analytics example...");
        var example = new AnalyticsServiceExample(analyticsService);
        
        await example.ComputeDeviceAnalyticsAsync("device-001");
        await example.ComputeFleetAnalyticsAsync();
        
        Console.WriteLine("Analytics example completed!");
    }
}
```

## IJourneyService

The `IJourneyService` interface provides functionality for managing GPS device journeys and tracking trips. It allows starting new journeys, adding waypoints during journeys, completing journeys, and retrieving journey history. The service also provides utility methods for calculating total distance traveled and cleaning up old journey records.

## IDeviceService

The `IDeviceService` interface provides functionality for managing GPS tracking devices in the system. It handles device registration, status updates, heartbeat monitoring, and device lifecycle management including registration, deregistration, and status queries.

Example usage in code:

```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.Logging;

public class DeviceServiceExample
{
    private readonly IDeviceService _deviceService;

    public DeviceServiceExample(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public async Task ManageDevicesAsync()
    {
        // Register a new GPS device
        var newDevice = new Device
        {
            Imei = "123456789012345",
            Name = "Truck GPS Unit #1",
            Model = "GT06",
            PhoneNumber = "+1234567890",
            Status = DeviceStatus.Active,
            LastHeartbeat = DateTime.UtcNow.AddMinutes(-5),
            CreatedAt = DateTime.UtcNow
        };

        var registeredDevice = await _deviceService.RegisterDeviceAsync(newDevice);
        Console.WriteLine($"Registered device: {registeredDevice.Name} (IMEI: {registeredDevice.Imei})");

        // Get device by ID
        var deviceById = await _deviceService.GetDeviceByIdAsync(registeredDevice.Id);
        Console.WriteLine($"Device found by ID: {deviceById?.Name}");

        // Get device by IMEI
        var deviceByImei = await _deviceService.GetDeviceByImeiAsync("123456789012345");
        Console.WriteLine($"Device found by IMEI: {deviceByImei?.Name}");

        // Get all devices
        var allDevices = await _deviceService.GetAllDevicesAsync();
        Console.WriteLine($"Total devices in system: {allDevices.Count()}");

        // Get online devices
        var onlineDevices = await _deviceService.GetOnlineDevicesAsync();
        Console.WriteLine($"Online devices: {onlineDevices.Count()}");

        // Update device status
        await _deviceService.UpdateDeviceStatusAsync(registeredDevice.Id, DeviceStatus.Maintenance);
        Console.WriteLine("Device status updated to Maintenance");

        // Update device heartbeat
        await _deviceService.UpdateDeviceHeartbeatAsync(registeredDevice.Id);
        Console.WriteLine("Device heartbeat updated");

        // Get device status DTO
        var deviceStatus = await _deviceService.GetDeviceStatusAsync(registeredDevice.Id);
        Console.WriteLine($"Device status: {deviceStatus?.Status}, Last seen: {deviceStatus?.LastSeen:u}");

        // Get all device statuses
        var allStatuses = await _deviceService.GetAllDeviceStatusesAsync();
        Console.WriteLine($"Total device statuses: {allStatuses.Count()}");

        // Update device information
        deviceById.Name = "Truck GPS Unit #1 - Updated";
        var updateSuccess = await _deviceService.UpdateDeviceAsync(deviceById);
        Console.WriteLine($"Device update successful: {updateSuccess}");

        // Deregister device
        var deregisterSuccess = await _deviceService.DeregisterDeviceAsync(registeredDevice.Id);
        Console.WriteLine($"Device deregistered: {deregisterSuccess}");
    }

    public static async Task Main(string[] args)
    {
        // Example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var unitOfWork = new UnitOfWork();
        var deviceService = new DeviceService(unitOfWork, loggerFactory.CreateLogger<DeviceService>());

        Console.WriteLine("Starting device service example...");
        var example = new DeviceServiceExample(deviceService);
        await example.ManageDevicesAsync();

        Console.WriteLine("Device service example completed!");
    }
}
```

## IFleetDashboardService

The `IFleetDashboardService` interface provides functionality for managing a fleet of vehicles and generating real-time analytics dashboards. It allows registering vehicles, tracking their status, computing optimized routes, and generating fleet-wide KPI metrics by aggregating data from GPS devices, location history, and fuel tracking services.

Example usage in code:

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using Microsoft.Extensions.Logging;

public class FleetDashboardServiceExample
{
    private readonly IFleetDashboardService _fleetDashboardService;

    public FleetDashboardServiceExample(IFleetDashboardService fleetDashboardService)
    {
        _fleetDashboardService = fleetDashboardService;
    }

    public async Task ManageFleetAsync()
    {
        // Register a new vehicle in the fleet
        var newVehicle = new FleetVehicle
        {
            RegistrationNumber = "ABC-123",
            DeviceId = "gps-device-001",
            Make = "Toyota",
            Model = "Hilux",
            Year = 2022,
            FuelType = FuelType.Diesel,
            TankCapacityLiters = 80.0,
            BaseConsumptionLper100km = 7.5
        };

        var registeredVehicle = await _fleetDashboardService.RegisterVehicleAsync(newVehicle);
        Console.WriteLine($"Registered vehicle: {registeredVehicle.RegistrationNumber} (ID: {registeredVehicle.Id})");

        // Get a specific vehicle
        var vehicle = await _fleetDashboardService.GetVehicleAsync(registeredVehicle.Id);
        Console.WriteLine($"Vehicle found: {vehicle?.RegistrationNumber}");

        // Get all vehicles in fleet
        var allVehicles = await _fleetDashboardService.GetAllVehiclesAsync();
        Console.WriteLine($"Total fleet size: {allVehicles.Count()}");

        // Update vehicle information
        vehicle.FuelType = FuelType.Petrol;
        var updatedVehicle = await _fleetDashboardService.UpdateVehicleAsync(vehicle);
        Console.WriteLine($"Updated vehicle fuel type to: {updatedVehicle.FuelType}");

        // Get vehicle status
        var status = await _fleetDashboardService.GetVehicleStatusAsync(registeredVehicle.Id);
        Console.WriteLine($"Vehicle status: {status.Status}, Current location: ({status.CurrentLatitude}, {status.CurrentLongitude})");

        // Generate dashboard snapshot
        var dashboard = await _fleetDashboardService.GetDashboardSnapshotAsync();
        Console.WriteLine($"Dashboard generated at {dashboard.GeneratedAt:u}");
        Console.WriteLine($"Total vehicles: {dashboard.TotalVehicles}");
        Console.WriteLine($"Active vehicles: {dashboard.ActiveVehicles}");
        Console.WriteLine($"Total fleet distance: {dashboard.TotalFleetDistanceKm} km");

        // Compute fleet KPIs for a specific period
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var kpis = await _fleetDashboardService.ComputeFleetKpisAsync(from, to);
        Console.WriteLine($"Average fleet efficiency: {kpis["fleet.avg_consumption_l_per_100km"]} L/100km");

        // Optimize a route for the vehicle
        var routeStops = new List<RouteStop>
        {
            new RouteStop { Name = "Warehouse A", Latitude = 40.7128, Longitude = -74.0060, Order = 1 },
            new RouteStop { Name = "Customer 1", Latitude = 40.7306, Longitude = -73.9352, Order = 2 },
            new RouteStop { Name = "Customer 2", Latitude = 40.7589, Longitude = -73.9851, Order = 3 }
        };

        var optimizedRoute = await _fleetDashboardService.OptimizeRouteAsync(
            registeredVehicle.Id, 
            routeStops,
            RouteOptimizationAlgorithm.GeneticAlgorithm
        );
        Console.WriteLine($"Optimized route distance: {optimizedRoute.TotalDistanceKm} km");
        Console.WriteLine($"Estimated route duration: {optimizedRoute.EstimatedDuration.TotalMinutes} minutes");

        // Remove vehicle from fleet
        var removed = await _fleetDashboardService.RemoveVehicleAsync(registeredVehicle.Id);
        Console.WriteLine("Vehicle removed: {removed}");
    }

    public static async Task Main(string[] args)
    {
        // Example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var unitOfWork = new UnitOfWork();
        var fleetDashboardService = new FleetDashboardService(
            new DeviceService(unitOfWork),
            new LocationDataService(unitOfWork),
            new FuelTrackingService(loggerFactory.CreateLogger<FuelTrackingService>()),
            new RouteOptimizationEngine(),
            new FleetDashboardOptions { MaxFleetSize = 100 },
            loggerFactory.CreateLogger<FleetDashboardService>()
        );

        Console.WriteLine("Starting fleet dashboard service example...");
        var example = new FleetDashboardServiceExample(fleetDashboardService);
        await example.ManageFleetAsync();

        Console.WriteLine("Fleet dashboard service example completed!");
    }
}
```

## FleetDashboardOptions

The `FleetDashboardOptions` class provides configuration settings for the fleet analytics dashboard feature. It controls route optimization behavior, fuel price assumptions, caching policies, and fleet-wide limits that affect how the dashboard computes KPIs, generates snapshots, and estimates fuel consumption.

Example usage for configuring fleet dashboard options:

```csharp
using GpsTrackerProtocol.Configuration;
using GpsTrackerProtocol.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

public class FleetDashboardOptionsExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configure fleet dashboard options with custom settings
        services.AddFleetAnalyticsDashboard(opts =>
        {
            opts.DefaultAlgorithm = RouteOptimizationAlgorithm.GeneticAlgorithm;
            opts.DefaultFuelPricePerLiter = 1.79;
            opts.AverageRoadSpeedKmh = 60.0;
            opts.MaxStopsPerRoute = 150;
            opts.MaxFleetSize = 50;
            opts.SnapshotCacheTtl = TimeSpan.FromMinutes(1);
            opts.EnableDistanceBasedFallback = false;
            opts.LowFuelThresholdLiters = 15.0;
        });
    }

    public static void ConfigureFromAppSettings()
    {
        // Alternatively, configure via appsettings.json:
        // {
        //   "FleetDashboard": {
        //     "DefaultAlgorithm": "GeneticAlgorithm",
        //     "DefaultFuelPricePerLiter": 1.79,
        //     "AverageRoadSpeedKmh": 60.0,
        //     "MaxStopsPerRoute": 150,
        //     "MaxFleetSize": 50,
        //     "SnapshotCacheTtl": "00:01:00",
        //     "EnableDistanceBasedFallback": false,
        //     "LowFuelThresholdLiters": 15.0
        //   }
        // }
    }
}
```

## GpsTrackerProtocolOptions

The `GpsTrackerProtocolOptions` class provides centralized configuration for the GPS tracker protocol system. It controls global settings such as protocol-specific behaviors (GT06, H02, TK103), rate limiting, caching policies, location history limits, and logging configuration that affect how the system processes incoming GPS tracker data from various device protocols.

Example usage for configuring GPS tracker protocol options:

```csharp
using GpsTrackerProtocol.Configuration;
using GpsTrackerProtocol.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

public class GpsTrackerProtocolOptionsExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configure GPS tracker protocol options with custom settings
        services.ConfigureGpsTrackerProtocolOptions(opts =>
        {
            opts.DefaultProtocol = ProtocolType.GT06;
            opts.MaxDevices = 1000;
            opts.LocationHistoryLimit = 10000;
            opts.CacheExpirationMinutes = 30;
            opts.RateLimitPerMinute = 600;
            opts.LoggingLevel = "Information";
            
            // GT06 protocol settings
            opts.GT06Enabled = true;
            opts.GT06Timeout = 30;
            opts.GT06MaxFrameSize = 1024;
            
            // H02 protocol settings
            opts.H02Enabled = true;
            opts.H02Timeout = 25;
            opts.H02MaxFrameSize = 512;
            
            // TK103 protocol settings
            opts.TK103Enabled = true;
            opts.TK103Timeout = 20;
            opts.TK103MaxFrameSize = 256;
            
            // Protocol-specific settings
            opts.Protocol = new ProtocolSettings
            {
                HeartbeatInterval = TimeSpan.FromMinutes(5),
                MaxSpeedKmh = 220.0,
                MinSpeedKmh = 0.1,
                MaxAltitudeMeters = 5000.0,
                MinSatellites = 4
            };
        });
    }

    public static void ConfigureFromAppSettings()
    {
        // Alternatively, configure via appsettings.json:
        // {
        //   "GpsTrackerProtocol": {
        //     "DefaultProtocol": "GT06",
        //     "MaxDevices": 1000,
        //     "LocationHistoryLimit": 10000,
        //     "CacheExpirationMinutes": 30,
        //     "RateLimitPerMinute": 600,
        //     "LoggingLevel": "Information",
        //     "GT06Enabled": true,
        //     "GT06Timeout": 30,
        //     "GT06MaxFrameSize": 1024,
        //     "H02Enabled": true,
        //     "H02Timeout": 25,
        //     "H02MaxFrameSize": 512,
        //     "TK103Enabled": true,
        //     "TK103Timeout": 20,
        //     "TK103MaxFrameSize": 256,
        //     "Protocol": {
        //       "HeartbeatInterval": "00:05:00",
        //       "MaxSpeedKmh": 220.0,
        //       "MinSpeedKmh": 0.1,
        //       "MaxAltitudeMeters": 5000.0,
        //       "MinSatellites": 4
        //     }
        //   }
        // }
    }
}
```

## CommandServiceTestsExtensions

The `CommandServiceTestsExtensions` class provides factory methods and extension utilities for creating test entities in `CommandServiceTests`. It simplifies test setup by offering convenient methods to generate realistic `Command`, `Device`, and `CommandResponse` objects with proper defaults, reducing boilerplate code in unit tests.

Example usage in tests:

```csharp
using gps_tracker_protocol.Tests;
using GpsTrackerProtocol.Domain.Models;

public class CommandServiceTestsExample
{
    private readonly CommandServiceTests _tests = new();

    public void CreateTestEntities()
    {
        // Create a test device
        var device = _tests.CreateDevice(
            deviceId: "device-001",
            imei: "123456789012345",
            deviceName: "Test GPS Device",
            isActive: true
        );
        Console.WriteLine($"Created device: {device.DeviceName} (IMEI: {device.Imei})");

        // Create a sent command
        var sentCommand = _tests.CreateSentCommand(
            deviceId: device.Id,
            commandType: "REBOOT",
            payload: "{ \"delay\": 30 }"
        );
        Console.WriteLine($"Created sent command: {sentCommand.CommandType} (ID: {sentCommand.Id})");

        // Create a pending command
        var pendingCommand = _tests.CreateCommand(
            deviceId: device.Id,
            commandType: "SET_CONFIG",
            payload: "{ \"speedThreshold\": 120 }"
        );
        Console.WriteLine($"Created pending command: {pendingCommand.CommandType}");

        // Create a list of commands for batch testing
        var commandList = _tests.CreateCommandList(
            deviceId: device.Id,
            count: 5,
            commandType: "BATCH_TEST"
        );
        Console.WriteLine($"Created {commandList.Count} commands for batch testing");

        // Create a command response
        var response = _tests.CreateCommandResponse(
            commandId: pendingCommand.Id,
            success: true,
            responseData: "{\"status\": \"reboot_initiated\"}"
        );
        Console.WriteLine($"Created command response: Success={response.Success}, Data={response.ResponseData}");
    }
}
```

## IJourneyService

Example usage in code:

## IGeofenceEventProcessor

The `IGeofenceEventProcessor` interface processes GPS device location updates to detect geofence boundary crossings (entering or exiting geographic zones). It maintains device state, tracks dwell times within geofences, and can notify subscribers via webhooks or internal event bus when boundary transitions occur.

Example usage in code:

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using Microsoft.Extensions.Logging;

public class GeofenceEventProcessorExample
{
    private readonly IGeofenceEventProcessor _geofenceEventProcessor;
    private readonly IGeofenceService _geofenceService;

    public GeofenceEventProcessorExample(
        IGeofenceEventProcessor geofenceEventProcessor,
        IGeofenceService geofenceService)
    {
        _geofenceEventProcessor = geofenceEventProcessor;
        _geofenceService = geofenceService;
    }

    public void SetupGeofencesAndWebhooks()
    {
        // Create geofences for important areas
        _geofenceService.AddGeofence(
            id: "warehouse-nyc",
            centerLat: 40.7128,
            centerLon: -74.0060,
            radiusKm: 2.5);

        _geofenceService.AddGeofence(
            id: "restricted-zone",
            centerLat: 40.7306,
            centerLon: -73.9352,
            radiusKm: 1.0);

        // Register a webhook for a device to receive geofence events
        _geofenceEventProcessor.RegisterWebhook(
            deviceId: "truck-001",
            webhookUrl: "https://api.example.com/webhooks/gps-events");
    }

    public async Task ProcessDeviceLocationAsync(string deviceId, double latitude, double longitude)
    {
        // Process a location update to detect geofence transitions
        var location = new LocationData
        {
            DeviceId = deviceId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow,
            Speed = 65.5,
            Satellites = 8
        };

        await _geofenceEventProcessor.ProcessLocationAsync(location);

        // Get current geofences for the device
        var currentGeofences = _geofenceEventProcessor.GetCurrentGeofences(deviceId);
        Console.WriteLine($"Device {deviceId} is currently inside {currentGeofences.Count} geofence(s)");

        foreach (var geofenceId in currentGeofences)
        {
            Console.WriteLine($"  - {geofenceId}");
        }
    }

    public void UnregisterDevice(string deviceId)
    {
        // Remove webhook subscription when device is no longer tracked
        _geofenceEventProcessor.UnregisterWebhook(deviceId);
    }

    public static async Task Main(string[] args)
    {
        // Setup example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var geofenceService = new GeofenceService(loggerFactory.CreateLogger<GeofenceService>());
        var webhookClient = new WebhookClient();
        var eventPublisher = new EventPublisher();
        var notificationService = new NotificationService();
        var geofenceEventProcessor = new GeofenceEventProcessor(
            geofenceService,
            webhookClient,
            eventPublisher,
            notificationService,
            loggerFactory.CreateLogger<GeofenceEventProcessor>());

        Console.WriteLine("Starting geofence event processor example...");
        var example = new GeofenceEventProcessorExample(geofenceEventProcessor, geofenceService);

        example.SetupGeofencesAndWebhooks();
        
        // Process location updates for a device
        await example.ProcessDeviceLocationAsync("truck-001", 40.7150, -74.0075);
        await example.ProcessDeviceLocationAsync("truck-001", 40.7350, -73.9400);

        // Check current geofences
        var current = geofenceEventProcessor.GetCurrentGeofences("truck-001");
        Console.WriteLine($"Final state: {current.Count} geofences");

        Console.WriteLine("Geofence event processor example completed!");
    }
}
```

## IFuelTrackingService

The `IFuelTrackingService` interface provides functionality for recording and reporting fuel events for fleet vehicles. It supports explicit event recording (consumption, refuel, drain) as well as distance-based consumption estimation. The service allows tracking fuel consumption over specific time periods, calculating average fuel efficiency, and estimating fuel requirements for planned trips.

Example usage in code:

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using Microsoft.Extensions.Logging;

public class FuelTrackingServiceExample
{
    private readonly IFuelTrackingService _fuelTrackingService;

    public FuelTrackingServiceExample(IFuelTrackingService fuelTrackingService)
    {
        _fuelTrackingService = fuelTrackingService;
    }

    public async Task ManageFuelTrackingAsync(string vehicleId, string deviceId)
    {
        // Record a fuel consumption event
        var consumptionEvent = new FuelRecord
        {
            VehicleId = vehicleId,
            DeviceId = deviceId,
            EventType = FuelEventType.Consumption,
            FuelAmountLiters = 45.5,
            OdometerKm = 12500.0,
            Timestamp = DateTime.UtcNow,
            TotalCost = 72.80,
            Location = new LocationData
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Timestamp = DateTime.UtcNow
            }
        };

        var recordedConsumption = await _fuelTrackingService.RecordFuelEventAsync(consumptionEvent);
        Console.WriteLine($"Recorded fuel consumption: {recordedConsumption.FuelAmountLiters:F2} L");

        // Record a refuel event
        var refuelEvent = new FuelRecord
        {
            VehicleId = vehicleId,
            DeviceId = deviceId,
            EventType = FuelEventType.Refuel,
            FuelAmountLiters = 60.0,
            OdometerKm = 12545.5,
            Timestamp = DateTime.UtcNow.AddMinutes(30),
            TotalCost = 96.00,
            Location = new LocationData
            {
                Latitude = 40.7306,
                Longitude = -73.9352,
                Timestamp = DateTime.UtcNow.AddMinutes(30)
            }
        };

        var recordedRefuel = await _fuelTrackingService.RecordFuelEventAsync(refuelEvent);
        Console.WriteLine($"Recorded refuel: {recordedRefuel.FuelAmountLiters:F2} L");

        // Get all fuel records for the vehicle
        var allRecords = await _fuelTrackingService.GetRecordsAsync(vehicleId);
        Console.WriteLine($"Total fuel records: {allRecords.Count()}");

        // Get consumption records only
        var consumptionRecords = await _fuelTrackingService.GetRecordsAsync(vehicleId, FuelEventType.Consumption);
        Console.WriteLine($"Consumption events: {consumptionRecords.Count()}");

        // Generate a fuel consumption report for the last 7 days
        var report = await _fuelTrackingService.GetReportAsync(
            vehicleId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow
        );

        Console.WriteLine($"Fuel Report for {report.VehicleId}:");
        Console.WriteLine($"  Period: {report.PeriodStart:d} to {report.PeriodEnd:d}");
        Console.WriteLine($"  Total Distance: {report.TotalDistanceKm:F2} km");
        Console.WriteLine($"  Fuel Consumed: {report.TotalFuelConsumedLiters:F2} L");
        Console.WriteLine($"  Average Consumption: {report.AverageConsumptionLper100km:F2} L/100km");
        Console.WriteLine($"  Total Cost: ${report.TotalCost:F2}");
        Console.WriteLine($"  Refuel Count: {report.RefuelCount}");

        // Estimate fuel needed for a planned trip
        var tripDistance = 320.0; // km
        var consumptionRate = 8.5; // L/100km
        var estimatedFuel = _fuelTrackingService.EstimateFuelLiters(tripDistance, consumptionRate);
        Console.WriteLine($"Estimated fuel for {tripDistance} km trip: {estimatedFuel:F2} L");

        // Delete a specific record (e.g., if it was entered in error)
        var deleteSuccess = await _fuelTrackingService.DeleteRecordAsync(recordedConsumption.Id);
        Console.WriteLine($"Record deletion successful: {deleteSuccess}");
    }

    public static async Task Main(string[] args)
    {
        // Example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var fuelTrackingService = new FuelTrackingService(loggerFactory.CreateLogger<FuelTrackingService>());

        Console.WriteLine("Starting fuel tracking example...");
        var example = new FuelTrackingServiceExample(fuelTrackingService);
        await example.ManageFuelTrackingAsync("truck-001", "device-gps-001");

        Console.WriteLine("Fuel tracking example completed!");
    }
}
```

## IGeofenceService

The `IGeofenceService` interface provides functionality for managing geographic boundary zones (geofences) and detecting when GPS devices enter or exit these zones. It allows adding circular geofences with specified centers and radii, checking if a location is inside a geofence, and finding nearby geofences for a given location.

Example usage in code:

```csharp

## IGeofenceAlertingService

The `IGeofenceAlertingService` interface manages geofence alert rules and the alerts they produce. It allows creating alert rules that trigger when devices cross geofence boundaries, retrieving active and historical alerts, and acknowledging alerts once they've been addressed.

Example usage in code:

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;
using Microsoft.Extensions.Logging;

public class GeofenceAlertingServiceExample
{
    private readonly IGeofenceAlertingService _geofenceAlertingService;
    private readonly IGeofenceService _geofenceService;
    private readonly IEventPublisher _eventPublisher;

    public GeofenceAlertingServiceExample(
        IGeofenceAlertingService geofenceAlertingService,
        IGeofenceService geofenceService,
        IEventPublisher eventPublisher)
    {
        _geofenceAlertingService = geofenceAlertingService;
        _geofenceService = geofenceService;
        _eventPublisher = eventPublisher;
    }

    public void SetupAlertRules()
    {
        // Create a geofence for a restricted area
        _geofenceService.AddGeofence(
            id: "restricted-zone",
            centerLat: 40.7128,
            centerLon: -74.0060,
            radiusKm: 1.0);

        // Create an alert rule for when device enters the restricted zone
        var enterRule = _geofenceAlertingService.CreateAlertRule(
            deviceId: "truck-001",
            geofenceId: "restricted-zone",
            alertType: GeofenceAlertType.Enter,
            cooldown: TimeSpan.FromMinutes(10),
            description: "Alert when truck enters restricted zone");

        Console.WriteLine($"Created enter alert rule: {enterRule.Id}");

        // Create an alert rule for when device exits the restricted zone
        var exitRule = _geofenceAlertingService.CreateAlertRule(
            deviceId: "truck-001",
            geofenceId: "restricted-zone",
            alertType: GeofenceAlertType.Exit,
            cooldown: TimeSpan.FromMinutes(10),
            description: "Alert when truck exits restricted zone");

        Console.WriteLine($"Created exit alert rule: {exitRule.Id}");
    }

    public void CheckActiveAlerts(string deviceId)
    {
        // Get all active alerts for a device
        var activeAlerts = _geofenceAlertingService.GetActiveAlerts(deviceId);
        Console.WriteLine($"Active alerts for {deviceId}: {activeAlerts.Count}");

        foreach (var alert in activeAlerts)
        {
            Console.WriteLine($"  Alert {alert.Id}: {alert.AlertType} at {alert.FiredAt:u}");
        }
    }

    public void CheckAlertHistory(string deviceId)
    {
        // Get recent alert history
        var alertHistory = _geofenceAlertingService.GetAlertHistory(deviceId, limit: 10);
        Console.WriteLine($"Alert history for {deviceId}: {alertHistory.Count} entries");

        foreach (var alert in alertHistory)
        {
            Console.WriteLine($"  {alert.FiredAt:u}: {alert.AlertType} - {alert.Status}");
        }
    }

    public void AcknowledgeAlert(string alertId)
    {
        // Acknowledge an alert once it's been addressed
        bool acknowledged = _geofenceAlertingService.AcknowledgeAlert(
            alertId: alertId,
            notes: "False alarm - driver had permission to enter");
        
        Console.WriteLine($"Alert {alertId} acknowledged: {acknowledged}");
    }

    public void DeleteAlertRuleExample(string ruleId)
    {
        // Remove an alert rule
        _geofenceAlertingService.DeleteAlertRule(ruleId);
        Console.WriteLine($"Alert rule {ruleId} deleted");
    }

    public static void Main(string[] args)
    {
        // Setup example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var eventPublisher = new EventPublisher();
        var geofenceService = new GeofenceService(loggerFactory.CreateLogger<GeofenceService>());
        var geofenceAlertingService = new GeofenceAlertingService(eventPublisher, 
            loggerFactory.CreateLogger<GeofenceAlertingService>());

        Console.WriteLine("Starting geofence alerting service example...");
        var example = new GeofenceAlertingServiceExample(
            geofenceAlertingService, 
            geofenceService, 
            eventPublisher);
        
        example.SetupAlertRules();
        example.CheckActiveAlerts("truck-001");
        example.CheckAlertHistory("truck-001");

        Console.WriteLine("Geofence alerting service example completed!");
    }
}
```

## IGeofenceService
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.Logging;

public class GeofenceServiceExample
{
    private readonly IGeofenceService _geofenceService;

    public GeofenceServiceExample(IGeofenceService geofenceService)
    {
        _geofenceService = geofenceService;
    }

    public void ManageGeofences()
    {
        // Add a geofence for a warehouse location
        _geofenceService.AddGeofence(
            id: "warehouse-nyc",
            centerLat: 40.7128,
            centerLon: -74.0060,
            radiusKm: 2.5);

        // Add another geofence for a delivery zone
        _geofenceService.AddGeofence(
            id: "delivery-zone",
            centerLat: 40.7306,
            centerLon: -73.9352,
            radiusKm: 5.0);

        // Check if a device is inside a specific geofence
        bool isInside = _geofenceService.IsInsideGeofence(
            geofenceId: "warehouse-nyc",
            latitude: 40.7150,
            longitude: -74.0075);
        
        Console.WriteLine($"Device is inside warehouse geofence: {isInside}");

        // Get all geofences within a search radius of a location
        var nearbyGeofences = _geofenceService.GetNearbyGeofences(
            latitude: 40.7200,
            longitude: -73.9900,
            searchRadiusKm: 10.0);

        Console.WriteLine($"Nearby geofences: {string.Join(", ", nearbyGeofences)}");
    }

    public static void Main(string[] args)
    {
        // Example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var geofenceService = new GeofenceService(loggerFactory.CreateLogger<GeofenceService>());

        Console.WriteLine("Starting geofence service example...");
        var example = new GeofenceServiceExample(geofenceService);
        example.ManageGeofences();

        Console.WriteLine("Geofence service example completed!");
    }
}
```

## IProtocolHandler

The `IProtocolHandler` interface defines the contract for parsing and processing GPS tracker protocol data. Each protocol handler is responsible for:

- Identifying its supported protocol type via the `Protocol` property
- Determining if incoming data matches the protocol signature using `CanHandle(byte[] preamble)`
- Parsing raw device data into structured `GpsFrame` objects via `CreateFrameAsync(byte[] data, string sourceAddress)`

Protocol handlers are registered with the `ProtocolAutoDetector` service, which uses the `CanHandle` method to route incoming data to the appropriate handler based on protocol signatures.

Example usage in code:

```csharp
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.Logging;

public class ProtocolHandlerExample
{
    private readonly IProtocolAutoDetector _protocolAutoDetector;
    private readonly ILogger<ProtocolHandlerExample> _logger;

    public ProtocolHandlerExample(IProtocolAutoDetector protocolAutoDetector, ILogger<ProtocolHandlerExample> logger)
    {
        _protocolAutoDetector = protocolAutoDetector;
        _logger = logger;
    }

    public async Task ProcessIncomingDataAsync(byte[] rawData, string sourceAddress)
    {
        // Detect the protocol from the first few bytes
        var detectedProtocol = _protocolAutoDetector.Detect(rawData);
        _logger.LogInformation("Detected protocol: {Protocol}", detectedProtocol);

        // Get the appropriate handler for this protocol
        var handler = _protocolAutoDetector.GetHandler(rawData);
        
        if (handler != null)
        {
            // Create a structured GPS frame from the raw data
            var gpsFrame = await handler.CreateFrameAsync(rawData, sourceAddress);
            
            _logger.LogInformation("Created GPS frame for protocol {Protocol} from {Source}", 
                gpsFrame.Protocol, gpsFrame.SourceAddress);
            _logger.LogInformation("Frame contains {DataLength} bytes received at {ReceivedAt}",
                gpsFrame.RawData.Length, gpsFrame.ReceivedAt);
        }
        else
        {
            _logger.LogWarning("No handler found for protocol type: {Protocol}", detectedProtocol);
        }
    }

    public static async Task Main(string[] args)
    {
        // Setup example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ProtocolHandlerExample>();
        
        // Create protocol handlers for supported protocols
        var gt06Handler = new GT06ProtocolHandler();
        var h02Handler = new H02ProtocolHandler();
        var tk103Handler = new TK103ProtocolHandler();
        
        // Create the protocol auto-detector with registered handlers
        var protocolAutoDetector = new ProtocolAutoDetector(
            new List<IProtocolHandler> { gt06Handler, h02Handler, tk103Handler },
            loggerFactory.CreateLogger<ProtocolAutoDetector>(),
            ProtocolType.GT06 // Default protocol if no match found
        );

        var example = new ProtocolHandlerExample(protocolAutoDetector, logger);
        
        // Example 1: GT06 protocol data (starts with 0x78 0x78)
        var gt06Data = new byte[] { 0x78, 0x78, 0x01, 0x02, 0x03, 0x04 };
        await example.ProcessIncomingDataAsync(gt06Data, "192.168.1.100:5000");
        
        // Example 2: H02 protocol data (starts with *HQ)
        var h02Data = System.Text.Encoding.ASCII.GetBytes("*HQ,123456,1234,123456,0,1,1,110129.083211,A,3642.7718,N,13929.8534,E,0.000,0.00,0,120312,FFFFFBFF,1,25,2,1,1,1,1,1,1,1,1*3B");
        await example.ProcessIncomingDataAsync(h02Data, "192.168.1.101:5001");
        
        // Example 3: TK103 protocol data (starts with '(')
        var tk103Data = System.Text.Encoding.ASCII.GetBytes("(123456789012345,GPRMC,120312,083211,A,3642.7718,N,13929.8534,E,0.000,0.00,0,120312,0,0,0*7D");
        await example.ProcessIncomingDataAsync(tk103Data, "192.168.1.102:5002");
        
        Console.WriteLine("Protocol handler example completed!");
    }
}

```

## InMemoryRepository

The `InMemoryRepository<T>` class provides a thread-safe, in-memory repository implementation designed for testing, prototyping, and demonstration purposes. It implements the core `IRepository<T>` interface with basic CRUD operations and includes additional specialized methods for location data management when used with `LocationData` entities.

This repository uses a `Dictionary<string, T>` as its backing store with a `ReaderWriterLockSlim` to ensure thread-safe concurrent access. All operations are asynchronous and include proper locking to prevent race conditions.

Example usage for basic repository operations:

```csharp
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain.Models;

public class InMemoryRepositoryExample
{
    public async Task ManageEntitiesAsync()
    {
        // Create repository instance
        var repository = new InMemoryRepository<LocationData>();
        
        // Create and store entities
        var location1 = new LocationData
        {
            Id = "loc-001",
            DeviceId = "device-001",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 65.5,
            Altitude = 100.0,
            Satellites = 8,
            HDOP = 1.2
        };
        
        var location2 = new LocationData
        {
            Id = "loc-002",
            DeviceId = "device-001",
            Latitude = 40.7306,
            Longitude = -73.9352,
            Timestamp = DateTime.UtcNow.AddMinutes(-30),
            Speed = 55.0,
            Altitude = 120.0,
            Satellites = 7,
            HDOP = 1.5
        };
        
        await repository.CreateAsync(location1);
        await repository.CreateAsync(location2);
        
        Console.WriteLine("Created 2 location entities");
        
        // Retrieve entities
        var allLocations = await repository.GetAllAsync();
        Console.WriteLine($"Total entities: {allLocations.Count()}");
        
        var byId = await repository.GetByIdAsync("loc-001");
        Console.WriteLine($"Retrieved entity: {byId?.Id}");
        
        // Update entity
        location1.Speed = 70.2;
        var updated = await repository.UpdateAsync(location1);
        Console.WriteLine($"Updated entity speed to: {updated.Speed}");
        
        // Check existence
        var exists = await repository.ExistsAsync("loc-001");
        Console.WriteLine($"Entity exists: {exists}");
        
        // Delete entity
        var deleteSuccess = await repository.DeleteAsync("loc-002");
        Console.WriteLine($"Delete successful: {deleteSuccess}");
        
        // Get remaining entities
        var remaining = await repository.GetAllAsync();
        Console.WriteLine($"Remaining entities: {remaining.Count()}");
    }
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting in-memory repository example...");
        var example = new InMemoryRepositoryExample();
        await example.ManageEntitiesAsync();
        Console.WriteLine("In-memory repository example completed!");
    }
}
```

Example usage for location-specific operations with `InMemoryLocationDataRepository`:

```csharp
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain.Models;

public class LocationRepositoryExample
{
    public async Task QueryLocationDataAsync()
    {
        // Create location-specific repository
        var locationRepo = new InMemoryLocationDataRepository();
        
        // Add some location data
        var deviceId = "truck-gps-001";
        
        for (int i = 0; i < 10; i++)
        {
            await locationRepo.CreateAsync(new LocationData
            {
                Id = $"loc-{i:D3}",
                DeviceId = deviceId,
                Latitude = 40.7128 + (Random.Shared.NextDouble() * 0.1 - 0.05),
                Longitude = -74.0060 + (Random.Shared.NextDouble() * 0.1 - 0.05),
                Timestamp = DateTime.UtcNow.AddHours(-i),
                Speed = Random.Shared.Next(40, 80),
                Satellites = Random.Shared.Next(6, 12)
            });
        }
        
        // Query by device ID
        var deviceLocations = await locationRepo.GetByDeviceIdAsync(deviceId);
        Console.WriteLine($"Locations for device {deviceId}: {deviceLocations.Count()}");
        
        // Get latest location
        var latest = await locationRepo.GetLatestByDeviceIdAsync(deviceId);
        Console.WriteLine($"Latest location: Lat={latest?.Latitude:F4}, Lon={latest?.Longitude:F4}");
        
        // Query by time range
        var startTime = DateTime.UtcNow.AddHours(-5);
        var endTime = DateTime.UtcNow;
        var timeRangeLocations = await locationRepo.GetByTimeRangeAsync(startTime, endTime);
        Console.WriteLine($"Locations in time range: {timeRangeLocations.Count()}");
        
        // Query by device and time range
        var deviceAndTimeRange = await locationRepo.GetByDeviceAndTimeRangeAsync(
            deviceId, 
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow
        );
        Console.WriteLine($"Device locations in time range: {deviceAndTimeRange.Count()}");
        
        // Get locations within radius
        var nearby = await locationRepo.GetWithinRadiusAsync(
            latitude: 40.7128,
            longitude: -74.0060,
            radiusKm: 10.0
        );
        Console.WriteLine($"Locations within 10km radius: {nearby.Count()}");
        
        // Delete old data
        var deletedCount = await locationRepo.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30));
        Console.WriteLine($"Deleted {deletedCount} old location records");
    }
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting location repository example...");
        var example = new LocationRepositoryExample();
        await example.QueryLocationDataAsync();
        Console.WriteLine("Location repository example completed!");
    }
}
```

## KalmanLocationSmoother

The `KalmanLocationSmoother` class implements a Kalman filter to smooth noisy GPS location data and reject physically impossible jumps. It maintains per-device state to track location variance and applies filtering to each new location fix. The filter grows uncertainty over time based on process noise and uses measurement accuracy to update its state estimate.

Example usage for smoothing GPS tracker data:

```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Utilities;

public class KalmanLocationSmootherExample
{
    private readonly KalmanLocationSmoother _smoother = new KalmanLocationSmoother
    {
        ProcessNoiseMetersPerSecond = 2.5,    // Expected device speed variation
        MaxPlausibleSpeedKmh = 250.0,       // Maximum acceptable speed
        DefaultAccuracyMeters = 10.0          // Default accuracy for fixes without accuracy data
    };

    public void ProcessGpsFixes()
    {
        // Create a location fix from a GPS device
        var rawFix = new LocationData
        {
            DeviceId = "truck-gps-001",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 65.5,
            Accuracy = 8.2,  // meters
            Altitude = 125.0,
            SatelliteCount = 9,
            Protocol = "GT06"
        };

        // Apply Kalman smoothing
        var smoothedLocation = _smoother.Smooth(rawFix);

        if (smoothedLocation != null)
        {
            Console.WriteLine($"Raw: ({rawFix.Latitude:F6}, {rawFix.Longitude:F6}) " +
                            $"Smoothed: ({smoothedLocation.Latitude:F6}, {smoothedLocation.Longitude:F6})");
            
            // Access raw values from ExtendedData
            var rawLat = (double)smoothedLocation.ExtendedData["kalman.rawLat"];
            var rawLon = (double)smoothedLocation.ExtendedData["kalman.rawLon"];
            Console.WriteLine($"Raw coordinates stored in ExtendedData: ({rawLat:F6}, {rawLon:F6})");
        }
        else
        {
            Console.WriteLine("Location fix rejected as outlier");
        }

        // Reset filter state for a device (e.g., after long offline period)
        _smoother.Reset("truck-gps-001");

        // Reset all filter states
        _smoother.ResetAll();

        // Calculate distance between two coordinates
        var distance = KalmanLocationSmoother.DistanceMeters(
            40.7128, -74.0060,
            40.7306, -73.9352
        );
        Console.WriteLine($"Distance between points: {distance:F2} meters");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Kalman location smoother example...");
        var example = new KalmanLocationSmootherExample();
        example.ProcessGpsFixes();
        Console.WriteLine("Kalman location smoother example completed!");
    }
}
```

## StringExtensions

The `StringExtensions` class provides various extension methods for string manipulation and validation commonly used in GPS tracker protocol processing.

Example usage:
```csharp
using GpsTrackerProtocol.Utilities;

public class StringExtensionsExample
{
    public void ProcessDeviceData()
    {
        // Parse device ID from a string
        string deviceId = "TRK-001-ABC";
        bool isValid = deviceId.IsValidDeviceId();
        Console.WriteLine($"Device ID '{deviceId}' is valid: {isValid}");

        // Parse IMEI
        string imei = "123456789012345";
        bool validImei = imei.IsValidImei();
        Console.WriteLine($"IMEI '{imei}' is valid: {validImei}");

        // Convert string to double with default fallback
        string speedValue = "65.5";
        double speed = speedValue.ToDoubleOrDefault(0.0);
        Console.WriteLine($"Parsed speed: {speed} km/h");

        // Convert string to int with default fallback
        string countValue = "5";
        int count = countValue.ToIntOrDefault(0);
        Console.WriteLine($"Parsed count: {count}");

        // Split NMEA sentence
        string nmeaSentence = "$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47";
        string[] nmeaParts = nmeaSentence.SplitNmea();
        Console.WriteLine($"NMEA sentence split into {nmeaParts.Length} parts");

        // Get and validate NMEA checksum
        string nmeaWithChecksum = "$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47";
        string checksum = nmeaWithChecksum.GetNmeaChecksum();
        bool validChecksum = nmeaWithChecksum.IsValidNmeaChecksum();
        Console.WriteLine($"NMEA checksum: {checksum}, Valid: {validChecksum}");

        // Remove checksum from NMEA sentence
        string sentenceWithoutChecksum = nmeaWithChecksum.RemoveNmeaChecksum();
        Console.WriteLine($"Sentence without checksum: {sentenceWithoutChecksum}");

        // Convert hex string to byte array
        string hexString = "A1B2C3D4";
        byte[] hexBytes = hexString.HexToByteArray();
        Console.WriteLine($"Hex string converted to {hexBytes.Length} bytes");

        // Sanitize device ID
        string rawDeviceId = "  TRK-001-ABC  ";
        string sanitized = rawDeviceId.SanitizeDeviceId();
        Console.WriteLine($"Sanitized device ID: '{sanitized}'");

        // Truncate string
        string longString = "This is a very long device identifier that needs to be shortened";
        string truncated = longString.Truncate(20);
        Console.WriteLine($"Truncated string: '{truncated}'");

        // Validate hex color
        string color = "#FF5733";
        bool isValidColor = color.IsValidHexColor();
        Console.WriteLine($"Color '{color}' is valid hex color: {isValidColor}");
    }
}
```

## DateTimeExtensions

The `DateTimeExtensions` class provides a comprehensive set of extension methods for DateTime manipulation, timestamp conversion, and formatting. It includes utilities for Unix timestamp conversion, date rounding, time comparisons, human-readable formatting, and date boundary calculations.

Example usage for DateTime operations:

```csharp
using GpsTrackerProtocol.Utilities;

public class DateTimeExtensionsExample
{
    public void ProcessTimestamps()
    {
        // Convert DateTime to Unix timestamp
        var now = DateTime.UtcNow;
        long unixTimestamp = now.ToUnixTimestamp();
        Console.WriteLine($"Current Unix timestamp: {unixTimestamp}");
        
        // Convert Unix timestamp back to DateTime
        var convertedBack = DateTimeExtensions.FromUnixTimestamp(unixTimestamp);
        Console.WriteLine($"Converted back: {convertedBack:u}");
        
        // Round down to nearest 5 minutes
        var roundedDown = now.RoundDown(TimeSpan.FromMinutes(5));
        Console.WriteLine($"Rounded down to 5-minute interval: {roundedDown:u}");
        
        // Round up to nearest 15 minutes
        var roundedUp = now.RoundUp(TimeSpan.FromMinutes(15));
        Console.WriteLine($"Rounded up to 15-minute interval: {roundedUp:u}");
        
        // Check if within last 30 seconds
        var recentTime = DateTime.UtcNow.AddSeconds(-15);
        bool isRecent = recentTime.IsWithinSeconds(30);
        Console.WriteLine($"Is within 30 seconds: {isRecent}");
        
        // Get human-readable relative time
        var pastTime = DateTime.UtcNow.AddMinutes(-45);
        string relativeTime = pastTime.ToRelativeTime();
        Console.WriteLine($"Relative time: {relativeTime}");
        
        // Get start/end of day
        var startOfDay = now.GetStartOfDay();
        var endOfDay = now.GetEndOfDay();
        Console.WriteLine($"Start of day: {startOfDay:u}");
        Console.WriteLine($"End of day: {endOfDay:u}");
        
        // Get start/end of month
        var startOfMonth = now.GetStartOfMonth();
        var endOfMonth = now.GetEndOfMonth();
        Console.WriteLine($"Start of month: {startOfMonth:yyyy-MM-dd}");
        Console.WriteLine($"End of month: {endOfMonth:yyyy-MM-dd}");
        
        // Check if same day
        var sameDay = now.IsSameDay(DateTime.UtcNow);
        Console.WriteLine($"Is same day: {sameDay}");
        
        // Format as ISO 8601
        string iso8601 = now.ToIso8601String();
        Console.WriteLine($"ISO 8601 format: {iso8601}");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting DateTimeExtensions example...");
        var example = new DateTimeExtensionsExample();
        example.ProcessTimestamps();
        Console.WriteLine("DateTimeExtensions example completed!");
    }
}
```

## GpsUtilities

The `GpsUtilities` class provides utility methods for GPS coordinate calculations and conversions. It includes functionality for calculating distances between coordinates using the Haversine formula, determining bearings (azimuths) between points, validating coordinate ranges, converting between coordinate formats, and working with speed units.

Example usage for GPS coordinate calculations:

```csharp
using GpsTrackerProtocol.Utilities;

public class GpsUtilitiesExample
{
    public void ProcessGpsCoordinates()
    {
        // Coordinates for New York City
        double nycLat = 40.7128;
        double nycLon = -74.0060;

        // Coordinates for Los Angeles
        double laLat = 34.0522;
        double laLon = -118.2437;

        // Calculate distance between two cities in kilometers
        double distanceKm = GpsUtilities.CalculateDistanceKm(nycLat, nycLon, laLat, laLon);
        Console.WriteLine($"Distance between NYC and LA: {distanceKm:F2} km");

        // Calculate bearing from NYC to LA
        double bearing = GpsUtilities.CalculateBearing(nycLat, nycLon, laLat, laLon);
        Console.WriteLine($"Bearing from NYC to LA: {bearing:F2}°");

        // Validate coordinates
        bool isValidNyc = GpsUtilities.IsValidCoordinate(nycLat, nycLon);
        bool isValidInvalid = GpsUtilities.IsValidCoordinate(200, 300);
        Console.WriteLine($"NYC coordinates valid: {isValidNyc}");
        Console.WriteLine($"Invalid coordinates valid: {isValidInvalid}");

        // Check if coordinate is within bounds
        bool isWithinUs = GpsUtilities.IsWithinBounds(nycLat, nycLon, -90, 90, -180, -60);
        Console.WriteLine($"NYC within US bounds: {isWithinUs}");

        // Convert DMS to decimal degrees
        double decimalDegrees = GpsUtilities.DmsToDecimal(4030.5000, "N");
        Console.WriteLine($"DMS 4030.5000 N = {decimalDegrees:F6} decimal degrees");

        // Convert decimal degrees to DMS
        var (degrees, minutes, seconds) = GpsUtilities.DecimalToDms(40.5083);
        Console.WriteLine($"Decimal 40.5083 = {degrees}° {minutes}' {seconds:F3}\"");

        // Speed conversions
        double speedKnots = 25.0;
        double speedKmh = GpsUtilities.KnotsToKmh(speedKnots);
        double speedMs = GpsUtilities.KmhToMs(speedKmh);
        Console.WriteLine($"{speedKnots} knots = {speedKmh:F2} km/h = {speedMs:F2} m/s");

        // Calculate zoom level for bounding box
        int zoomLevel = GpsUtilities.CalculateZoomLevel(40.0, 41.0, -75.0, -74.0);
        Console.WriteLine($"Zoom level for bounding box: {zoomLevel}");

        // Get center of bounding box
        var (centerLat, centerLon) = GpsUtilities.GetBoundingBoxCenter(40.0, 41.0, -75.0, -74.0);
        Console.WriteLine($"Bounding box center: ({centerLat:F4}, {centerLon:F4})");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting GpsUtilities example...");
        var example = new GpsUtilitiesExample();
        example.ProcessGpsCoordinates();
        Console.WriteLine("GpsUtilities example completed!");
    }
}
```

## IPerformanceMonitor

The `IPerformanceMonitor` interface provides functionality for measuring and tracking the performance of operations within the GPS tracker protocol system. It allows measuring operation execution time, recording metrics, and generating performance reports to identify bottlenecks and optimize system performance.

Example usage for performance monitoring:

```csharp
using GpsTrackerProtocol.Utilities;
using System;
using System.Diagnostics;

public class PerformanceMonitorExample : IDisposable
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly OperationTimer _operationTimer;

    public PerformanceMonitorExample()
    {
        // Create a performance monitor for a specific operation
        _performanceMonitor = new PerformanceMonitor("Database Query");
        
        // Start measuring operation time
        _operationTimer = _performanceMonitor.MeasureOperation();
    }

    public async Task ProcessDeviceDataAsync()
    {
        // Simulate some work
        await Task.Delay(100);
        
        // Record an operation
        _performanceMonitor.RecordOperation("Data Processing", TimeSpan.FromMilliseconds(150));
        
        // Get current metrics
        var metrics = _performanceMonitor.GetMetrics();
        Console.WriteLine($"Operation: {metrics.OperationName}");
        Console.WriteLine($"Count: {metrics.Count}");
        Console.WriteLine($"Total Duration: {metrics.TotalDuration.TotalMilliseconds} ms");
        Console.WriteLine($"Average Duration: {metrics.AverageDuration.TotalMilliseconds} ms");
        Console.WriteLine($"Min Duration: {metrics.MinDuration.TotalMilliseconds} ms");
        Console.WriteLine($"Max Duration: {metrics.MaxDuration.TotalMilliseconds} ms");
        Console.WriteLine($"Median Duration: {metrics.MedianDuration.TotalMilliseconds} ms");
    }

    public void PrintReport()
    {
        // Print a formatted performance report
        _performanceMonitor.PrintReport();
    }

    public void Dispose()
    {
        // Stop the operation timer and dispose the monitor
        _operationTimer?.Dispose();
        (_performanceMonitor as IDisposable)?.Dispose();
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting performance monitor example...");
        
        using var example = new PerformanceMonitorExample();
        await example.ProcessDeviceDataAsync();
        
        example.PrintReport();
        Console.WriteLine("Performance monitor example completed!");
    }
}
```

## ByteExtensions

The `ByteExtensions` class provides extension methods for byte array operations commonly used in GPS tracker protocol parsing. It includes utilities for converting byte arrays to hexadecimal strings, parsing big-endian integers, calculating checksums, converting ASCII strings, and searching for byte sequences.

Example usage for byte array operations:

```csharp
using GpsTrackerProtocol.Utilities;

public class ByteExtensionsExample
{
    public void ProcessProtocolData()
    {
        // Sample GPS protocol frame data
        byte[] protocolData = new byte[] { 0x78, 0x78, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
        
        // Convert to hexadecimal string
        string hexString = protocolData.ToHexString();
        Console.WriteLine($"Hex representation: {hexString}");
        
        // Convert to hexadecimal string with spaces
        string hexWithSpaces = protocolData.ToHexString(addSpaces: true);
        Console.WriteLine($"Hex with spaces: {hexWithSpaces}");
        
        // Parse 16-bit unsigned integer from big-endian bytes
        ushort value16 = protocolData.ToUInt16BigEndian(offset: 2);
        Console.WriteLine($"16-bit value at offset 2: {value16}");
        
        // Parse 32-bit unsigned integer from big-endian bytes
        uint value32 = protocolData.ToUInt32BigEndian(offset: 2);
        Console.WriteLine($"32-bit value at offset 2: {value32}");
        
        // Calculate XOR checksum for validation
        byte checksum = protocolData.CalculateXorChecksum(startIndex: 2, length: 4);
        Console.WriteLine($"XOR checksum for bytes 2-5: 0x{checksum:X2}");
        
        // Convert ASCII bytes to string
        byte[] asciiData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        string asciiString = asciiData.ToAsciiString(startIndex: 0, length: 5);
        Console.WriteLine($"ASCII string: {asciiString}");
        
        // Check if data starts with marker bytes
        bool startsWithMarker = protocolData.StartsWithMarker(0x78, 0x78);
        Console.WriteLine($"Starts with marker (0x78, 0x78): {startsWithMarker}");
        
        // Find index of byte sequence
        byte[] searchSequence = new byte[] { 0x03, 0x04 };
        int sequenceIndex = protocolData.IndexOfSequence(searchSequence);
        Console.WriteLine($"Sequence found at index: {sequenceIndex}");
        
        // Copy range of bytes
        byte[] copiedRange = protocolData.CopyRange(startIndex: 2, length: 4);
        Console.WriteLine($"Copied range length: {copiedRange.Length}");
    }
}
```

## CollectionExtensions

The `CollectionExtensions` class provides a set of extension methods for working with collections and sequences in a functional style. It includes utilities for chunking sequences, calculating medians, removing duplicates while preserving order, finding min/max values, calculating percentages, safe indexing, and creating sliding windows of elements.

Example usage for collection operations:

```csharp
using GpsTrackerProtocol.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

public class CollectionExtensionsExample
{
    public void ProcessGpsData()
    {
        // Sample GPS location readings
        var gpsReadings = new List<(double Latitude, double Longitude, double Speed)>
        {
            (40.7128, -74.0060, 65.5),
            (40.7306, -73.9352, 55.0),
            (40.7589, -73.9851, 72.3),
            (40.7128, -74.0060, 68.1),
            (40.7306, -73.9352, 45.7),
            (40.7749, -73.9712, 80.2)
        };

        // Chunk GPS readings into batches of 2 for batch processing
        var readingBatches = gpsReadings.Chunk(2);
        foreach (var batch in readingBatches)
        {
            Console.WriteLine($"Processing batch with {batch.Count()} readings...");
            foreach (var reading in batch)
            {
                Console.WriteLine($"  Location: ({reading.Latitude:F4}, {reading.Longitude:F4}), Speed: {reading.Speed:F1} km/h");
            }
        }

        // Calculate median speed from all readings
        var medianSpeed = gpsReadings.Select(r => r.Speed).Median(x => x);
        Console.WriteLine($"Median speed across all readings: {medianSpeed:F1} km/h");

        // Remove duplicate locations while preserving order
        var uniqueLocations = gpsReadings
            .Select(r => r.Latitude)
            .DistinctByOrder(lat => lat);
        Console.WriteLine($"Unique latitudes: {string.Join(", ", uniqueLocations)}");

        // Find minimum and maximum speeds
        var (minSpeed, maxSpeed) = gpsReadings.Select(r => r.Speed).MinMax();
        Console.WriteLine($"Speed range: {minSpeed:F1} km/h to {maxSpeed:F1} km/h");

        // Calculate percentage of high-speed readings (> 70 km/h)
        var highSpeedPercentage = gpsReadings.PercentageWhere(r => r.Speed > 70);
        Console.WriteLine($"High speed readings (>70 km/h): {highSpeedPercentage:F1}%");

        // Safely get a reading at a specific index
        var readingsList = gpsReadings.ToList();
        var safeReading = readingsList.SafeGet(10, (0.0, 0.0, 0.0));
        Console.WriteLine($"Safe get at index 10: ({safeReading.Latitude}, {safeReading.Longitude})");

        // Create sliding windows of 3 consecutive readings
        var windows = gpsReadings.SlidingWindow(3);
        foreach (var window in windows)
        {
            var windowList = window.ToList();
            var avgSpeed = windowList.Average(r => r.Speed);
            Console.WriteLine($"Window average speed: {avgSpeed:F1} km/h");
        }
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting collection extensions example...");
        var example = new CollectionExtensionsExample();
        example.ProcessGpsData();
        Console.WriteLine("Collection extensions example completed!");
    }
}
```

## DictionaryExtensions

The `DictionaryExtensions` class provides extension methods for safe and convenient access to dictionary values. It includes utilities for getting values with default fallbacks, parsing values to specific types, merging dictionaries, flattening nested dictionaries, and converting dictionaries to query string format.

Example usage for dictionary operations:

```csharp
using GpsTrackerProtocol.Utilities;
using System;
using System.Collections.Generic;

public class DictionaryExtensionsExample
{
    public void ProcessDeviceConfiguration()
    {
        // Create a dictionary with device configuration
        var deviceConfig = new Dictionary<string, object>
        {
            { "deviceId", "TRK-001-ABC" },
            { "protocol", "GT06" },
            { "enabled", true },
            { "maxSpeedKmh", 120.5 },
            { "timeoutSeconds", 30 },
            { "coordinates", new Dictionary<string, object>
                {
                    { "latitude", 40.7128 },
                    { "longitude", -74.0060 },
                    { "accuracy", 8.2 }
                }
            }
        };

        // Safely get values with default fallbacks
        string deviceId = deviceConfig.GetStringOrEmpty("deviceId");
        Console.WriteLine($"Device ID: {deviceId}");

        bool isEnabled = deviceConfig.GetBoolOrDefault("enabled");
        Console.WriteLine($"Device enabled: {isEnabled}");

        double maxSpeed = deviceConfig.GetDoubleOrDefault("maxSpeedKmh", 100.0);
        Console.WriteLine($"Max speed: {maxSpeed} km/h");

        int timeout = deviceConfig.GetIntOrDefault("timeoutSeconds");
        Console.WriteLine($"Timeout: {timeout} seconds");

        // Get value with default (returns null if not found)
        int missingValue = deviceConfig.GetIntOrDefault("missingKey", 42);
        Console.WriteLine($"Missing key default: {missingValue}");

        // Safely get nested values
        if (deviceConfig.TryGetValue("coordinates", out var coords) && 
            coords is Dictionary<string, object> coordsDict)
        {
            double lat = coordsDict.GetDoubleOrDefault("latitude");
            double lon = coordsDict.GetDoubleOrDefault("longitude");
            Console.WriteLine($"Coordinates: ({lat:F4}, {lon:F4})");
        }

        // Merge another dictionary
        var additionalConfig = new Dictionary<string, object>
        {
            { "timeoutSeconds", 60 },
            { "retryCount", 3 },
            { "debugMode", false }
        };

        deviceConfig.Merge(additionalConfig);
        Console.WriteLine($"Merged config - timeout now: {deviceConfig.GetIntOrDefault("timeoutSeconds")}");

        // Flatten nested dictionary
        var flattened = deviceConfig.Flatten();
        Console.WriteLine($"Flattened dictionary has {flattened.Count} entries");
        foreach (var kvp in flattened)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        // Convert to query string
        string queryString = deviceConfig.ToQueryString();
        Console.WriteLine($"Query string: {queryString}");
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting DictionaryExtensions example...");
        var example = new DictionaryExtensionsExample();
        example.ProcessDeviceConfiguration();
        Console.WriteLine("DictionaryExtensions example completed!");
    }
}
```

## ICachingService

The `ICachingService` interface provides an in-memory caching layer for GPS tracker protocol data, reducing database queries and improving response times. It supports storing, retrieving, and managing cached values with optional time-to-live expiration. The service is thread-safe and designed for high-performance scenarios where frequent access to device information, location data, and other entities is required.

Example usage for caching GPS tracker data:

```csharp
using GpsTrackerProtocol.Caching;
using GpsTrackerProtocol.Domain.Models;

public class CachingServiceExample
{
    private readonly ICachingService _cachingService;

    public CachingServiceExample(ICachingService cachingService)
    {
        _cachingService = cachingService;
    }

    public void ManageCache()
    {
        // Cache a device object with 5-minute TTL
        var device = new Device
        {
            Id = "device-001",
            Imei = "123456789012345",
            Name = "Truck GPS Unit #1",
            Status = DeviceStatus.Active
        };

        _cachingService.Set("device:device-001", device, TimeSpan.FromMinutes(5));

        // Try to retrieve cached device
        if (_cachingService.TryGet<Device>("device:device-001", out var cachedDevice))
        {
            Console.WriteLine($"Retrieved device from cache: {cachedDevice.Name}");
        }

        // Cache a list of all devices
        var deviceList = new List<Device> { device };
        _cachingService.Set("devices:all", deviceList);

        // Get all cache keys
        var allKeys = _cachingService.GetAllKeys();
        Console.WriteLine($"Total cache entries: {allKeys.Count()}");

        // Remove a specific cache entry
        _cachingService.Remove("device:device-001");

        // Clear the entire cache
        _cachingService.Clear();
    }

    public static void Main(string[] args)
    {
        // Example with direct service instantiation
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var cachingService = new CachingService(loggerFactory.CreateLogger<CachingService>());

        Console.WriteLine("Starting caching service example...");
        var example = new CachingServiceExample(cachingService);
        example.ManageCache();

        Console.WriteLine("Caching service example completed!");
    }
}
```

## ICommandService

The `ICommandService` interface provides functionality for managing device commands in the GPS tracker system. It allows creating commands, retrieving command history, executing commands, handling command failures, and cleaning up old command records. The service supports both modern repository pattern and legacy repository implementations.

Example usage in code:

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;

public class CommandServiceExample
{
    private readonly ICommandService _commandService;

    public CommandServiceExample(ICommandService commandService)
    {
        _commandService = commandService;
    }

    public async Task ManageDeviceCommandsAsync(string deviceId)
    {
        // Create a new command for a device
        var command = new Command
        {
            DeviceId = deviceId,
            CommandType = "REBOOT",
            Payload = "{}",
            Priority = CommandPriority.High
        };

        var createdCommand = await _commandService.CreateCommandAsync(command);
        Console.WriteLine($"Created command {createdCommand.Id} of type {createdCommand.CommandType}");

        // Get pending commands
        var pendingCommands = await _commandService.GetPendingCommandsAsync();
        Console.WriteLine($"Pending commands count: {pendingCommands.Count()}");

        // Get command history for device
        var commandHistory = await _commandService.GetCommandHistoryAsync(deviceId);
        Console.WriteLine($"Command history count: {commandHistory.Count()}");

        // Execute a command
        var executed = await _commandService.ExecuteCommandAsync(createdCommand.Id);
        Console.WriteLine($"Command executed: {executed}");

        // Mark command as failed (for retry logic)
        var markedAsFailed = await _commandService.MarkCommandAsFailedAsync(createdCommand.Id);
        Console.WriteLine($"Command marked as failed: {markedAsFailed}");

        // Retry failed commands
        var retryCount = await _commandService.RetryFailedCommandsAsync();
        Console.WriteLine($"Retry count: {retryCount}");

        // Cleanup old commands
        var cleanupCount = await _commandService.CleanupOldCommandsAsync(DateTime.UtcNow.AddDays(-30));
        Console.WriteLine($"Cleanup count: {cleanupCount}");
    }

    public static async Task Main(string[] args)
    {
        // Example with direct service instantiation
        var commandService = new CommandService(new UnitOfWork());

        Console.WriteLine("Starting command management example...");
        var example = new CommandServiceExample(commandService);
        await example.ManageDeviceCommandsAsync("device-001");

        Console.WriteLine("Command management example completed!");
    }
}
```

## IAnalyticsService

The `IAnalyticsService` interface provides functionality for computing analytics and statistics for GPS tracker devices and fleets. It aggregates location data, journey information, and device metrics to generate comprehensive reports including device-specific analytics, fleet-wide statistics, and route analysis.

Example usage in code:

```csharp
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;

public class JourneyServiceExample
{
    private readonly IJourneyService _journeyService;

    public JourneyServiceExample(IJourneyService journeyService)
    {
        _journeyService = journeyService;
    }

    public async Task ManageDeviceJourneysAsync(string deviceId)
    {
        // Start a new journey for a device
        var journey = await _journeyService.StartJourneyAsync(deviceId);
        Console.WriteLine($"Started journey {journey.Id} at {journey.StartTime}");

        // Add waypoints to the ongoing journey
        var location1 = new LocationData
        {
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 65.5
        };
        
        var location2 = new LocationData
        {
            Latitude = 40.7306,
            Longitude = -73.9352,
            Timestamp = DateTime.UtcNow.AddMinutes(15),
            Speed = 55.0
        };

        bool waypointAdded = await _journeyService.AddWaypointAsync(journey.Id, location1);
        Console.WriteLine($"Waypoint added: {waypointAdded}");

        waypointAdded = await _journeyService.AddWaypointAsync(journey.Id, location2);
        Console.WriteLine($"Waypoint added: {waypointAdded}");

        // Check ongoing journey
        var ongoingJourney = await _journeyService.GetOngoingJourneyAsync(deviceId);
        Console.WriteLine($"Ongoing journey: {ongoingJourney?.Id}");

        // Complete the journey
        var completedJourney = await _journeyService.CompleteJourneyAsync(journey.Id);
        Console.WriteLine($"Completed journey {completedJourney.Id} with {completedJourney.Waypoints.Count} waypoints");

        // Get journey history
        var journeyHistory = await _journeyService.GetJourneyHistoryAsync(deviceId);
        Console.WriteLine($"Total journeys: {journeyHistory.Count()}");

        // Calculate total distance traveled
        var totalDistance = await _journeyService.GetTotalDistanceAsync(deviceId);
        Console.WriteLine($"Total distance: {totalDistance:F2} km");

        // Get a specific journey
        var specificJourney = await _journeyService.GetJourneyAsync(journey.Id);
        Console.WriteLine($"Retrieved journey: {specificJourney?.DeviceId}");
    }

    public static async Task Main(string[] args)
    {
        // Example with direct service instantiation
        var journeyService = new JourneyService(new UnitOfWork());
        
        Console.WriteLine("Starting journey management example...");
        var example = new JourneyServiceExample(journeyService);
        await example.ManageDeviceJourneysAsync("device-001");
        
        Console.WriteLine("Journey management example completed!");
    }
}
```

## InMemoryDeviceRepository

The `InMemoryDeviceRepository` class provides an in-memory implementation of the `IDeviceRepository` interface, designed for testing, prototyping, and demonstration purposes. It extends the base `InMemoryRepository<Device>` class with device-specific query methods that allow filtering devices by IMEI, status, protocol type, and active state.

This repository uses thread-safe operations with `ReaderWriterLockSlim` for concurrent access and supports all standard CRUD operations along with specialized device management methods.

Example usage for device management:

```csharp
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain.Models;

public class InMemoryDeviceRepositoryExample
{
public async Task ManageDevicesAsync()
{
// Create repository instance
var deviceRepository = new InMemoryDeviceRepository();

// Register devices with different protocols and statuses
var gpsDevice1 = new Device
{
Id = "device-001",
Imei = "123456789012345",
Name = "Truck GPS Unit #1",
Model = "GT06",
Protocol = ProtocolType.GT06,
PhoneNumber = "+1234567890",
Status = DeviceStatus.Active,
LastHeartbeat = DateTime.UtcNow.AddMinutes(-5),
CreatedAt = DateTime.UtcNow
};

var gpsDevice2 = new Device
{
Id = "device-002",
Imei = "234567890123456",
Name = "Van GPS Unit #2",
Model = "H02",
Protocol = ProtocolType.H02,
PhoneNumber = "+1987654321",
Status = DeviceStatus.Maintenance,
LastHeartbeat = DateTime.UtcNow.AddHours(-2),
CreatedAt = DateTime.UtcNow
};

var tk103Device = new Device
{
Id = "device-003",
Imei = "345678901234567",
Name = "Car GPS Tracker",
Model = "TK103",
Protocol = ProtocolType.TK103,
PhoneNumber = "+15551234567",
Status = DeviceStatus.Active,
LastHeartbeat = DateTime.UtcNow.AddMinutes(-15),
CreatedAt = DateTime.UtcNow
};

await deviceRepository.CreateAsync(gpsDevice1);
await deviceRepository.CreateAsync(gpsDevice2);
await deviceRepository.CreateAsync(tk103Device);

Console.WriteLine("Created 3 GPS devices");

// Retrieve devices by IMEI
var deviceByImei = await deviceRepository.GetByImeiAsync("123456789012345");
Console.WriteLine($"Device found by IMEI: {deviceByImei?.Name}");

// Get all devices by status
var activeDevices = await deviceRepository.GetByStatusAsync(DeviceStatus.Active);
Console.WriteLine($"Active devices: {activeDevices.Count()}");

var maintenanceDevices = await deviceRepository.GetByStatusAsync(DeviceStatus.Maintenance);
Console.WriteLine($"Maintenance devices: {maintenanceDevices.Count()}");

// Get devices by protocol type
var gt06Devices = await deviceRepository.GetByProtocolAsync(ProtocolType.GT06);
Console.WriteLine($"GT06 devices: {gt06Devices.Count()}");

var h02Devices = await deviceRepository.GetByProtocolAsync(ProtocolType.H02);
Console.WriteLine($"H02 devices: {h02Devices.Count()}");

// Get active devices
var activeOnlyDevices = await deviceRepository.GetActiveDevicesAsync();
Console.WriteLine($"Active devices (IsActive): {activeOnlyDevices.Count()}");

// Get total device count
var totalCount = await deviceRepository.GetTotalCountAsync();
Console.WriteLine($"Total devices in repository: {totalCount}");

// Get offline devices (not seen for more than 1 hour)
var offlineDevices = await deviceRepository.GetOfflineDevicesAsync(TimeSpan.FromHours(1));
Console.WriteLine($"Offline devices: {offlineDevices.Count()}");

// Update device status
var deviceToUpdate = activeDevices.First();
deviceToUpdate.Status = DeviceStatus.Maintenance;
var updated = await deviceRepository.UpdateAsync(deviceToUpdate);
Console.WriteLine($"Updated device {updated.Name} status to: {updated.Status}");

// Delete a device
var deleteSuccess = await deviceRepository.DeleteAsync("device-003");
Console.WriteLine($"Device deletion successful: {deleteSuccess}");

// Get remaining devices
var remainingDevices = await deviceRepository.GetAllAsync();
Console.WriteLine($"Remaining devices: {remainingDevices.Count()}");
}

public static async Task Main(string[] args)
{
Console.WriteLine("Starting InMemoryDeviceRepository example...");
var example = new InMemoryDeviceRepositoryExample();
await example.ManageDevicesAsync();
Console.WriteLine("InMemoryDeviceRepository example completed!");
}
}
```
