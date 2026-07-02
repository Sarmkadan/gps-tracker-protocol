#nullable enable
using System;
using System.Threading.Tasks;
using GpsTrackerProtocol;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Integration example - ASP.NET Core Dependency Injection integration
/// This demonstrates how to integrate GPS Tracker Protocol into an ASP.NET Core web application
/// </summary>
public class IntegrationExample
{
    public static async Task Run()
    {
        Console.WriteLine("=== GPS Tracker Protocol - ASP.NET Core Integration ===\n");

        // Step 1: Create and configure the web application
        // This shows how to integrate GPS Tracker Protocol with ASP.NET Core's DI system
        var builder = WebApplication.CreateBuilder(args);

        // Configure GPS Tracker Protocol services
        // =====================================
        builder.Services.AddGpsTrackerServices(options =>
        {
            // Set default protocol
            options.DefaultProtocol = ProtocolType.GT06;

            // Configure maximum devices
            options.MaxDevices = 10000;

            // Set cache expiration
            options.CacheExpirationMinutes = 60;

            // Enable protocol validation
            options.EnableProtocolValidation = true;
            options.EnableChecksumValidation = true;
        });

        // Configure logging (integrates with ASP.NET Core logging)
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Optionally configure additional services
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();

        // Step 2: Build the application
        var app = builder.Build();

        // Step 3: Configure middleware pipeline
        // ==================================

        // Use developer exception page in development
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Add endpoints for GPS data ingestion
        app.MapPost("/api/gps/ingest", IngestGpsData);
        app.MapGet("/api/gps/devices/{imei}/latest", GetLatestLocation);
        app.MapGet("/api/gps/devices/{imei}/history", GetLocationHistory);
        app.MapGet("/api/gps/devices/{imei}/analytics", GetDeviceAnalytics);
        app.MapPost("/api/gps/devices/register", RegisterDevice);

        Console.WriteLine("✓ ASP.NET Core application configured\n");

        // Step 4: Demonstrate service usage
        // ================================
        Console.WriteLine("--- Service Usage Examples ---\n");

        // Get services from DI container
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var deviceService = services.GetRequiredService<IDeviceService>();
            var locationService = services.GetRequiredService<ILocationDataService>();
            var parser = services.GetRequiredService<IProtocolParserService>();
            var logger = services.GetRequiredService<ILogger<IntegrationExample>>();

            // Register a device
            var device = new Device
            {
                Imei = "358240050447495",
                DeviceName = "Fleet Vehicle #005",
                Protocol = ProtocolType.GT06,
                IsActive = true
            };

            var registeredDevice = await deviceService.RegisterDeviceAsync(device);
            Console.WriteLine($"✓ Device registered via DI: {registeredDevice.Imei}\n");

            // Parse and store location
            byte[] rawData = new byte[] {
                0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23,
                0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00,
                0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A
            };

            var frame = new GpsFrame
            {
                RawData = rawData,
                Protocol = ProtocolType.GT06,
                ReceivedAt = DateTime.UtcNow
            };

            var location = await parser.ExtractLocationDataAsync(frame);
            if (location != null)
            {
                location.DeviceId = registeredDevice.Id;
                await locationService.StoreLocationAsync(location);
                Console.WriteLine($"✓ Location stored via DI: {location.Latitude:F6}, {location.Longitude:F6}\n");
            }
        }

        // Step 5: Endpoint handlers
        // =========================

