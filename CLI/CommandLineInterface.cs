#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.CLI;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Formatting;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Utilities;

/// <summary>
/// Command-line interface for GPS tracker operations.
/// Parses arguments and routes to appropriate command handlers.
/// </summary>
public interface ICommandLineInterface
{
    Task<int> ExecuteAsync(string[] args);
}

public class CommandLineInterface : ICommandLineInterface
{
    private readonly ILogger<CommandLineInterface> _logger;
    private readonly IProtocolParserService _parserService;
    private readonly IDeviceService _deviceService;
    private readonly ILocationDataService _locationService;
    private readonly IJourneyService _journeyService;
    private readonly IJsonFormatter _jsonFormatter;
    private readonly ICsvFormatter _csvFormatter;
    private readonly IGeofenceAlertingService _geofenceAlerting;
    private readonly IRouteReplayService _routeReplay;
    private readonly IDeviceDiagnosticsService _diagnostics;

    public CommandLineInterface(
        ILogger<CommandLineInterface> logger,
        IProtocolParserService parserService,
        IDeviceService deviceService,
        ILocationDataService locationService,
        IJourneyService journeyService,
        IJsonFormatter jsonFormatter,
        ICsvFormatter csvFormatter,
        IGeofenceAlertingService geofenceAlerting,
        IRouteReplayService routeReplay,
        IDeviceDiagnosticsService diagnostics)
    {
        _logger = logger;
        _parserService = parserService;
        _deviceService = deviceService;
        _locationService = locationService;
        _journeyService = journeyService;
        _jsonFormatter = jsonFormatter;
        _csvFormatter = csvFormatter;
        _geofenceAlerting = geofenceAlerting;
        _routeReplay = routeReplay;
        _diagnostics = diagnostics;
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return 0;
            }

            var command = args[0].ToLower();
            var commandArgs = args.Skip(1).ToArray();

            return command switch
            {
                "parse" => await ParseFrameCommandAsync(commandArgs),
                "devices" => await ListDevicesCommandAsync(commandArgs),
                "location" => await GetLocationCommandAsync(commandArgs),
                "journey" => await GetJourneyCommandAsync(commandArgs),
                "export" => await ExportCommandAsync(commandArgs),
                "alerts" => await AlertsCommandAsync(commandArgs),
                "replay" => await ReplayCommandAsync(commandArgs),
                "diagnostics" => await DiagnosticsCommandAsync(commandArgs),
                "help" => HandleHelpCommand(commandArgs),
                _ => HandleUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command execution failed");
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> ParseFrameCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: parse <protocol> <hex-data>");
            return 1;
        }

        var protocol = args[0].ToUpper();
        var hexData = string.Join(" ", args.Skip(1));
        var rawData = hexData.HexToByteArray();

        if (rawData.Length == 0)
        {
            Console.Error.WriteLine("Invalid hex data");
            return 1;
        }

        var frame = new GpsFrame
        {
            FrameId = Guid.NewGuid().ToString(),
            RawData = rawData,
            ReceivedAt = DateTime.UtcNow,
            Protocol = Enum.Parse<ProtocolType>(protocol)
        };

        var detectedProtocol = await _parserService.DetectProtocolAsync(rawData).ConfigureAwait(false);
        Console.WriteLine($"Detected protocol: {detectedProtocol}");

        var isValid = await _parserService.ValidateFrameAsync(frame).ConfigureAwait(false);
        Console.WriteLine($"Valid frame: {isValid}");

        if (isValid)
        {
            var location = await _parserService.ParseFrameAsync(frame).ConfigureAwait(false);
            var json = _jsonFormatter.Format(location, prettyPrint: true);
            Console.WriteLine("Parsed location:");
            Console.WriteLine(json);
        }

