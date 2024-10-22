# JourneyAnalyzer

The `JourneyAnalyzer` type provides asynchronous utilities for processing GPS telemetry data, simulating journeys, and generating fleet‑level reports within the GPS‑Tracker‑Protocol application. It is intended to be instantiated once per processing context and used to orchestrate analysis workflows without blocking the calling thread.

## API

### JourneyAnalyzer()
Initializes a new instance of the `JourneyAnalyzer`. The constructor does not take any parameters and prepares internal state required for subsequent analysis operations. No exceptions are thrown under normal conditions.

### AnalyzeDeviceAsync(string deviceId, DateTime? startTime = null, DateTime? endTime = null, CancellationToken cancellationToken = default)
Analyzes telemetry data for a single device within an optional time window.

- **Purpose** – Retrieves and processes GPS points for the specified device, computing metrics such as distance traveled, average speed, and stop duration.
- **Parameters**  
  - `deviceId`: Identifier of the device to analyze; must not be `null` or whitespace.  
  - `startTime`: Inclusive lower bound of the analysis period; if `null` the earliest available data is used.  
  - `endTime`: Exclusive upper bound of the analysis period; if `null` the latest available data is used.  
  - `cancellationToken`: Token to observe for cancellation requests.
- **Return Value** – A `Task` that completes when the analysis finishes. The method does not return a value; results are typically stored internally or raised via events (not part of the public surface described here).
- **Exceptions**  
  - `ArgumentNullException` or `ArgumentException` if `deviceId` is invalid.  
  - `InvalidOperationException` if no telemetry data exists for the given device and time range.  
  - `OperationCanceledException` if the operation is cancelled via `cancellationToken`.

### SimulateJourneyAsync(JourneyDefinition definition, SpeedProfile profile, CancellationToken cancellationToken = default)
Generates a synthetic journey based on a route definition and a speed profile.

- **Purpose** – Produces a sequence of simulated GPS points that follow the supplied route, adjusting timestamps according to the speed profile, useful for testing or demonstration.
- **Parameters**  
  - `definition`: Object describing the waypoints, roads, and constraints of the journey; must not be `null`.  
  - `profile`: Object specifying how speed varies over time or distance; must not be `null`.  
  - `cancellationToken`: Token to observe for cancellation requests.
- **Return Value** – A `Task` that completes when the simulation finishes. The simulated data is made available through internal storage or callbacks.
- **Exceptions**  
  - `ArgumentNullException` if `definition` or `profile` is `null`.  
  - `InvalidOperationException` if the definition contains invalid geometry (e.g., discontinuous waypoints).  
  - `OperationCanceledException` if cancelled.

### GenerateFleetReportAsync(string fleetId, ReportPeriod period, CancellationToken cancellationToken = default)
Creates an aggregated report for all devices belonging to a fleet over a specified period.

- **Purpose** – Consolidates individual device analyses into fleet‑wide statistics such as total distance, fuel consumption estimates, and compliance metrics.
- **Parameters**  
  - `fleetId`: Identifier of the fleet to report on; must not be `null` or whitespace.  
  - `period`: Object defining the start and end dates for the report; must not be `null`.  
  - `cancellationToken`: Token to observe for cancellation requests.
- **Return Value** – A `Task` that completes when the report generation finishes. The report can be accessed via a property or event after the task completes (details omitted as they are not part of the listed members).
- **Exceptions**  
  - `ArgumentNullException` or `ArgumentException` if `fleetId` is invalid.  
  - `ArgumentNullException` if `period` is `null`.  
  - `InvalidOperationException` if the fleet has no associated devices or no data for the period.  
  - `OperationCanceledException` if cancelled.

### Main(string[] args)
Entry point for the console application that drives the `JourneyAnalyzer`.

- **Purpose** – Parses command‑line arguments, configures the analyzer, and invokes the desired operation (analysis, simulation, or reporting) based on user input.
- **Parameters**  
  - `args`: Command‑line arguments supplied to the executable; content is application‑specific.
- **Return Value** – A `Task` representing the asynchronous execution of the application. The method returns when all requested work has completed.
- **Exceptions**  
  - May propagate exceptions from the invoked analyzer methods (e.g., `ArgumentException`, `InvalidOperationException`).  
  - `OperationCanceledException` if the host terminates the operation early.

## Usage

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using GpsTrackerProtocol; // namespace containing JourneyAnalyzer

class Program
{
    static async Task Main(string[] args)
    {
        var analyzer = new JourneyAnalyzer();

        // Example: analyze a single device for the last 24 hours
        var deviceId = "ABC123";
        var start = DateTime.UtcNow.AddDays(-1);
        var end   = DateTime.UtcNow;

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            await analyzer.AnalyzeDeviceAsync(deviceId, start, end, cts.Token);
            Console.WriteLine($"Analysis for device {deviceId} completed.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Analysis was cancelled.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error during analysis: {ex.Message}");
        }
    }
}
```

```csharp
using System;
using System.Threading.Tasks;
using GpsTrackerProtocol;

class Demo
{
    static async Task RunSimulationAndReport()
    {
        var analyzer = new JourneyAnalyzer();

        // Assume JourneyDefinition and SpeedProfile are defined elsewhere
        var definition = new JourneyDefinition(/* waypoints */);
        var profile    = new SpeedProfile(/* speed limits */);

        // Simulate a journey
        await analyzer.SimulateJourneyAsync(definition, profile);

        // Generate a fleet report for the simulated data
        var fleetId = "FleetAlpha";
        var period  = new ReportPeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1));

        await analyzer.GenerateFleetReportAsync(fleetId, period);
        Console.WriteLine("Fleet report generated.");
    }
}
```

## Notes

- **Argument validation** – All public async methods validate their reference‑type arguments and throw `ArgumentNullException` or `ArgumentException` when invalid input is detected. Callers should ensure non‑null, non‑empty strings and non‑null complex objects before invocation.
- **Cancellation** – Each method accepts a `CancellationToken`. If cancellation is requested, the method will throw `OperationCanceledException`. It is safe to call the same instance from multiple threads concurrently, but shared cancellation sources will affect all ongoing operations.
- **Thread‑safety** – The class does not protect internal state with locks. Consequently, invoking multiple analysis or simulation methods on the same instance from different threads may lead to race conditions if those methods mutate shared buffers or caches. For concurrent independent work, either create separate `JourneyAnalyzer` instances per thread or synchronize access externally.
- **Static `Main`** – The `Main` method is safe to call from any thread as it merely orchestrates the analyzer; however, it is intended to be used as the application entry point and should not be invoked repeatedly within a running program.
- **Error propagation** – Exceptions thrown by the async methods are not wrapped; they propagate directly through the returned `Task`. Callers should `await` the task within a `try/catch` block or inspect `Task.Exception` when using `Task.Wait`/`Result`.
- **State after completion** – After a method completes successfully, any generated data (e.g., analysis results, simulated points, fleet reports) remains accessible via the instance until the instance is disposed or overwritten by a subsequent call. No automatic clearing occurs.