        // POST /api/gps/ingest - Ingest raw GPS data
        async Task IngestGpsData(HttpContext context, IProtocolParserService parser, ILocationDataService locationService, IDeviceService deviceService)
        {
            try
            {
                // Read raw data from request body
                using var ms = new System.IO.MemoryStream();
                await context.Request.Body.CopyToAsync(ms);
                var rawData = ms.ToArray();

                // Detect protocol
                var protocol = await parser.DetectProtocolAsync(rawData);
                if (protocol == ProtocolType.Unknown)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Protocol detection failed");
                    return;
                }

                // Create frame
                var frame = new GpsFrame
                {
                    RawData = rawData,
                    Protocol = protocol,
                    ReceivedAt = DateTime.UtcNow
                };

                // Extract location
                var location = await parser.ExtractLocationDataAsync(frame);
                if (location == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Failed to extract location data");
                    return;
                }

                // Store location (device must be registered first)
                // In production, you'd extract device identifier from the frame
                var stored = await locationService.StoreLocationAsync(location);

                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = true,
                    Protocol = protocol.ToString(),
                    LocationId = stored.Id,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Speed = location.Speed,
                    Timestamp = location.Timestamp
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        // GET /api/gps/devices/{imei}/latest - Get latest location for device
        async Task GetLatestLocation(string imei, HttpContext context, IDeviceService deviceService, ILocationDataService locationService)
        {
            try
            {
                // Find device by IMEI
                var device = await deviceService.GetDeviceByImeiAsync(imei);
                if (device == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Device not found");
                    return;
                }

                // Get latest location
                var location = await locationService.GetLatestLocationAsync(device.Id);
                if (location == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("No location data found");
                    return;
                }

                await context.Response.WriteAsJsonAsync(new
                {
                    Device = device.DeviceName,
                    Imei = device.Imei,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Speed = location.Speed,
                    Bearing = location.Bearing,
                    Altitude = location.Altitude,
                    Timestamp = location.Timestamp
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        // GET /api/gps/devices/{imei}/history - Get location history
        async Task GetLocationHistory(string imei, int limit, HttpContext context, IDeviceService deviceService, ILocationDataService locationService)
        {
            try
            {
                var device = await deviceService.GetDeviceByImeiAsync(imei);
                if (device == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Device not found");
                    return;
                }

                var history = await locationService.GetLocationHistoryAsync(device.Id, limit);

                await context.Response.WriteAsJsonAsync(new
                {
                    Device = device.DeviceName,
                    Imei = device.Imei,
                    Locations = history.Select(l => new
                    {
                        l.Latitude,
                        l.Longitude,
                        l.Speed,
                        l.Bearing,
                        l.Timestamp
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        // GET /api/gps/devices/{imei}/analytics - Get device analytics
        async Task GetDeviceAnalytics(string imei, HttpContext context, IDeviceService deviceService, IAnalyticsService analyticsService)
        {
            try
            {
                var device = await deviceService.GetDeviceByImeiAsync(imei);
                if (device == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Device not found");
                    return;
                }

                var analytics = await analyticsService.GetDeviceAnalyticsAsync(
                    device.Id,
                    new DateRange { Start = DateTime.UtcNow.AddDays(-7), End = DateTime.UtcNow }
                );

                await context.Response.WriteAsJsonAsync(new
                {
                    Device = device.DeviceName,
                    Imei = device.Imei,
                    TotalDistanceKm = analytics.TotalDistanceKm,
                    TotalDuration = analytics.TotalDuration.TotalHours,
                    AverageSpeed = analytics.AverageSpeed,
                    MaxSpeed = analytics.MaxSpeed,
                    TotalLocationPoints = analytics.TotalLocationPoints,
                    TotalJourneys = analytics.TotalJourneys
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        // POST /api/gps/devices/register - Register a new device
        async Task RegisterDevice(HttpContext context, IDeviceService deviceService)
        {
            try
            {
                var deviceRequest = await context.Request.ReadFromJsonAsync<Device>();
                if (deviceRequest == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid device data");
                    return;
                }

                var registered = await deviceService.RegisterDeviceAsync(deviceRequest);

                await context.Response.WriteAsJsonAsync(new
                {
                    Success = true,
                    Device = new
                    {
                        Id = registered.Id,
                        Imei = registered.Imei,
                        DeviceName = registered.DeviceName,
                        Protocol = registered.Protocol
                    }
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"Error: {ex.Message}");
            }
        }

        // Step 6: Run the application
        // ==========================
        Console.WriteLine("✓ Starting ASP.NET Core application...\n");
        Console.WriteLine("Available endpoints:");
        Console.WriteLine("  POST /api/gps/ingest - Ingest GPS data");
        Console.WriteLine("  GET /api/gps/devices/{imei}/latest - Get latest location");
        Console.WriteLine("  GET /api/gps/devices/{imei}/history - Get location history");
        Console.WriteLine("  GET /api/gps/devices/{imei}/analytics - Get device analytics");
        Console.WriteLine("  POST /api/gps/devices/register - Register device\n");

        Console.WriteLine("=== Integration Example Complete ===");
        Console.WriteLine("To run this application, use:");
        Console.WriteLine("  dotnet run --project <your-project-file>\n");
    }
}

// Helper class for date range
public class DateRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}