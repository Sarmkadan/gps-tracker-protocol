# PerformanceBenchmark

The `PerformanceBenchmark` class provides a suite of asynchronous benchmarking methods for measuring the throughput and latency of core operations in the GPS tracker protocol. It is designed to validate frame parsing, location storage, location querying, and overall system stress tolerance. The class is intended for use in development and CI environments to detect performance regressions and to verify that the protocol implementation meets required performance targets.

## API

### `public PerformanceBenchmark()`

Initializes a new instance of the `PerformanceBenchmark` class. The constructor sets up any internal state required for the benchmark runs, such as default configuration values or resource pools.

**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** None.

---

### `public async Task BenchmarkFrameValidationAsync(...)`

Executes a benchmark that measures the time taken to validate a specified number of protocol frames. The method simulates receiving raw frame data, parsing it, and checking its integrity according to the protocol specification.

**Parameters:** The method accepts parameters that control the benchmark scenario (e.g., number of frames, frame size, validation rules). The exact parameter names and types are defined in the source code.  
**Returns:** A `Task` that completes when the benchmark finishes.  
**Throws:**  
- `ArgumentOutOfRangeException` if any numeric parameter is outside the valid range.  
- `InvalidOperationException` if the benchmark cannot be started because the instance is not properly initialized.

---

### `public async Task BenchmarkLocationStorageAsync(...)`

Measures the performance of storing location data points into the underlying storage system. The benchmark inserts a configurable number of location records and records the total elapsed time and per-operation latency.

**Parameters:** The method accepts parameters that control the benchmark scenario (e.g., number of locations, batch size, storage backend). The exact parameter names and types are defined in the source code.  
**Returns:** A `Task` that completes when the benchmark finishes.  
**Throws:**  
- `ArgumentOutOfRangeException` if any numeric parameter is outside the valid range.  
- `InvalidOperationException` if the storage system is not available or misconfigured.

---

### `public async Task BenchmarkLocationQueryAsync(...)`

Measures the performance of querying stored location data by various criteria, such as time range, device ID, or geographic bounding box. The benchmark executes a configurable number of queries and reports latency percentiles.

**Parameters:** The method accepts parameters that control the benchmark scenario (e.g., number of queries, query complexity, data set size). The exact parameter names and types are defined in the source code.  
**Returns:** A `Task` that completes when the benchmark finishes.  
**Throws:**  
- `ArgumentOutOfRangeException` if any numeric parameter is outside the valid range.  
- `InvalidOperationException` if the query engine is not ready or the data set is empty.

---

### `public async Task RunStressTestAsync(...)`

Runs a combined stress test that exercises frame validation, location storage, and location querying concurrently under a simulated load. The test runs for a specified duration and reports overall system throughput and error rates.

**Parameters:** The method accepts parameters that control the stress test (e.g., duration, concurrency level, mix of operations). The exact parameter names and types are defined in the source code.  
**Returns:** A `Task` that completes when the stress test finishes.  
**Throws:**  
- `ArgumentOutOfRangeException` if any numeric parameter is outside the valid range.  
- `TimeoutException` if the test exceeds an internal safety limit.

---

### `public static async Task Main(string[] args)`

The program entry point. When the application is started, this method parses command-line arguments, creates a `PerformanceBenchmark` instance, and runs one or more benchmarks based on the provided arguments. Typical arguments include the benchmark name (e.g., `frame-validation`, `location-storage`, `location-query`, `stress-test`) and scenario-specific parameters.

**Parameters:**  
- `args`: An array of command-line arguments.  
**Returns:** A `Task` that completes when the selected benchmarks have finished.  
**Throws:**  
- `ArgumentException` if the command-line arguments are invalid or missing required values.  
- `InvalidOperationException` if the requested benchmark is not recognized.

---

## Usage

### Example 1: Running a single benchmark from code

```csharp
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var benchmark = new PerformanceBenchmark();

        // Run frame validation benchmark with 10,000 frames of size 512 bytes
        await benchmark.BenchmarkFrameValidationAsync(
            frameCount: 10000,
            frameSize: 512);

        Console.WriteLine("Frame validation benchmark completed.");
    }
}
```

### Example 2: Running a stress test via command line

```bash
# Run stress test for 60 seconds with 8 concurrent workers
dotnet run -- stress-test --duration 60 --concurrency 8
```

The corresponding `Main` method parses these arguments and invokes `RunStressTestAsync` with the specified parameters.

---

## Notes

- **Thread safety:** Instances of `PerformanceBenchmark` are not thread-safe. Do not call multiple benchmark methods concurrently on the same instance. If parallel benchmarks are required, create separate instances for each concurrent run.
- **Resource cleanup:** Each benchmark method may allocate significant resources (e.g., frame buffers, database connections). The class does not implement `IDisposable`; callers should ensure that benchmark instances are not reused after a failure that leaves resources in an inconsistent state.
- **Cancellation:** The benchmark methods do not accept a `CancellationToken` in their current signature. Long-running benchmarks (especially stress tests) may block the calling thread. Consider running them in a separate process or using a timeout mechanism at the application level.
- **Edge cases:**  
  - Passing zero or negative iteration counts may cause `ArgumentOutOfRangeException`.  
  - If the underlying storage or network is unavailable, the methods will throw `InvalidOperationException` or propagate exceptions from the infrastructure layer.  
  - The `Main` method expects at least one argument; running without arguments will throw `ArgumentException`.
- **Performance impact:** Running benchmarks on a production system may degrade performance of other services. It is recommended to execute benchmarks in an isolated environment.
