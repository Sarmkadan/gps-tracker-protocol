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
