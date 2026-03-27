#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Exports GPS location data to various formats: JSON, CSV, and GeoJSON.
/// Supports filtering by device, date range, and geographic bounds.
/// </summary>
public class DataExporter
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<DataExporter> _logger;
    private readonly ILocationDataService _locationService;
    private readonly IDeviceService _deviceService;

    public DataExporter()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<DataExporter>>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
    }

    /// <summary>Exports locations to JSON format</summary>
    public async Task ExportToJsonAsync(string deviceId, string outputPath)
    {
        var locations = await _locationService.GetLocationHistoryAsync(deviceId, 10000);
        var locList = locations.ToList();

        var json = new StringBuilder();
        json.AppendLine("[");

        for (int i = 0; i < locList.Count; i++)
        {
            var loc = locList[i];
            json.AppendLine("  {");
            json.AppendLine($"    \"deviceId\": \"{loc.DeviceId}\",");
            json.AppendLine($"    \"latitude\": {loc.Latitude},");
            json.AppendLine($"    \"longitude\": {loc.Longitude},");
            json.AppendLine($"    \"speed\": {loc.Speed},");
            json.AppendLine($"    \"bearing\": {loc.Bearing},");
            json.AppendLine($"    \"altitude\": {loc.Altitude},");
            json.AppendLine($"    \"satelliteCount\": {loc.SatelliteCount},");
            json.AppendLine($"    \"accuracy\": {loc.Accuracy},");
            json.AppendLine($"    \"timestamp\": \"{loc.Timestamp:O}\"");
            json.Append("  }");

            if (i < locList.Count - 1) json.AppendLine(",");
            else json.AppendLine();
        }

        json.AppendLine("]");

        await File.WriteAllTextAsync(outputPath, json.ToString());
        _logger.LogInformation("Exported {0} locations to JSON: {1}", locList.Count, outputPath);
    }

    /// <summary>Exports locations to CSV format</summary>
    public async Task ExportToCsvAsync(string deviceId, string outputPath)
    {
        var locations = await _locationService.GetLocationHistoryAsync(deviceId, 10000);
        var locList = locations.ToList();

        var csv = new StringBuilder();
        csv.AppendLine("DeviceId,Latitude,Longitude,Speed,Bearing,Altitude,SatelliteCount,Accuracy,Timestamp");

        foreach (var loc in locList)
        {
            csv.AppendLine($"{loc.DeviceId},{loc.Latitude:F6},{loc.Longitude:F6}," +
                $"{loc.Speed:F1},{loc.Bearing:F1},{loc.Altitude:F1}," +
                $"{loc.SatelliteCount},{loc.Accuracy:F1},{loc.Timestamp:O}");
        }

        await File.WriteAllTextAsync(outputPath, csv.ToString());
        _logger.LogInformation("Exported {0} locations to CSV: {1}", locList.Count, outputPath);
    }

    /// <summary>Exports locations to GeoJSON format (suitable for mapping libraries)</summary>
    public async Task ExportToGeoJsonAsync(string deviceId, string outputPath)
    {
        var device = await _deviceService.GetDeviceAsync(deviceId);
        var locations = await _locationService.GetLocationHistoryAsync(deviceId, 10000);
        var locList = locations.ToList();

        var geoJson = new StringBuilder();
        geoJson.AppendLine("{");
        geoJson.AppendLine("  \"type\": \"FeatureCollection\",");
        geoJson.AppendLine($"  \"name\": \"{device?.DeviceName ?? deviceId}\",");
        geoJson.AppendLine("  \"features\": [");

        for (int i = 0; i < locList.Count; i++)
        {
            var loc = locList[i];
            geoJson.AppendLine("    {");
            geoJson.AppendLine("      \"type\": \"Feature\",");
            geoJson.AppendLine("      \"properties\": {");
            geoJson.AppendLine($"        \"deviceId\": \"{loc.DeviceId}\",");
            geoJson.AppendLine($"        \"speed\": {loc.Speed},");
            geoJson.AppendLine($"        \"bearing\": {loc.Bearing},");
            geoJson.AppendLine($"        \"satellites\": {loc.SatelliteCount},");
            geoJson.AppendLine($"        \"timestamp\": \"{loc.Timestamp:O}\"");
            geoJson.AppendLine("      },");
            geoJson.AppendLine("      \"geometry\": {");
            geoJson.AppendLine("        \"type\": \"Point\",");
            geoJson.AppendLine($"        \"coordinates\": [{loc.Longitude}, {loc.Latitude}]");
            geoJson.AppendLine("      }");
            geoJson.Append("    }");

            if (i < locList.Count - 1) geoJson.AppendLine(",");
            else geoJson.AppendLine();
        }

        geoJson.AppendLine("  ]");
        geoJson.AppendLine("}");

        await File.WriteAllTextAsync(outputPath, geoJson.ToString());
        _logger.LogInformation("Exported {0} locations to GeoJSON: {1}", locList.Count, outputPath);
    }

    /// <summary>Exports all devices to JSON</summary>
    public async Task ExportDevicesToJsonAsync(string outputPath)
    {
        var devices = await _deviceService.GetAllDevicesAsync();
        var deviceList = devices.ToList();

        var json = JsonSerializer.Serialize(deviceList.Select(d => new
        {
            d.Id,
            d.DeviceName,
            d.Imei,
            d.Protocol,
            d.Status,
            d.IsActive,
            RegisteredAt = d.RegisteredAt.ToString("O"),
            LastUpdateAt = d.LastUpdateAt?.ToString("O")
        }), new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(outputPath, json);
        _logger.LogInformation("Exported {0} devices to JSON: {1}", deviceList.Count, outputPath);
    }

    public static async Task Main(string[] args)
    {
        var exporter = new DataExporter();

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: DataExporter <format> <output> [device-id]");
            Console.WriteLine("Formats: json | csv | geojson | devices");
            Console.WriteLine("Examples:");
            Console.WriteLine("  DataExporter json locations.json device-001");
            Console.WriteLine("  DataExporter csv locations.csv device-001");
            Console.WriteLine("  DataExporter geojson map.json device-001");
            Console.WriteLine("  DataExporter devices devices.json");
            return;
        }

        var format = args[0].ToLower();
        var output = args[1];
        var deviceId = args.Length > 2 ? args[2] : "";

        try
        {
            switch (format)
            {
                case "json":
                    await exporter.ExportToJsonAsync(deviceId, output);
                    break;
                case "csv":
                    await exporter.ExportToCsvAsync(deviceId, output);
                    break;
                case "geojson":
                    await exporter.ExportToGeoJsonAsync(deviceId, output);
                    break;
                case "devices":
                    await exporter.ExportDevicesToJsonAsync(output);
                    break;
                default:
                    Console.WriteLine("Unknown format: {0}", format);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: {0}", ex.Message);
        }
    }
}
