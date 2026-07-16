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

Example usage in code:

## IGeofenceService

The `IGeofenceService` interface provides functionality for managing geographic boundary zones (geofences) and detecting when GPS devices enter or exit these zones. It allows adding circular geofences with specified centers and radii, checking if a location is inside a geofence, and finding nearby geofences for a given location.

Example usage in code:

```csharp
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
