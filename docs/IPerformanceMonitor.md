# IPerformanceMonitor

Provides a contract for measuring the duration of named operations, collecting metrics, and reporting performance data. Implementations typically accumulate timing information for each operation and expose it through read‑only properties or methods for retrieval and display.

## API

| Member | Description | Parameters | Return Value | Exceptions |
|--------|-------------|------------|--------------|------------|
| `PerformanceMonitor` | Gets the underlying monitor instance associated with this interface. | None | An object of type `PerformanceMonitor` that provides concrete measurement functionality. | None specified. |
| `IDisposable MeasureOperation()` | Begins a measurement scope for an operation. The returned `IDisposable` should be disposed to end the measurement. | None | An `IDisposable` representing the active measurement. Disposing stops the timer and records the elapsed time. | May throw `ObjectDisposedException` if the monitor has been disposed. |
| `void RecordOperation()` | Records a completed operation without using a scoped measurement. Intended for manual timing where the caller supplies the duration elsewhere. | None | None. | May throw `InvalidOperationException` if called outside of a valid measurement context. |
| `OperationMetrics GetMetrics()` | Retrieves a snapshot of all collected metrics for the monitored operation. | None | An `OperationMetrics` instance containing counts, totals, averages, minima, maxima, and medians. | None specified. |
| `void PrintReport()` | Writes a formatted performance report to the standard output or a configured logger. | None | None. | May throw `IOException` if the underlying output stream fails. |
| `OperationTimer` | Gets a timer object that can be used to manually start and stop interval measurements. | None | An `OperationTimer` providing `Start()` and `Stop()` methods. | None specified. |
| `void Dispose()` | Releases any resources held by the monitor (e.g., file handles, timers). | None | None. | None specified. |
| `string OperationName` | Gets the name of the operation being monitored. | None | The operation name as a string. | None specified. |
| `int Count` | Gets the total number of recorded operations. | None | The count of operations. | None specified. |
| `TimeSpan TotalDuration` | Gets the cumulative duration of all recorded operations. | None | The sum of all operation durations. | None specified. |
| `TimeSpan AverageDuration` | Gets the average duration of a single operation. | None | The mean duration; returns `TimeSpan.Zero` if `Count` is zero. | None specified. |
| `TimeSpan MinDuration` | Gets the shortest recorded operation duration. | None | The minimum duration; returns `TimeSpan.Zero` if no operations have been recorded. | None specified. |
| `TimeSpan MaxDuration` | Gets the longest recorded operation duration. | None | The maximum duration; returns `TimeSpan.Zero` if no operations have been recorded. | None specified. |
| `TimeSpan MedianDuration` | Gets the median duration of all recorded operations. | None | The middle value when durations are sorted; returns `TimeSpan.Zero` if `Count` is zero. | None specified. |

## Usage

### Example 1: Scoped measurement with `MeasureOperation`

```csharp
using (var scope = monitor.MeasureOperation())
{
    // Simulate work
    System.Threading.Tasks.Task.Delay(120).Wait();
}
// After disposal, the operation duration is recorded automatically.
```

### Example 2: Manual recording and reporting

```csharp
// Start manual timer
var timer = monitor.OperationTimer;
timer.Start();
// Perform work
DoWork();
timer.Stop();

// Record the elapsed time manually
monitor.RecordOperation(); // assumes elapsed time was captured elsewhere

// Retrieve and display metrics
var metrics = monitor.GetMetrics();
Console.WriteLine($"Operations: {monitor.Count}");
Console.WriteLine($"Average: {monitor.AverageDuration}");
monitor.PrintReport();
```

## Notes

- The interface does not guarantee thread‑safety; concurrent calls from multiple threads may result in corrupted metrics unless the implementation provides its own synchronization.
- `MeasureOperation` returns an `IDisposable` that must be disposed; failing to do so will leave the operation unrecorded.
- `RecordOperation` is intended for scenarios where timing is performed outside the monitor; calling it without a preceding measurement will record a zero‑duration interval.
- All metric properties (`Count`, `TotalDuration`, etc.) reflect the state at the moment of access and are not snapshotted; rapid updates may lead to inconsistent reads if not synchronized.
- Invoking any member after `Dispose` has been called may throw an `ObjectDisposedException` depending on the implementation.