        return 0;
    }

    private async Task<int> ListDevicesCommandAsync(string[] args)
    {
        var devices = await _deviceService.GetAllDevicesAsync().ConfigureAwait(false);

        if (!devices.Any())
        {
            Console.WriteLine("No devices found");
            return 0;
        }

        Console.WriteLine($"Total devices: {devices.Count()}");
        foreach (var device in devices)
        {
            Console.WriteLine($"  {device.Id}: {device.DeviceName} ({device.Imei}) - {device.Status}");
        }

        return 0;
    }

    private async Task<int> GetLocationCommandAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: location <device-id> [count]");
            return 1;
        }

        var deviceId = args[0];
        var count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 10;

        var locations = await _locationService.GetLocationHistoryAsync(deviceId, count).ConfigureAwait(false);

        if (!locations.Any())
        {
            Console.WriteLine($"No locations found for device {deviceId}");
            return 0;
        }

        var csv = _csvFormatter.FormatLocationHistory(locations);
        Console.WriteLine(csv);

        return 0;
    }

    private async Task<int> GetJourneyCommandAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: journey <device-id>");
            return 1;
        }

        var deviceId = args[0];
        var journeys = await _journeyService.GetJourneyHistoryAsync(deviceId).ConfigureAwait(false);

        if (!journeys.Any())
        {
            Console.WriteLine($"No journeys found for device {deviceId}");
            return 0;
        }

        foreach (var journey in journeys.Where(j => j.Status == 1))
        {
            Console.WriteLine($"Journey {journey.Id}: {journey.Waypoints.Count} waypoints, {journey.GetTotalDistance():F2}km");
        }

        return 0;
    }

    private async Task<int> ExportCommandAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: export <device-id> <format> [output-file]");
            Console.WriteLine("Formats: json, csv, geojson");
            return 1;
        }

        var deviceId = args[0];
        var format = args[1].ToLower();
        var outputFile = args.Length > 2 ? args[2] : $"export-{deviceId}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{format}";

        var locations = await _locationService.GetLocationHistoryAsync(deviceId, int.MaxValue).ConfigureAwait(false);

        if (!locations.Any())
        {
            Console.Error.WriteLine($"No data to export for device {deviceId}");
            return 1;
        }

        var content = format switch
        {
            "json" => locations.Select(l => _jsonFormatter.Format(l)).First(),
            "csv" => _csvFormatter.FormatLocationHistory(locations),
            _ => throw new InvalidOperationException($"Unsupported format: {format}")
        };

        await File.WriteAllTextAsync(outputFile, content).ConfigureAwait(false);
        Console.WriteLine($"Exported {locations.Count()} locations to {outputFile}");

        return 0;
    }


    private async Task<int> AlertsCommandAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: alerts <sub-command> [options]");
            Console.WriteLine("Sub-commands: list <device-id> | add <device-id> <geofence-id> <enter|exit> | ack <alert-id>");
            return 1;
        }

        var sub = args[0].ToLower();

        switch (sub)
        {
            case "list":
            {
                if (args.Length < 2) { Console.WriteLine("Usage: alerts list <device-id>"); return 1; }
                var deviceId = args[1];
                var active = _geofenceAlerting.GetActiveAlerts(deviceId);
                if (!active.Any())
                {
                    Console.WriteLine($"No active alerts for device {deviceId}");
                    return 0;
                }
                foreach (var a in active)
                    Console.WriteLine($"  [{a.Id[..8]}] {a.AlertType} geofence={a.GeofenceId} at {a.FiredAt:u}");
                return 0;
            }

            case "add":
            {
                if (args.Length < 4) { Console.WriteLine("Usage: alerts add <device-id> <geofence-id> <enter|exit>"); return 1; }
                var deviceId   = args[1];
                var geofenceId = args[2];
                var direction  = args[3].ToLower();
                var alertType  = direction == "exit" ? GeofenceAlertType.Exit : GeofenceAlertType.Enter;
                var rule = _geofenceAlerting.CreateAlertRule(deviceId, geofenceId, alertType);
                Console.WriteLine($"Alert rule created: {rule.Id[..8]} ({alertType} on {geofenceId} for {deviceId})");
                return 0;
            }

            case "ack":
            {
                if (args.Length < 2) { Console.WriteLine("Usage: alerts ack <alert-id>"); return 1; }
                var notes = args.Length > 2 ? string.Join(" ", args.Skip(2)) : "";
                var ok = _geofenceAlerting.AcknowledgeAlert(args[1], notes);
                Console.WriteLine(ok ? $"Alert {args[1]} acknowledged." : $"Alert {args[1]} not found.");
                return ok ? 0 : 1;
            }

            default:
                Console.Error.WriteLine($"Unknown alerts sub-command: {sub}");
                return 1;
        }
    }


    private async Task<int> ReplayCommandAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: replay <journey-id> [speed-multiplier]");
            return 1;
        }

        var journeyId = args[0];
        double multiplier = 1.0;
        if (args.Length > 1 && !double.TryParse(args[1], out multiplier))
        {
            Console.Error.WriteLine("Invalid speed multiplier; must be a positive number.");
            return 1;
        }

        try
        {
            var options = new ReplayOptions { SpeedMultiplier = multiplier };
            var result  = await _routeReplay.ReplayJourneyAsync(journeyId, options).ConfigureAwait(false);

            Console.WriteLine($"Route replay for journey {result.JourneyId} (device: {result.DeviceId})");
            Console.WriteLine($"  Frames        : {result.Frames.Count}");
            Console.WriteLine($"  Distance      : {result.TotalDistanceKm:F2} km");
            Console.WriteLine($"  Real duration : {result.OriginalDuration:hh\\:mm\\:ss}");
            Console.WriteLine($"  Replay time   : {result.ReplayDuration:hh\\:mm\\:ss} ({multiplier}x speed)");
            Console.WriteLine();
            Console.WriteLine($"{"#",-5} {"Lat",10} {"Lon",12} {"Speed",8} {"Replay Time",-25} {"Km",8}");
            foreach (var f in result.Frames)
            {
                Console.WriteLine($"{f.Index,-5} {f.Location.Latitude,10:F5} {f.Location.Longitude,12:F5} " +
                                  $"{f.Location.Speed,8:F1} {f.ReplayTimestamp,-25:u} {f.CumulativeDistanceKm,8:F2}");
            }
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Replay failed: {ex.Message}");
            return 1;
        }
    }


    private async Task<int> DiagnosticsCommandAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: diagnostics <device-id> [--selftest]");
            return 1;
        }

        var deviceId = args[0];
        var runSelfTest = args.Contains("--selftest");

        if (runSelfTest)
        {
            var selfTest = await _diagnostics.RunSelfTestAsync(deviceId).ConfigureAwait(false);
            if (selfTest is null)
            {
                Console.Error.WriteLine($"Device {deviceId} not found.");
                return 1;
            }
            Console.WriteLine($"Self-test for {deviceId}: {(selfTest.AllOk ? "PASS" : "WARN")}");
            foreach (var w in selfTest.Warnings)
                Console.WriteLine($"  ⚠ {w}");
            return selfTest.AllOk ? 0 : 2;
        }

        var report = await _diagnostics.GetDiagnosticsAsync(deviceId).ConfigureAwait(false);
        if (report is null)
        {
            Console.Error.WriteLine($"Device {deviceId} not found.");
            return 1;
        }

        Console.WriteLine($"Diagnostics for {report.DeviceName} ({report.DeviceId})");
        Console.WriteLine($"  Status          : {report.Status} (online: {report.IsOnline})");
        Console.WriteLine($"  Last seen       : {report.LastSeen:u} ({report.TimeSinceLastContact:hh\\:mm\\:ss} ago)");
        Console.WriteLine($"  Packets recv'd  : {report.TotalPacketsReceived}");
        Console.WriteLine($"  Battery         : {(report.BatteryLevel < 0 ? "unknown" : $"{report.BatteryLevel}%")}");
        Console.WriteLine($"  Signal          : {report.SignalQuality} ({report.SignalStrength} dBm)");
        Console.WriteLine($"  Location points : {report.TotalLocationPoints}");
        Console.WriteLine($"  Total distance  : {report.TotalDistanceKm:F2} km");
        Console.WriteLine($"  Journeys        : {report.TotalJourneys} total, {report.ActiveJourneys} active");

        if (report.LastLocation is not null)
            Console.WriteLine($"  Last position   : {report.LastLocation.Latitude:F6}, {report.LastLocation.Longitude:F6}");

        return 0;
    }

    private int HandleHelpCommand(string[] args)
    {
        PrintHelp();
        return 0;
    }

    private int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.Error.WriteLine("Use 'help' for usage information");
        return 1;
    }

    private void PrintHelp()
    {
        Console.WriteLine(@"
GPS Tracker Protocol - Command Line Interface

Usage: GpsTrackerProtocol <command> [options]

Commands:
  parse <protocol> <hex-data>              Parse a raw GPS frame
  devices                                   List all registered devices
  location <device-id> [count]              Get location history
  journey <device-id>                       Get journey history
  export <device-id> <format>               Export data (json, csv, geojson)
  alerts list <device-id>                   List active geofence alerts
  alerts add <device-id> <fence> <enter|exit>  Create an alert rule
  alerts ack <alert-id> [notes]             Acknowledge an alert
  replay <journey-id> [speed-multiplier]    Replay a journey's route
  diagnostics <device-id> [--selftest]      Show device diagnostics
  help [command]                            Show help information

Examples:
  GpsTrackerProtocol parse GT06 78781F120119110B162334
  GpsTrackerProtocol devices
  GpsTrackerProtocol location device-001 50
  GpsTrackerProtocol export device-001 csv output.csv
  GpsTrackerProtocol alerts add device-001 zone-hq enter
  GpsTrackerProtocol replay journey-abc 10
  GpsTrackerProtocol diagnostics device-001 --selftest
");
    }
}
