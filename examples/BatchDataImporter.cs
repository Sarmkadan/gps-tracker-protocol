#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Batch importer for loading GPS locations from CSV and JSON files.
/// Supports large-scale data import with progress reporting and error handling.
/// </summary>
public class BatchDataImporter
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<BatchDataImporter> _logger;
    private readonly IDeviceService _deviceService;
    private readonly ILocationDataService _locationService;
    private int _successCount;
    private int _errorCount;

    public BatchDataImporter()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<BatchDataImporter>>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();
    }

    /// <summary>Imports locations from CSV file (DeviceId,Latitude,Longitude,Speed,Timestamp)</summary>
    public async Task ImportCsvAsync(string filePath)
    {
        _successCount = 0;
        _errorCount = 0;

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {0}", filePath);
            return;
        }

        _logger.LogInformation("Starting CSV import from {0}", filePath);
        var lines = await File.ReadAllLinesAsync(filePath);

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var parts = line.Split(',');
                if (parts.Length < 5) continue;

                var location = new LocationData
                {
                    DeviceId = parts[0].Trim(),
                    Latitude = double.Parse(parts[1]),
                    Longitude = double.Parse(parts[2]),
                    Speed = double.Parse(parts[3]),
                    Timestamp = DateTime.Parse(parts[4]),
                    SatelliteCount = 12,
                    Accuracy = 5.0
                };

                await _locationService.StoreLocationAsync(location);
                _successCount++;

                if (_successCount % 1000 == 0)
                    _logger.LogInformation("Imported {0} locations...", _successCount);
            }
            catch (Exception ex)
            {
                _errorCount++;
                _logger.LogWarning(ex, "Error importing line: {0}", line);
            }
        }

        _logger.LogInformation("CSV import completed: {0} success, {1} errors",
            _successCount, _errorCount);
    }

    /// <summary>Imports locations from JSON file (array of location objects)</summary>
    public async Task ImportJsonAsync(string filePath)
    {
        _successCount = 0;
        _errorCount = 0;

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {0}", filePath);
            return;
        }

        _logger.LogInformation("Starting JSON import from {0}", filePath);
        var json = await File.ReadAllTextAsync(filePath);

        try
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                var locations = root.EnumerateArray();

                foreach (var locElement in locations)
                {
                    try
                    {
                        var location = new LocationData
                        {
                            DeviceId = locElement.GetProperty("deviceId").GetString() ?? "",
                            Latitude = locElement.GetProperty("latitude").GetDouble(),
                            Longitude = locElement.GetProperty("longitude").GetDouble(),
                            Speed = locElement.GetProperty("speed").GetDouble(),
                            Bearing = locElement.TryGetProperty("bearing", out var b) ? b.GetDouble() : 0,
                            Altitude = locElement.TryGetProperty("altitude", out var a) ? a.GetDouble() : 0,
                            Timestamp = DateTime.Parse(locElement.GetProperty("timestamp").GetString() ?? DateTime.UtcNow.ToString()),
                            SatelliteCount = 12,
                            Accuracy = 5.0
                        };

                        await _locationService.StoreLocationAsync(location);
                        _successCount++;

                        if (_successCount % 1000 == 0)
                            _logger.LogInformation("Imported {0} locations...", _successCount);
                    }
                    catch (Exception ex)
                    {
                        _errorCount++;
                        _logger.LogWarning(ex, "Error importing location object");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JSON file");
            return;
        }

        _logger.LogInformation("JSON import completed: {0} success, {1} errors",
            _successCount, _errorCount);
    }

    /// <summary>Imports devices from CSV file (IMEI,DeviceName,Protocol)</summary>
    public async Task ImportDevicesAsync(string filePath)
    {
        int count = 0;

        if (!File.Exists(filePath))
        {
            _logger.LogError("File not found: {0}", filePath);
            return;
        }

        _logger.LogInformation("Starting device import from {0}", filePath);
        var lines = await File.ReadAllLinesAsync(filePath);

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var protocol = Enum.Parse<ProtocolType>(parts[2].Trim());

                var device = new Device
                {
                    Imei = parts[0].Trim(),
                    DeviceName = parts[1].Trim(),
                    Protocol = protocol,
                    IsActive = true
                };

                await _deviceService.RegisterDeviceAsync(device);
                count++;

                _logger.LogInformation("Device registered: {0} ({1})", device.Imei, device.Protocol);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error importing device: {0}", line);
            }
        }

        _logger.LogInformation("Device import completed: {0} devices imported", count);
    }

    public static async Task Main(string[] args)
    {
        var importer = new BatchDataImporter();

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: BatchDataImporter <command> [file]");
            Console.WriteLine("Commands: csv <file> | json <file> | devices <file>");
            return;
        }

        var command = args[0].ToLower();
        var filePath = args.Length > 1 ? args[1] : "";

        switch (command)
        {
            case "csv":
                await importer.ImportCsvAsync(filePath);
                break;
            case "json":
                await importer.ImportJsonAsync(filePath);
                break;
            case "devices":
                await importer.ImportDevicesAsync(filePath);
                break;
            default:
                Console.WriteLine("Unknown command: {0}", command);
                break;
        }
    }
}
