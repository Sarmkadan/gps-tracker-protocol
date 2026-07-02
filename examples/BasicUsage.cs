#nullable enable
using System;
using System.Threading.Tasks;
using GpsTrackerProtocol;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Basic usage example - Minimal setup and first call
/// This demonstrates the simplest possible way to use the GPS Tracker Protocol library
/// </summary>
public class BasicUsage
{
    public static async Task Run()
    {
        Console.WriteLine("=== GPS Tracker Protocol - Basic Usage ===\n");

        // Step 1: Setup services (minimal configuration)
        // This is the simplest way to get started - just register the core services
        var services = new ServiceCollection();
        services.AddGpsTrackerServices(); // Registers all core services

        var provider = services.BuildServiceProvider();

        // Step 2: Get the protocol parser service
        // This service handles protocol detection, validation, and parsing
        var parser = provider.GetRequiredService<IProtocolParserService>();

        // Step 3: Get device and location services
        var deviceService = provider.GetRequiredService<IDeviceService>();
        var locationService = provider.GetRequiredService<ILocationDataService>();

        Console.WriteLine("✓ Services initialized\n");

        // Step 4: Register a tracking device
        // Every GPS device needs to be registered before we can store its locations
        var device = new Device
        {
            Imei = "358240050447491", // Unique device identifier
            DeviceName = "Company Vehicle #001",
            Protocol = ProtocolType.GT06, // GT06 is the most common protocol
            IsActive = true
        };

        var registeredDevice = await deviceService.RegisterDeviceAsync(device);
        Console.WriteLine($"✓ Device registered:");
        Console.WriteLine($"  ID: {registeredDevice.Id}");
        Console.WriteLine($"  Name: {registeredDevice.DeviceName}");
        Console.WriteLine($"  IMEI: {registeredDevice.Imei}\n");

        // Step 5: Parse a GPS frame
        // This is what you'd receive from a GPS tracker device over TCP/UDP
        byte[] rawGpsData = new byte[] {
            0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23,
            0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00,
            0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A
        };

        // Create a frame object
        var frame = new GpsFrame
        {
            RawData = rawGpsData,
            Protocol = ProtocolType.GT06,
            ReceivedAt = DateTime.UtcNow
        };

        // Detect protocol automatically
        var detectedProtocol = await parser.DetectProtocolAsync(rawGpsData);
        Console.WriteLine($"✓ Protocol detected: {detectedProtocol}");

        // Validate the frame (checksum validation)
        bool isValid = await parser.ValidateFrameAsync(frame);
        Console.WriteLine($"✓ Frame validation: {(isValid ? "PASSED" : "FAILED")}\n");

        // Step 6: Extract location data from the frame
        var locationData = await parser.ExtractLocationDataAsync(frame);

        if (locationData != null)
        {
            Console.WriteLine("✓ Location extracted:");
            Console.WriteLine($"  Latitude: {locationData.Latitude:F6}°");
            Console.WriteLine($"  Longitude: {locationData.Longitude:F6}°");
            Console.WriteLine($"  Speed: {locationData.Speed:F1} km/h");
            Console.WriteLine($"  Bearing: {locationData.Bearing:F1}°");
            Console.WriteLine($"  Altitude: {locationData.Altitude:F1} m");
            Console.WriteLine($"  Satellites: {locationData.SatelliteCount}");
            Console.WriteLine($"  Accuracy: {locationData.Accuracy:F1} m");
            Console.WriteLine($"  Timestamp: {locationData.Timestamp:yyyy-MM-dd HH:mm:ss UTC}\n");

            // Step 7: Store the location
            locationData.DeviceId = registeredDevice.Id;
            var storedLocation = await locationService.StoreLocationAsync(locationData);
            Console.WriteLine($"✓ Location stored with ID: {storedLocation.Id}\n");
        }
        else
        {
            Console.WriteLine("✗ Failed to extract location data from frame\n");
        }

        // Step 8: Query the latest location
        var latestLocation = await locationService.GetLatestLocationAsync(registeredDevice.Id);

        if (latestLocation != null)
        {
            Console.WriteLine("✓ Latest location retrieved:");
            Console.WriteLine($"  Device: {registeredDevice.DeviceName}");
            Console.WriteLine($"  Position: {latestLocation.Latitude:F6}, {latestLocation.Longitude:F6}");
            Console.WriteLine($"  Speed: {latestLocation.Speed:F1} km/h\n");
        }

        // Step 9: Clean up
        await deviceService.UnregisterDeviceAsync(registeredDevice.Id);
        Console.WriteLine("✓ Device unregistered\n");

        Console.WriteLine("=== Basic Usage Complete ===");
    }
}
