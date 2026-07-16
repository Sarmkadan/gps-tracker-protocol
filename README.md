

## ProtocolBenchmarks

The `ProtocolBenchmarks` class provides comprehensive benchmarking and performance testing capabilities for GPS tracker protocol parsing and processing. It includes methods for setting up test environments, parsing various protocol frames (GT06, H02, TK103), protocol detection, frame validation, location data storage and retrieval, batch processing, and generating analytics reports for device and fleet performance.

This class is essential for performance testing and optimization of protocol handling code, allowing developers to measure parsing speed, validate correctness, and identify bottlenecks in GPS tracker data processing pipelines.

Example usage for protocol benchmarking:

```csharp
using GpsTrackerProtocol.Benchmarks;
using GpsTrackerProtocol.Domain.Models;

public class ProtocolBenchmarksExample
{
    private readonly ProtocolBenchmarks _benchmarks;

    public ProtocolBenchmarksExample(ProtocolBenchmarks benchmarks)
    {
        _benchmarks = benchmarks;
    }

    public async Task RunProtocolBenchmarksAsync()
    {
        // Setup benchmark environment
        _benchmarks.Setup();

        // Register a test device
        await _benchmarks.Register_Device("test-device-001", "GT06", "123456789012345");

        // Parse individual protocol frames
        var gt06Frame = new byte[] { 0x78, 0x78, 0x0D, 0x01, 0x02, 0x03, 0x04 };
        await _benchmarks.Parse_GT06_Frame(gt06Frame);

        var h02Frame = new byte[] { 0x79, 0x79, 0x0D, 0xA0, 0x01, 0x02, 0x03 };
        await _benchmarks.Parse_H02_Frame(h02Frame);

        var tk103Frame = new byte[] { 0x24, 0x47, 0x50, 0x52, 0x4D, 0x43, 0x2C };
        await _benchmarks.Parse_TK103_Frame(tk103Frame);

        // Detect protocol automatically
        var protocolType = await _benchmarks.Detect_Protocol(gt06Frame);
        Console.WriteLine($"Detected protocol: {protocolType}");

        // Validate frames
        var isValid = await _benchmarks.Validate_GT06_Frame(gt06Frame);
        Console.WriteLine($"Frame validation result: {isValid}");

        // Store location data
        var locationData = new LocationData
        {
            DeviceId = "test-device-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 45.5,
            Bearing = 90,
            Timestamp = DateTime.UtcNow
        };
        await _benchmarks.Store_Location_Data(locationData);

        // Get latest location
        var latestLocation = await _benchmarks.Get_Latest_Location("test-device-001");
        Console.WriteLine($"Latest location: {latestLocation?.Latitude}, {latestLocation?.Longitude}");

        // Batch processing - parse 100 frames
        var batchFrames = Enumerable.Range(0, 100)
            .Select(i => new byte[] { 0x78, 0x78, 0x0D, (byte)(0x01 + i), (byte)(0x02 + i), (byte)(0x03 + i), (byte)(0x04 + i) })
            .ToList();
        await _benchmarks.Batch_Parse_100_Frames(batchFrames);

        // Generate analytics reports
        var deviceAnalytics = await _benchmarks.Generate_Device_Analytics("test-device-001");
        Console.WriteLine($"Device analytics - Total messages: {deviceAnalytics.TotalMessages}, Avg speed: {deviceAnalytics.AverageSpeed:F2} km/h");

        var fleetAnalytics = await _benchmarks.Generate_Fleet_Analytics(new[] { "test-device-001" });
        Console.WriteLine($"Fleet analytics - Active devices: {fleetAnalytics.ActiveDeviceCount}");

        // Cleanup after benchmark
        _benchmarks.Cleanup();
    }
}
```