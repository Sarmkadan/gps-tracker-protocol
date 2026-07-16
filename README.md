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
