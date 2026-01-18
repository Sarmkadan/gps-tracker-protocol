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

    public CommandLineInterface(
        ILogger<CommandLineInterface> logger,
        IProtocolParserService parserService,
        IDeviceService deviceService,
        ILocationDataService locationService,
        IJourneyService journeyService,
        IJsonFormatter jsonFormatter,
        ICsvFormatter csvFormatter)
    {
        _logger = logger;
        _parserService = parserService;
        _deviceService = deviceService;
        _locationService = locationService;
        _journeyService = journeyService;
        _jsonFormatter = jsonFormatter;
        _csvFormatter = csvFormatter;
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

        var detectedProtocol = await _parserService.DetectProtocolAsync(rawData);
        Console.WriteLine($"Detected protocol: {detectedProtocol}");

        var isValid = await _parserService.ValidateFrameAsync(frame);
        Console.WriteLine($"Valid frame: {isValid}");

        if (isValid)
        {
            var location = await _parserService.ParseFrameAsync(frame);
            var json = _jsonFormatter.Format(location, prettyPrint: true);
            Console.WriteLine("Parsed location:");
            Console.WriteLine(json);
        }

        return 0;
    }

    private async Task<int> ListDevicesCommandAsync(string[] args)
    {
        var devices = await _deviceService.GetAllDevicesAsync();

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

        var locations = await _locationService.GetLocationHistoryAsync(deviceId, count);

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
        var journeys = await _journeyService.GetJourneyHistoryAsync(deviceId);

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

        var locations = await _locationService.GetLocationHistoryAsync(deviceId, int.MaxValue);

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

        await File.WriteAllTextAsync(outputFile, content);
        Console.WriteLine($"Exported {locations.Count()} locations to {outputFile}");

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
  parse <protocol> <hex-data>   Parse a raw GPS frame
  devices                        List all registered devices
  location <device-id> [count]   Get location history
  journey <device-id>            Get journey history
  export <device-id> <format>    Export data (json, csv, geojson)
  help [command]                 Show help information

Examples:
  GpsTrackerProtocol parse GT06 78781F120119110B162334
  GpsTrackerProtocol devices
  GpsTrackerProtocol location device-001 50
  GpsTrackerProtocol export device-001 csv output.csv
");
    }
}
