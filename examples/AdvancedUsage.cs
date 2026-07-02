#nullable enable
using System;
using System.Threading.Tasks;
using GpsTrackerProtocol;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Advanced usage example - Configuration, custom options, and error handling
/// This demonstrates advanced features including custom configuration, error handling,
/// multiple protocols, and advanced service configuration
/// </summary>
public class AdvancedUsage
{
    public static async Task Run()
    {
        Console.WriteLine("=== GPS Tracker Protocol - Advanced Usage ===\n");

        // Advanced Configuration Options
        // ============================

        // Option 1: Configure from appsettings.json (recommended for production)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables("GPS_")
            .Build();

        var services = new ServiceCollection();

        // Advanced service configuration with custom options
        services.AddGpsTrackerServices(options =>
        {
            // Configure default protocol
            options.DefaultProtocol = ProtocolType.GT06;

            // Set maximum number of devices to track
            options.MaxDevices = 10000;

            // Configure cache expiration (in minutes)
            options.CacheExpirationMinutes = 30;

            // Enable/disable protocol-specific features
            options.EnableProtocolValidation = true;
            options.EnableChecksumValidation = true;

            // Configure rate limiting
            options.RateLimitPerMinute = 1000;

            // Configure location history limits
            options.LocationHistoryLimit = 10000;
        });

        // Option 2: Configure logging (detailed logging for debugging)
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug); // Debug for development, Information for production
        });

        // Option 3: Configure caching (in-memory by default)
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024; // Maximum number of cached items
        });

        // Option 4: Configure specific protocols with custom settings
        services.ConfigureProtocol<ProtocolSettings>(ProtocolType.GT06, settings =>
        {
            settings.Enabled = true;
            settings.TimeoutSeconds = 30;
            settings.MaxFrameSize = 200;
            settings.ChecksumValidation = true;
        });

        services.ConfigureProtocol<ProtocolSettings>(ProtocolType.H02, settings =>
        {
            settings.Enabled = true;
            settings.TimeoutSeconds = 30;
            settings.MaxFrameSize = 300;
            settings.ChecksumValidation = true;
        });

        services.ConfigureProtocol<ProtocolSettings>(ProtocolType.TK103, settings =>
        {
            settings.Enabled = true;
            settings.TimeoutSeconds = 30;
            settings.MaxFrameSize = 100;
            settings.ChecksumValidation = true;
        });

        var provider = services.BuildServiceProvider();

        // Get services
        var parser = provider.GetRequiredService<IProtocolParserService>();
        var deviceService = provider.GetRequiredService<IDeviceService>();
        var locationService = provider.GetRequiredService<ILocationDataService>();
        var journeyService = provider.GetRequiredService<IJourneyService>();
        var analyticsService = provider.GetRequiredService<IAnalyticsService>();
        var logger = provider.GetRequiredService<ILogger<AdvancedUsage>>();

        Console.WriteLine("✓ Advanced services configured\n");

        // Error Handling
        // =============
        Console.WriteLine("--- Error Handling Examples ---\n");

        try
        {
            // Example: Try to register a device with invalid IMEI
            var invalidDevice = new Device
            {
                Imei = "INVALID",
                DeviceName = "Test Device",
                Protocol = ProtocolType.GT06
            };

            var result = await deviceService.RegisterDeviceAsync(invalidDevice);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is ValidationException)
        {
            logger.LogError(ex, "Failed to register device: {Message}", ex.Message);
            Console.WriteLine($"✓ Caught expected error: {ex.GetType().Name}");
            Console.WriteLine($"  Message: {ex.Message}\n");
        }

        // Multiple Protocols
        // ====================
        Console.WriteLine("--- Multi-Protocol Support ---\n");

        // Register devices with different protocols
        var gt06Device = new Device
        {
            Imei = "358240050447492",
            DeviceName = "GT06 Tracker",
            Protocol = ProtocolType.GT06
        };

        var h02Device = new Device
        {
            Imei = "358240050447493",
            DeviceName = "H02 Tracker",
            Protocol = ProtocolType.H02
        };

        var tk103Device = new Device
        {
            Imei = "358240050447494",
            DeviceName = "TK103 Tracker",
            Protocol = ProtocolType.TK103
        };

        var gt06Registered = await deviceService.RegisterDeviceAsync(gt06Device);
        var h02Registered = await deviceService.RegisterDeviceAsync(h02Device);
        var tk103Registered = await deviceService.RegisterDeviceAsync(tk103Device);

        Console.WriteLine($"✓ Registered devices:");
        Console.WriteLine($"  GT06: {gt06Registered.Imei}");
        Console.WriteLine($"  H02: {h02Registered.Imei}");
        Console.WriteLine($"  TK103: {tk103Registered.Imei}\n");

        // Protocol-specific parsing examples
        // GT06 (Binary protocol)
        byte[] gt06Data = new byte[] { 0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23, 0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A };

        var gt06Frame = new GpsFrame { RawData = gt06Data, Protocol = ProtocolType.GT06, ReceivedAt = DateTime.UtcNow };
        var gt06Location = await parser.ExtractLocationDataAsync(gt06Frame);

        if (gt06Location != null)
        {
            gt06Location.DeviceId = gt06Registered.Id;
            await locationService.StoreLocationAsync(gt06Location);
            Console.WriteLine($"✓ GT06 location stored: {gt06Location.Latitude:F6}, {gt06Location.Longitude:F6}\n");
        }

        // H02 (ASCII protocol)
        string h02Data = "$GPRMC,123456.00,A,4044.2132,N,07400.2154,W,11.2,358.4,280626,0.0,E*62";
        var h02Bytes = System.Text.Encoding.ASCII.GetBytes(h02Data);
        var h02Frame = new GpsFrame { RawData = h02Bytes, Protocol = ProtocolType.H02, ReceivedAt = DateTime.UtcNow };
        var h02Location = await parser.ExtractLocationDataAsync(h02Frame);

        if (h02Location != null)
        {
            h02Location.DeviceId = h02Registered.Id;
            await locationService.StoreLocationAsync(h02Location);
            Console.WriteLine($"✓ H02 location stored: {h02Location.Latitude:F6}, {h02Location.Longitude:F6}\n");
        }

        // TK103 (Binary protocol)
        byte[] tk103Data = new byte[] { 0x78, 0x78, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23, 0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A };
        var tk103Frame = new GpsFrame { RawData = tk103Data, Protocol = ProtocolType.TK103, ReceivedAt = DateTime.UtcNow };
        var tk103Location = await parser.ExtractLocationDataAsync(tk103Frame);

        if (tk103Location != null)
        {
            tk103Location.DeviceId = tk103Registered.Id;
            await locationService.StoreLocationAsync(tk103Location);
            Console.WriteLine($"✓ TK103 location stored: {tk103Location.Latitude:F6}, {tk103Location.Longitude:F6}\n");
        }

        // Advanced Queries
        // =================
        Console.WriteLine("--- Advanced Queries ---\n");

        // Get all active devices
        var activeDevices = await deviceService.GetActiveDevicesAsync();
        Console.WriteLine($"✓ Active devices: {activeDevices.Count()}");
        foreach (var dev in activeDevices)
        {
            Console.WriteLine($"  - {dev.DeviceName} ({dev.Imei})");
        }
        Console.WriteLine();

        // Get locations by date range
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var recentLocations = locationService.GetLocationsByDateRangeAsync(gt06Registered.Id, startTime, endTime);
        Console.WriteLine($"✓ Recent locations for {gt06Registered.DeviceName}: {recentLocations.Result.Count()} records\n");

        // Get locations by region (within 10km of New York City)
        var nycLat = 40.7128;
        var nycLng = -74.0060;
        var regionLocations = await locationService.GetLocationsByRegionAsync(nycLat, nycLng, 10);
        Console.WriteLine($"✓ Locations within 10km of NYC: {regionLocations.Count()} records\n");

        // Journey tracking
        Console.WriteLine("--- Journey Tracking ---\n");

        // Start a journey
        var journey = await journeyService.StartJourneyAsync(gt06Registered.Id);
        Console.WriteLine($"✓ Journey started: {journey.Id}");

        // Add waypoints (simulate tracking over time)
        var waypoint1 = new LocationData
        {
            DeviceId = gt06Registered.Id,
            Latitude = 40.7128,
            Longitude = -74.0060,
            Speed = 0,
            Bearing = 0,
            Timestamp = DateTime.UtcNow.AddMinutes(-30)
        };

        var waypoint2 = new LocationData
        {
            DeviceId = gt06Registered.Id,
            Latitude = 40.7200,
            Longitude = -73.9900,
            Speed = 45.5,
            Bearing = 90,
            Timestamp = DateTime.UtcNow.AddMinutes(-20)
        };

        await journeyService.AddWaypointAsync(journey.Id, waypoint1);
        await journeyService.AddWaypointAsync(journey.Id, waypoint2);
        Console.WriteLine($"✓ Added 2 waypoints to journey\n");

        // Get journey analytics
        var journeyAnalytics = await analyticsService.GetDeviceAnalyticsAsync(
            gt06Registered.Id,
            new DateRange { Start = DateTime.UtcNow.AddDays(-1), End = DateTime.UtcNow }
        );

        Console.WriteLine("✓ Journey Analytics:");
        Console.WriteLine($"  Total distance: {journeyAnalytics.TotalDistanceKm:F2} km");
        Console.WriteLine($"  Total duration: {journeyAnalytics.TotalDuration.TotalMinutes:F0} minutes");
        Console.WriteLine($"  Average speed: {journeyAnalytics.AverageSpeed:F1} km/h");
        Console.WriteLine($"  Max speed: {journeyAnalytics.MaxSpeed:F1} km/h");
        Console.WriteLine($"  Location points: {journeyAnalytics.TotalLocationPoints}\n");

        // Clean up
        await deviceService.UnregisterDeviceAsync(gt06Registered.Id);
        await deviceService.UnregisterDeviceAsync(h02Registered.Id);
        await deviceService.UnregisterDeviceAsync(tk103Registered.Id);

        Console.WriteLine("=== Advanced Usage Complete ===");
    }
}

// Helper class for date range
public class DateRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}