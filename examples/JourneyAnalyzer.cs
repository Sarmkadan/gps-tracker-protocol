#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Analyzes device journeys and generates comprehensive trip reports including
/// distance, duration, speed statistics, and route visualization data.
/// </summary>
public class JourneyAnalyzer
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<JourneyAnalyzer> _logger;
    private readonly IDeviceService _deviceService;
    private readonly ILocationDataService _locationService;
    private readonly IJourneyService _journeyService;

    public JourneyAnalyzer()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<JourneyAnalyzer>>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();
        _journeyService = _provider.GetRequiredService<IJourneyService>();
    }

    /// <summary>Analyzes a single device's journey statistics</summary>
    public async Task AnalyzeDeviceAsync(string deviceId)
    {
        var device = await _deviceService.GetDeviceAsync(deviceId);
        if (device is null)
        {
            _logger.LogError("Device not found: {0}", deviceId);
            return;
        }

        _logger.LogInformation("Analyzing device: {0} ({1})", device.DeviceName, device.Imei);

        var journeys = await _journeyService.GetJourneyHistoryAsync(deviceId);
        var journeyList = journeys.ToList();

        if (journeyList.Count == 0)
        {
            _logger.LogWarning("No journeys found for device");
            return;
        }

        double totalDistance = 0;
        TimeSpan totalDuration = TimeSpan.Zero;
        double maxSpeed = 0;
        double minSpeed = double.MaxValue;
        int totalWaypoints = 0;

        foreach (var journey in journeyList)
        {
            totalDistance += journey.GetTotalDistance();
            totalDuration += journey.GetDuration();
            totalWaypoints += journey.Waypoints.Count;

            var maxJourneySpeed = journey.Waypoints.Max(w => w.Speed);
            var minJourneySpeed = journey.Waypoints.Min(w => w.Speed);

            if (maxJourneySpeed > maxSpeed) maxSpeed = maxJourneySpeed;
            if (minJourneySpeed < minSpeed) minSpeed = minJourneySpeed;
        }

        _logger.LogInformation("=== Journey Summary ===");
        _logger.LogInformation("Total journeys: {0}", journeyList.Count);
        _logger.LogInformation("Total distance: {0:F2} km", totalDistance);
        _logger.LogInformation("Total duration: {0:hh\\:mm\\:ss}", totalDuration);
        _logger.LogInformation("Average journey distance: {0:F2} km", totalDistance / journeyList.Count);
        _logger.LogInformation("Average speed: {0:F1} km/h", totalDistance / totalDuration.TotalHours);
        _logger.LogInformation("Max speed: {0:F1} km/h", maxSpeed);
        _logger.LogInformation("Min speed: {0:F1} km/h", minSpeed);
        _logger.LogInformation("Total waypoints: {0}", totalWaypoints);
    }

    /// <summary>Creates a new journey and simulates tracking</summary>
    public async Task SimulateJourneyAsync(string deviceId, int waypointCount = 10)
    {
        var device = await _deviceService.GetDeviceAsync(deviceId);
        if (device is null)
        {
            _logger.LogError("Device not found: {0}", deviceId);
            return;
        }

        _logger.LogInformation("Starting simulated journey for {0}", device.DeviceName);

        var journey = await _journeyService.StartJourneyAsync(deviceId);
        var baseTime = DateTime.UtcNow;

        double lat = 40.7128;
        double lng = -74.0060;
        double speed = 40.0;

        for (int i = 0; i < waypointCount; i++)
        {
            lat += 0.001 * (i % 2 == 0 ? 1 : -1);
            lng += 0.001 * (i % 2 == 0 ? 1 : -1);

            var waypoint = new LocationData
            {
                DeviceId = deviceId,
                Latitude = lat,
                Longitude = lng,
                Speed = speed + (i * 2),
                Bearing = (double)(i * 36 % 360),
                Altitude = 10.0,
                Timestamp = baseTime.AddSeconds(i * 60),
                SatelliteCount = 12,
                Accuracy = 5.0
            };

            await _journeyService.AddWaypointAsync(journey.Id, waypoint);
            _logger.LogInformation("Waypoint {0}: ({1:F4}, {2:F4}) Speed={3:F1}",
                i + 1, lat, lng, waypoint.Speed);
        }

        var completed = await _journeyService.CompleteJourneyAsync(journey.Id);
        _logger.LogInformation("Journey completed: Distance={0:F2}km, Duration={1:hh\\:mm\\:ss}",
            completed.GetTotalDistance(), completed.GetDuration());
    }

    /// <summary>Generates detailed journey report for all devices</summary>
    public async Task GenerateFleetReportAsync()
    {
        var devices = await _deviceService.GetAllDevicesAsync();
        var deviceList = devices.ToList();

        _logger.LogInformation("=== Fleet Journey Report ===");
        _logger.LogInformation("Total devices: {0}", deviceList.Count);

        double totalFleetDistance = 0;
        TimeSpan totalFleetDuration = TimeSpan.Zero;
        int totalFleetJourneys = 0;

        foreach (var device in deviceList)
        {
            var journeys = await _journeyService.GetJourneyHistoryAsync(device.Id);
            var journeyList = journeys.ToList();

            if (journeyList.Count == 0) continue;

            var deviceDistance = journeyList.Sum(j => j.GetTotalDistance());
            var deviceDuration = journeyList.Aggregate(TimeSpan.Zero,
                (acc, j) => acc + j.GetDuration());

            totalFleetDistance += deviceDistance;
            totalFleetDuration += deviceDuration;
            totalFleetJourneys += journeyList.Count;

            _logger.LogInformation("Device {0}: {1} journeys, {2:F2}km, {3:hh\\:mm\\:ss}",
                device.DeviceName, journeyList.Count, deviceDistance,
                deviceDuration);
        }

        _logger.LogInformation("--- Fleet Totals ---");
        _logger.LogInformation("Total journeys: {0}", totalFleetJourneys);
        _logger.LogInformation("Total distance: {0:F2} km", totalFleetDistance);
        _logger.LogInformation("Total duration: {0:hh\\:mm\\:ss}", totalFleetDuration);
    }

    public static async Task Main(string[] args)
    {
        var analyzer = new JourneyAnalyzer();

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: JourneyAnalyzer <command> [device-id]");
            Console.WriteLine("Commands:");
            Console.WriteLine("  analyze <device-id> - Analyze device journeys");
            Console.WriteLine("  simulate <device-id> [waypoints] - Simulate journey");
            Console.WriteLine("  fleet - Generate fleet report");
            return;
        }

        var command = args[0].ToLower();

        switch (command)
        {
            case "analyze":
                if (args.Length > 1)
                    await analyzer.AnalyzeDeviceAsync(args[1]);
                else
                    Console.WriteLine("Device ID required");
                break;

            case "simulate":
                if (args.Length > 1)
                {
                    int waypoints = args.Length > 2 && int.TryParse(args[2], out var w) ? w : 10;
                    await analyzer.SimulateJourneyAsync(args[1], waypoints);
                }
                else
                    Console.WriteLine("Device ID required");
                break;

            case "fleet":
                await analyzer.GenerateFleetReportAsync();
                break;

            default:
                Console.WriteLine("Unknown command: {0}", command);
                break;
        }
    }
}
