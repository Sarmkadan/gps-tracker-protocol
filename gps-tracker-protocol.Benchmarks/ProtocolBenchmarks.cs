using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GpsTrackerProtocol.Benchmarks;

/// <summary>
/// Benchmarks for GPS tracker protocol parsing operations.
/// Measures throughput and memory allocation for critical protocol operations.
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[CategoriesColumn]
public class ProtocolBenchmarks
{
    private IServiceProvider _provider;
    private IProtocolParserService _parserService;
    private IDeviceService _deviceService;
    private ILocationDataService _locationService;

    private GpsFrame _gt06Frame;
    private GpsFrame _h02Frame;
    private GpsFrame _tk103Frame;
    private Device _testDevice;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        _provider = services.BuildServiceProvider();

        _parserService = _provider.GetRequiredService<IProtocolParserService>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();

        // Setup test frames for all protocols
        _gt06Frame = new GpsFrame
        {
            RawData = new byte[] {
                0x78, 0x78, 0x1F, 0x12, 0x01, 0x19, 0x11, 0x0B, 0x16, 0x23,
                0x34, 0x02, 0xA2, 0xC0, 0x40, 0x05, 0x27, 0x6F, 0xD1, 0x00,
                0x00, 0xA2, 0x00, 0x00, 0x08, 0xFF, 0xFE, 0xF3, 0x0D, 0x0A
            },
            Protocol = ProtocolType.GT06,
            ReceivedAt = DateTime.UtcNow
        };

        _h02Frame = new GpsFrame
        {
            RawData = System.Text.Encoding.ASCII.GetBytes(
                "*HQ,358240050447491,V1,162345,40.7128,N,74.0060,W,55.5,123.45,020725"
            ),
            Protocol = ProtocolType.H02,
            ReceivedAt = DateTime.UtcNow
        };

        _tk103Frame = new GpsFrame
        {
            RawData = System.Text.Encoding.ASCII.GetBytes(
                "358240050447491,20250702162345,4071.2800,N,07400.6000,W,055.5,123.45"
            ),
            Protocol = ProtocolType.TK103,
            ReceivedAt = DateTime.UtcNow
        };

        // Register test device
        _testDevice = new Device
        {
            Imei = "358240050447491",
            DeviceName = "Benchmark Test Device",
            Protocol = ProtocolType.GT06,
            IsActive = true
        };
        _deviceService.RegisterDeviceAsync(_testDevice).GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _provider?.Dispose();
    }

    /// <summary>
    /// Benchmarks GT06 protocol frame parsing performance.
    /// GT06 is a binary protocol used by many GPS trackers.
    /// </summary>
    [BenchmarkCategory("Parsing")]
    [Benchmark(Baseline = true)]
    public async Task Parse_GT06_Frame()
    {
        await _parserService.ParseFrameAsync(_gt06Frame).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks H02 protocol frame parsing performance.
    /// H02 is an ASCII-based protocol (NMEA-like format).
    /// </summary>
    [BenchmarkCategory("Parsing")]
    [Benchmark]
    public async Task Parse_H02_Frame()
    {
        await _parserService.ParseFrameAsync(_h02Frame).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks TK103 protocol frame parsing performance.
    /// TK103 is a binary protocol with fixed-size frames.
    /// </summary>
    [BenchmarkCategory("Parsing")]
    [Benchmark]
    public async Task Parse_TK103_Frame()
    {
        await _parserService.ParseFrameAsync(_tk103Frame).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks protocol auto-detection performance.
    /// Critical for high-throughput data ingestion pipelines.
    /// </summary>
    [BenchmarkCategory("Detection")]
    [Benchmark]
    public async Task Detect_Protocol()
    {
        await _parserService.DetectProtocolAsync(_gt06Frame.RawData).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks frame validation performance.
    /// Validation is performed on every incoming frame.
    /// </summary>
    [BenchmarkCategory("Validation")]
    [Benchmark]
    public async Task Validate_GT06_Frame()
    {
        await _parserService.ValidateFrameAsync(_gt06Frame).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks location data storage performance.
    /// Measures throughput of storing parsed location data.
    /// </summary>
    [BenchmarkCategory("Storage")]
    [Benchmark]
    public async Task Store_Location_Data()
    {
        var location = new LocationData
        {
            DeviceId = _testDevice.Id,
            Latitude = 40.7128,
            Longitude = -74.0060,
            Speed = 55.5,
            Bearing = 123.45,
            Altitude = 15.0,
            SatelliteCount = 12,
            Accuracy = 5.0,
            Timestamp = DateTime.UtcNow,
            Protocol = ProtocolType.GT06
        };

        await _locationService.StoreLocationAsync(location).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks location data retrieval performance.
    /// Measures latency of querying latest location.
    /// </summary>
    [BenchmarkCategory("Query")]
    [Benchmark]
    public async Task Get_Latest_Location()
    {
        await _locationService.GetLatestLocationAsync(_testDevice.Id).ConfigureAwait(false);
    }

    /// <summary>
    /// Benchmarks batch parsing scenario - parsing multiple frames in sequence.
    /// Simulates real-world high-throughput data ingestion.
    /// </summary>
    [BenchmarkCategory("Throughput")]
    [Benchmark]
    public async Task Batch_Parse_100_Frames()
    {
        for (int i = 0; i < 100; i++)
        {
            await _parserService.ParseFrameAsync(_gt06Frame).ConfigureAwait(false);
        }
    }
}