// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol;
using GpsTrackerProtocol.Configuration;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

var services = new ServiceCollection();
services.AddGpsTrackerServices();
services.AddGpsTrackerLogging();

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();
var deviceService = provider.GetRequiredService<IDeviceService>();
var locationService = provider.GetRequiredService<ILocationDataService>();
var parserService = provider.GetRequiredService<IProtocolParserService>();
var journeyService = provider.GetRequiredService<IJourneyService>();
var commandService = provider.GetRequiredService<ICommandService>();

logger.LogInformation("=== GPS Tracker Protocol Parser Demo ===");

try
{
    // Demo 1: Register a device
    logger.LogInformation("\n[1] Registering a new device...");
    var device = new Device
    {
        Id = "device-001",
        Imei = "358240050477491",
        DeviceName = "Fleet Vehicle #001",
        Protocol = ProtocolType.GT06,
        IsActive = true
    };

    var registeredDevice = await deviceService.RegisterDeviceAsync(device);
    logger.LogInformation("Device registered: {RegisteredDevice}", registeredDevice);

    // Demo 2: Create and parse a GPS frame
    logger.LogInformation("\n[2] Creating and parsing GPS frame...");
    var gpsFrame = new GpsFrame
    {
        FrameId = "frame-001",
        Protocol = ProtocolType.GT06,
        RawData = new byte[] { 0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23, 0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A },
        ReceivedAt = DateTime.UtcNow,
        SourceAddress = "192.168.1.100",
        SourcePort = 5000,
        IsValidChecksum = true
    };

    var protocolType = await parserService.DetectProtocolAsync(gpsFrame.RawData);
    logger.LogInformation("Detected protocol: {ProtocolType}", protocolType);

    var isValid = await parserService.ValidateFrameAsync(gpsFrame);
    logger.LogInformation("Frame validation: {IsValid}", isValid);

    // Demo 3: Store location data
    logger.LogInformation("\n[3] Storing location data...");
    var location = new LocationData
    {
        DeviceId = "device-001",
        Latitude = 40.7128,
        Longitude = -74.0060,
        Speed = 50.0,
        Bearing = 45.0,
        Altitude = 10.0,
        Timestamp = DateTime.UtcNow,
        SatelliteCount = 12,
        Accuracy = 5.0,
        Protocol = ProtocolType.GT06
    };

    var storedLocation = await locationService.StoreLocationAsync(location);
    logger.LogInformation("Location stored: {StoredLocation}", storedLocation);

    var latestLocation = await locationService.GetLatestLocationAsync("device-001");
    logger.LogInformation("Latest location: {LatestLocation}", latestLocation);

    // Demo 4: Start a journey
    logger.LogInformation("\n[4] Starting a journey...");
    var journey = await journeyService.StartJourneyAsync("device-001");
    logger.LogInformation("Journey started: {Id}", journey.Id);

    // Add waypoints to journey
    for (int i = 0; i < 3; i++)
    {
        var waypoint = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 40.7128 + (i * 0.001),
            Longitude = -74.0060 + (i * 0.001),
            Speed = 50.0 + (i * 5),
            Bearing = 45.0 + (i * 10),
            Altitude = 10.0,
            Timestamp = DateTime.UtcNow.AddSeconds(i * 60),
            SatelliteCount = 12,
            Accuracy = 5.0,
            Protocol = ProtocolType.GT06
        };

        await journeyService.AddWaypointAsync(journey.Id, waypoint);
        logger.LogInformation($"Waypoint {i + 1} added");
    }

    var completedJourney = await journeyService.CompleteJourneyAsync(journey.Id);
    logger.LogInformation($"Journey completed: Distance={completedJourney.GetTotalDistance():F2}km, Duration={completedJourney.GetDuration().TotalMinutes:F1}min");

    // Demo 5: Create and manage commands
    logger.LogInformation("\n[5] Creating device commands...");
    var cmd = new Command
    {
        DeviceId = "device-001",
        Type = CommandType.SetGpsInterval,
        Parameters = new Dictionary<string, object> { { "interval", 60 } }
    };

    var createdCommand = await commandService.CreateCommandAsync(cmd);
    logger.LogInformation($"Command created: {createdCommand.ToFormattedCommand()}");

    await commandService.ExecuteCommandAsync(createdCommand.Id);
    logger.LogInformation("Command executed");

    // Demo 6: Query device information
    logger.LogInformation("\n[6] Querying device information...");
    var allDevices = await deviceService.GetAllDevicesAsync();
    logger.LogInformation($"Total devices: {allDevices.Count()}");

    var deviceById = await deviceService.GetDeviceAsync("device-001");
    logger.LogInformation($"Device: {deviceById?.DeviceName} - Status: {deviceById?.Status}");

    var locationHistory = await locationService.GetLocationHistoryAsync("device-001", 10);
    logger.LogInformation($"Location history count: {locationHistory.Count()}");

    var journeyHistory = await journeyService.GetJourneyHistoryAsync("device-001");
    logger.LogInformation($"Journey history count: {journeyHistory.Count()}");

    // Demo 7: Calculate analytics
    logger.LogInformation("\n[7] Calculating analytics...");
    var totalDistance = await journeyService.GetTotalDistanceAsync("device-001");
    logger.LogInformation("Total distance traveled: {TotalDistance}km", totalDistance);

    logger.LogInformation("\n=== Demo completed successfully ===");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during demo execution");
    Environment.Exit(1);
}

await provider.DisposeAsync();
