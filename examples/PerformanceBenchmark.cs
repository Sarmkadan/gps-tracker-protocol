#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Performance benchmark suite for GPS tracker protocol parsing and storage operations.
/// Measures throughput, latency, and memory usage under various load scenarios.
/// </summary>
public class PerformanceBenchmark
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<PerformanceBenchmark> _logger;
    private readonly IDeviceService _deviceService;
    private readonly ILocationDataService _locationService;
    private readonly IProtocolParserService _parserService;

    public PerformanceBenchmark()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<PerformanceBenchmark>>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();
        _parserService = _provider.GetRequiredService<IProtocolParserService>();
    }

    /// <summary>Benchmarks frame validation performance</summary>
    public async Task BenchmarkFrameValidationAsync(int frameCount = 10000)
    {
        _logger.LogInformation("Benchmarking frame validation ({0} frames)...", frameCount);

        var frame = new GpsFrame
        {
            RawData = new byte[] { 0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23,
                0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00, 0x00, 0xA2, 0x00,
                0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A },
            Protocol = ProtocolType.GT06
        };

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < frameCount; i++)
        {
            await _parserService.ValidateFrameAsync(frame).ConfigureAwait(false);
        }

        sw.Stop();

        double framesPerSecond = frameCount / sw.Elapsed.TotalSeconds;
        double avgLatency = sw.Elapsed.TotalMilliseconds / frameCount;

        _logger.LogInformation("=== Frame Validation Results ===");
        _logger.LogInformation("Total frames: {0}", frameCount);
        _logger.LogInformation("Total time: {0:F2}ms", sw.Elapsed.TotalMilliseconds);
        _logger.LogInformation("Frames/sec: {0:F0}", framesPerSecond);
        _logger.LogInformation("Avg latency: {0:F3}ms", avgLatency);
    }

    /// <summary>Benchmarks location storage performance</summary>
    public async Task BenchmarkLocationStorageAsync(int locationCount = 10000)
    {
        _logger.LogInformation("Benchmarking location storage ({0} locations)...", locationCount);

        var device = new Device
        {
            Imei = "358240050447491",
            DeviceName = "Benchmark Device",
            Protocol = ProtocolType.GT06,
            IsActive = true
        };

        var registered = await _deviceService.RegisterDeviceAsync(device).ConfigureAwait(false);

        var sw = Stopwatch.StartNew();
        var initialMemory = GC.GetTotalMemory(false);

        for (int i = 0; i < locationCount; i++)
        {
            var location = new LocationData
            {
                DeviceId = registered.Id,
                Latitude = 40.7128 + (i * 0.00001),
                Longitude = -74.0060 + (i * 0.00001),
                Speed = 50.0 + (i % 100),
                Bearing = (i * 36) % 360,
                Altitude = 10.0,
                Timestamp = DateTime.UtcNow.AddSeconds(i),
                SatelliteCount = 12,
                Accuracy = 5.0
            };

            await _locationService.StoreLocationAsync(location).ConfigureAwait(false);
        }

        sw.Stop();
        var finalMemory = GC.GetTotalMemory(false);
        var memoryUsed = (finalMemory - initialMemory) / 1024 / 1024;

        double locationsPerSecond = locationCount / sw.Elapsed.TotalSeconds;
        double avgLatency = sw.Elapsed.TotalMilliseconds / locationCount;

        _logger.LogInformation("=== Location Storage Results ===");
        _logger.LogInformation("Total locations: {0}", locationCount);
        _logger.LogInformation("Total time: {0:F2}ms", sw.Elapsed.TotalMilliseconds);
        _logger.LogInformation("Locations/sec: {0:F0}", locationsPerSecond);
        _logger.LogInformation("Avg latency: {0:F3}ms", avgLatency);
        _logger.LogInformation("Memory used: {0:F2}MB", memoryUsed);
    }

    /// <summary>Benchmarks location query performance</summary>
    public async Task BenchmarkLocationQueryAsync(int locationCount = 10000, int queryCount = 1000)
    {
        _logger.LogInformation("Benchmarking location queries ({0} queries on {1} locations)...",
            queryCount, locationCount);

        var device = new Device
        {
            Imei = "358240050447491",
            DeviceName = "Query Benchmark Device",
            Protocol = ProtocolType.GT06,
            IsActive = true
        };

        var registered = await _deviceService.RegisterDeviceAsync(device).ConfigureAwait(false);

        // Populate data
        for (int i = 0; i < locationCount; i++)
        {
            var location = new LocationData
            {
                DeviceId = registered.Id,
                Latitude = 40.7128 + (i * 0.00001),
                Longitude = -74.0060 + (i * 0.00001),
                Speed = 50.0,
                Timestamp = DateTime.UtcNow.AddSeconds(i),
                SatelliteCount = 12,
                Accuracy = 5.0
            };

            await _locationService.StoreLocationAsync(location).ConfigureAwait(false);
        }

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < queryCount; i++)
        {
            var latest = await _locationService.GetLatestLocationAsync(registered.Id).ConfigureAwait(false);
            var history = await _locationService.GetLocationHistoryAsync(registered.Id, 100).ConfigureAwait(false);
        }

        sw.Stop();

        double queriesPerSecond = queryCount / sw.Elapsed.TotalSeconds;
        double avgLatency = sw.Elapsed.TotalMilliseconds / queryCount;

        _logger.LogInformation("=== Location Query Results ===");
        _logger.LogInformation("Total queries: {0}", queryCount);
        _logger.LogInformation("Total time: {0:F2}ms", sw.Elapsed.TotalMilliseconds);
        _logger.LogInformation("Queries/sec: {0:F0}", queriesPerSecond);
        _logger.LogInformation("Avg latency: {0:F3}ms", avgLatency);
    }

    /// <summary>Runs comprehensive stress test</summary>
    public async Task RunStressTestAsync(int devices = 100, int locationsPerDevice = 100)
    {
        _logger.LogInformation("Running stress test ({0} devices, {1} locations each)...",
            devices, locationsPerDevice);

        var sw = Stopwatch.StartNew();
        var initialMemory = GC.GetTotalMemory(false);

        // Register devices
        var deviceList = new List<Device>();
        for (int d = 0; d < devices; d++)
        {
            var device = new Device
            {
                Imei = $"35824005044749{d:D2}",
                DeviceName = $"Stress Device {d}",
                Protocol = ProtocolType.GT06,
                IsActive = true
            };

            var registered = await _deviceService.RegisterDeviceAsync(device).ConfigureAwait(false);
            deviceList.Add(registered);
        }

        // Add locations for each device
        var locationCount = 0;
        foreach (var device in deviceList)
        {
            for (int l = 0; l < locationsPerDevice; l++)
            {
                var location = new LocationData
                {
                    DeviceId = device.Id,
                    Latitude = 40.7128 + (l * 0.0001),
                    Longitude = -74.0060 + (l * 0.0001),
                    Speed = 50.0 + (l % 50),
                    Timestamp = DateTime.UtcNow.AddSeconds(l),
                    SatelliteCount = 12,
                    Accuracy = 5.0
                };

                await _locationService.StoreLocationAsync(location).ConfigureAwait(false);
                locationCount++;
            }
        }

        sw.Stop();
        var finalMemory = GC.GetTotalMemory(false);
        var memoryUsed = (finalMemory - initialMemory) / 1024 / 1024;

        _logger.LogInformation("=== Stress Test Results ===");
        _logger.LogInformation("Devices: {0}", devices);
        _logger.LogInformation("Total locations: {0}", locationCount);
        _logger.LogInformation("Total time: {0:F2}ms", sw.Elapsed.TotalMilliseconds);
        _logger.LogInformation("Locations/sec: {0:F0}", locationCount / sw.Elapsed.TotalSeconds);
        _logger.LogInformation("Memory used: {0:F2}MB", memoryUsed);
    }

    public static async Task Main(string[] args)
    {
        var benchmark = new PerformanceBenchmark();

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: PerformanceBenchmark <test> [args]");
            Console.WriteLine("Tests:");
            Console.WriteLine("  validation [count] - Frame validation benchmark");
            Console.WriteLine("  storage [count] - Location storage benchmark");
            Console.WriteLine("  query [locations] [queries] - Query performance benchmark");
            Console.WriteLine("  stress [devices] [locations] - Full stress test");
            Console.WriteLine("  all - Run all benchmarks");
            return;
        }

        var test = args[0].ToLower();

        switch (test)
        {
            case "validation":
                {
                    int count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 10000;
                    await benchmark.BenchmarkFrameValidationAsync(count).ConfigureAwait(false);
                }
                break;

            case "storage":
                {
                    int count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 10000;
                    await benchmark.BenchmarkLocationStorageAsync(count).ConfigureAwait(false);
                }
                break;

            case "query":
                {
                    int locs = args.Length > 1 && int.TryParse(args[1], out var l) ? l : 10000;
                    int queries = args.Length > 2 && int.TryParse(args[2], out var q) ? q : 1000;
                    await benchmark.BenchmarkLocationQueryAsync(locs, queries).ConfigureAwait(false);
                }
                break;

            case "stress":
                {
                    int devices = args.Length > 1 && int.TryParse(args[1], out var d) ? d : 100;
                    int locs = args.Length > 2 && int.TryParse(args[2], out var l) ? l : 100;
                    await benchmark.RunStressTestAsync(devices, locs).ConfigureAwait(false);
                }
                break;

            case "all":
                await benchmark.BenchmarkFrameValidationAsync(10000).ConfigureAwait(false);
                Console.WriteLine();
                await benchmark.BenchmarkLocationStorageAsync(10000).ConfigureAwait(false);
                Console.WriteLine();
                await benchmark.BenchmarkLocationQueryAsync(5000, 500).ConfigureAwait(false);
                Console.WriteLine();
                await benchmark.RunStressTestAsync(50, 100).ConfigureAwait(false);
                break;

            default:
                Console.WriteLine("Unknown test: {0}", test);
                break;
        }
    }
}
