## PerformanceBenchmark

The `PerformanceBenchmark` class provides comprehensive benchmarking and performance testing capabilities for GPS tracker protocol parsing and storage operations. It includes methods for setting up test environments, parsing various protocol frames (GT06, H02, TK103), protocol detection, frame validation, location data collection, and batch processing.

Example usage for performance benchmarking:
```csharp
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

public class PerformanceBenchmarkExample
{
    public async Task RunBenchmarkAsync()
    {
        var benchmark = new PerformanceBenchmark();
        await benchmark.BenchmarkFrameValidationAsync(10000);
        await benchmark.BenchmarkLocationStorageAsync(10000);
        await benchmark.BenchmarkLocationQueryAsync(5000, 500);
        await benchmark.RunStressTestAsync(50, 100);
    }
}
```
