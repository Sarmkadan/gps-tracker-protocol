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
/// Interactive command-line interface for managing GPS tracker devices,
/// sending configuration commands, and querying device status.
/// </summary>
public class DeviceCommandCenter
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<DeviceCommandCenter> _logger;
    private readonly IDeviceService _deviceService;
    private readonly ICommandService _commandService;
    private readonly ILocationDataService _locationService;

    public DeviceCommandCenter()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<DeviceCommandCenter>>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
        _commandService = _provider.GetRequiredService<ICommandService>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();
    }

    /// <summary>Displays interactive command menu</summary>
    public async Task RunInteractiveAsync()
    {
        _logger.LogInformation("=== GPS Tracker Device Command Center ===\n");

        bool running = true;
        while (running)
        {
            DisplayMenu();
            Console.Write("\nSelect option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await RegisterDeviceAsync().ConfigureAwait(false);
                    break;
                case "2":
                    await ListDevicesAsync().ConfigureAwait(false);
                    break;
                case "3":
                    await QueryDeviceAsync().ConfigureAwait(false);
                    break;
                case "4":
                    await SendCommandAsync().ConfigureAwait(false);
                    break;
                case "5":
                    await ViewCommandHistoryAsync().ConfigureAwait(false);
                    break;
                case "6":
                    await GetDeviceLocationAsync().ConfigureAwait(false);
                    break;
                case "7":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option");
                    break;
            }
        }
    }

    /// <summary>Displays the main menu options</summary>
    private void DisplayMenu()
    {
        Console.WriteLine("\n--- Menu ---");
        Console.WriteLine("1. Register new device");
        Console.WriteLine("2. List all devices");
        Console.WriteLine("3. Query device details");
        Console.WriteLine("4. Send device command");
        Console.WriteLine("5. View command history");
        Console.WriteLine("6. Get device location");
        Console.WriteLine("7. Exit");
    }

    /// <summary>Registers a new device interactively</summary>
    private async Task RegisterDeviceAsync()
    {
        Console.Write("\nEnter IMEI: ");
        var imei = Console.ReadLine();

        Console.Write("Enter device name: ");
        var name = Console.ReadLine();

        Console.Write("Enter protocol (GT06/H02/TK103): ");
        var protocolStr = Console.ReadLine();

        if (!Enum.TryParse<ProtocolType>(protocolStr, out var protocol))
        {
            _logger.LogWarning("Invalid protocol");
            return;
        }

        var device = new Device
        {
            Imei = imei ?? "",
            DeviceName = name ?? "",
            Protocol = protocol,
            IsActive = true
        };

        var registered = await _deviceService.RegisterDeviceAsync(device).ConfigureAwait(false);
        _logger.LogInformation("Device registered: {0} (ID: {1})", registered.DeviceName, registered.Id);
    }

    /// <summary>Lists all registered devices</summary>
    private async Task ListDevicesAsync()
    {
        var devices = await _deviceService.GetAllDevicesAsync().ConfigureAwait(false);
        var deviceList = devices.ToList();

        Console.WriteLine("\n--- Registered Devices ---");
        Console.WriteLine("{0,-20} {1,-30} {2,-15} {3,-10}", "ID", "Name", "IMEI", "Protocol");
        Console.WriteLine(new string('-', 75));

        foreach (var device in deviceList)
        {
            Console.WriteLine("{0,-20} {1,-30} {2,-15} {3,-10}",
                device.Id, device.DeviceName, device.Imei, device.Protocol);
        }

        Console.WriteLine($"\nTotal: {deviceList.Count} devices");
    }

    /// <summary>Queries detailed information about a specific device</summary>
    private async Task QueryDeviceAsync()
    {
        Console.Write("\nEnter device ID: ");
        var deviceId = Console.ReadLine();

        var device = await _deviceService.GetDeviceAsync(deviceId ?? "").ConfigureAwait(false);
        if (device is null)
        {
            _logger.LogWarning("Device not found");
            return;
        }

        Console.WriteLine("\n--- Device Details ---");
        Console.WriteLine($"ID: {device.Id}");
        Console.WriteLine($"Name: {device.DeviceName}");
        Console.WriteLine($"IMEI: {device.Imei}");
        Console.WriteLine($"Protocol: {device.Protocol}");
        Console.WriteLine($"Status: {device.Status}");
        Console.WriteLine($"Active: {device.IsActive}");
        Console.WriteLine($"Registered: {device.RegisteredAt:G}");
        Console.WriteLine($"Last update: {device.LastUpdateAt:G}");
    }

    /// <summary>Sends a command to a device</summary>
    private async Task SendCommandAsync()
    {
        Console.Write("\nEnter device ID: ");
        var deviceId = Console.ReadLine();

        var device = await _deviceService.GetDeviceAsync(deviceId ?? "").ConfigureAwait(false);
        if (device is null)
        {
            _logger.LogWarning("Device not found");
            return;
        }

        Console.WriteLine("\nAvailable commands:");
        Console.WriteLine("1. SetGpsInterval");
        Console.WriteLine("2. SetAlarmThreshold");
        Console.WriteLine("3. EnableTracking");
        Console.WriteLine("4. DisableTracking");
        Console.WriteLine("5. RestoreFactory");

        Console.Write("\nSelect command: ");
        var cmdChoice = Console.ReadLine();

        var commandType = cmdChoice switch
        {
            "1" => CommandType.SetGpsInterval,
            "2" => CommandType.SetAlarmThreshold,
            "3" => CommandType.EnableTracking,
            "4" => CommandType.DisableTracking,
            "5" => CommandType.RestoreFactory,
            _ => CommandType.SetGpsInterval
        };

        var parameters = new Dictionary<string, object>();

        if (commandType == CommandType.SetGpsInterval)
        {
            Console.Write("Enter interval (seconds): ");
            if (int.TryParse(Console.ReadLine(), out var interval))
                parameters["interval"] = interval;
        }

        var command = new Command
        {
            DeviceId = deviceId ?? "",
            Type = commandType,
            Parameters = parameters,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _commandService.CreateCommandAsync(command).ConfigureAwait(false);
        var executed = await _commandService.ExecuteCommandAsync(created.Id).ConfigureAwait(false);

        _logger.LogInformation("Command {0} executed: {1}", created.Id, executed);
    }

    /// <summary>Views command history for a device</summary>
    private async Task ViewCommandHistoryAsync()
    {
        Console.Write("\nEnter device ID: ");
        var deviceId = Console.ReadLine();

        var commands = await _commandService.GetCommandHistoryAsync(deviceId ?? "").ConfigureAwait(false);
        var commandList = commands.ToList();

        Console.WriteLine("\n--- Command History ---");
        Console.WriteLine("{0,-20} {1,-20} {2,-20}", "Command ID", "Type", "Created At");
        Console.WriteLine(new string('-', 60));

        foreach (var cmd in commandList.Take(20))
        {
            Console.WriteLine("{0,-20} {1,-20} {2,-20}",
                cmd.Id.Substring(0, Math.Min(20, cmd.Id.Length)),
                cmd.Type,
                cmd.CreatedAt.ToString("g"));
        }

        Console.WriteLine($"\nTotal: {commandList.Count} commands");
    }

    /// <summary>Gets the latest location for a device</summary>
    private async Task GetDeviceLocationAsync()
    {
        Console.Write("\nEnter device ID: ");
        var deviceId = Console.ReadLine();

        var location = await _locationService.GetLatestLocationAsync(deviceId ?? "").ConfigureAwait(false);
        if (location is null)
        {
            _logger.LogWarning("No location data found");
            return;
        }

        Console.WriteLine("\n--- Latest Location ---");
        Console.WriteLine($"Device ID: {location.DeviceId}");
        Console.WriteLine($"Latitude: {location.Latitude:F6}");
        Console.WriteLine($"Longitude: {location.Longitude:F6}");
        Console.WriteLine($"Speed: {location.Speed:F1} km/h");
        Console.WriteLine($"Bearing: {location.Bearing:F1}°");
        Console.WriteLine($"Altitude: {location.Altitude:F1}m");
        Console.WriteLine($"Satellites: {location.SatelliteCount}");
        Console.WriteLine($"Accuracy: {location.Accuracy:F1}m");
        Console.WriteLine($"Timestamp: {location.Timestamp:G}");
    }

    public static async Task Main(string[] args)
    {
        var center = new DeviceCommandCenter();
        await center.RunInteractiveAsync().ConfigureAwait(false);
    }
}
