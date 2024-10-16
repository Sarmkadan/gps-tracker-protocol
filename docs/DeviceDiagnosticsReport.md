# DeviceDiagnosticsReport

Represents a snapshot of diagnostic information for a GPS tracking device, aggregating communication status, location data, power levels, and self‑test results. It is intended for consumption by monitoring services or UI components that need to display the current health and activity of a device.

## API

### DeviceId
- **Purpose**: Unique identifier assigned to the device, typically a UUID or serial number.
- **Type**: `string`
- **Remarks**: Read‑only after initialization; never null or empty for a valid report.
- **Exceptions**: None during normal get/set; assigning `null` or empty string may cause logical errors in dependent code.

### DeviceName
- **Purpose**: Human‑readable name or label for the device (e.g., “Truck‑01”).
- **Type**: `string`
- **Remarks**: Can be empty if no friendly name is configured.
- **Exceptions**: None.

### Imei
- **Purpose**: International Mobile Equipment Identity of the device's cellular modem.
- **Type**: `string`
- **Remarks**: May be null for devices without cellular capability.
- **Exceptions**: None.

### Protocol
- **Purpose**: Communication protocol used by the device (e.g., MQTT, UDP).
- **Type**: `ProtocolType` (enum)
- **Remarks**: Reflects the active protocol at the time the report was generated.
- **Exceptions**: None.

### Status
- **Purpose**: High‑level operational state of the device.
- **Type**: `DeviceStatus` (enum)
- **Remarks**: Values such as `Online`, `Offline`, `Error`, `Maintenance`.
- **Exceptions**: None.

### IsOnline
- **Purpose**: Convenience flag indicating whether the device is currently reachable.
- **Type**: `bool`
- **Remarks**: Derived from `Status` and `LastSeen`; true when the device has communicated within the expected timeout.
- **Exceptions**: None.

### LastSeen
- **Purpose**: Timestamp of the most recent successful communication from the device.
- **Type**: `DateTime` (UTC)
- **Remarks**: If the device has never been heard from, may be set to `DateTime.MinValue`.
- **Exceptions**: None.

### TimeSinceLastContact
- **Purpose**: Elapsed time since `LastSeen`.
- **Type**: `TimeSpan`
- **Remarks**: Computed as `DateTime.UtcNow - LastSeen`; updates automatically each time the report is inspected.
- **Exceptions**: None.

### TotalPacketsReceived
- **Purpose**: Cumulative count of all packets successfully received from the device since deployment.
- **Type**: `int`
- **Remarks**: Monotonically increasing; wraps only if the underlying storage overflows (unlikely in practice).
- **Exceptions**: None.

### IpAddress
- **Purpose**: Last known IP address used by the device for communication.
- **Type**: `string?`
- **Remarks**: Null when the device is offline or when the address cannot be determined.
- **Exceptions**: None.

### BatteryLevel
- **Purpose**: Current battery charge percentage.
- **Type**: `int`
- **Remarks**: Expected range 0–100; values outside this range indicate a sensor fault.
- **Exceptions**: None.

### SignalStrength
- **Purpose**: Relative strength of the cellular or radio signal.
- **Type**: `int`
- **Remarks**: Typically 0–5 (or 0–100) depending on hardware; higher values indicate better reception.
- **Exceptions**: None.

### SignalQuality
- **Purpose**: Qualitative description of the link quality.
- **Type**: `string`
- **Remarks**: Free‑form text such as “Excellent”, “Good”, “Fair”, “Poor”. May be empty if unknown.
- **Exceptions**: None.

### TotalLocationPoints
- **Purpose**: Number of distinct GPS fixes recorded and stored.
- **Type**: `int`
- **Remarks**: Increments with each valid location update; useful for assessing data completeness.
- **Exceptions**: None.

### LastLocation
- **Purpose**: Most recent geographic fix reported by the device.
- **Type**: `LocationData?`
- **Remarks**: Null when no location fix has ever been obtained.
- **Exceptions**: None.

### TotalDistanceKm
- **Purpose**: Cumulative distance traveled, calculated from successive location points.
- **Type**: `double`
- **Remarks**: Expressed in kilometers; may contain fractional precision.
- **Exceptions**: None.

### TotalJourneys
- **Purpose**: Number of completed trips (journey start → end) detected.
- **Type**: `int`
- **Remarks**: A journey is defined by a period of movement followed by a stationary interval exceeding a configured threshold.
- **Exceptions**: None.

### ActiveJourneys
- **Purpose**: Number of journeys currently in progress.
- **Type**: `int`
- **Remarks**: Typically 0 or 1 for a single‑device tracker; >1 only if the device reports multiple concurrent tracks.
- **Exceptions**: None.

### SelfTest
- **Purpose**: Results of the device's built‑in diagnostic self‑test, if available.
- **Type**: `DeviceSelfTestResult?`
- **Remarks**: Null when the device does not support self‑test or the test has not been run.
- **Exceptions**: None.

### GeneratedAt
- **Purpose**: Timestamp indicating when this `DeviceDiagnosticsReport` instance was populated.
- **Type**: `DateTime` (UTC)
- **Remarks**: Useful for assessing the freshness of the report relative to `LastSeen`.
- **Exceptions**: None.

## Usage

### Example 1: Creating a report from received telemetry
```csharp
var report = new DeviceDiagnosticsReport
{
    DeviceId        = "abc-123-def",
    DeviceName      = "Field Unit 07",
    Imei            = "860123040567890",
    Protocol        = ProtocolType.Mqtt,
    Status          = DeviceStatus.Online,
    IsOnline        = true,
    LastSeen        = DateTime.UtcNow.AddMinutes(-2),
    TimeSinceLastContact = TimeSpan.FromMinutes(2),
    TotalPacketsReceived = 12458,
    IpAddress       = "10.0.5.23",
    BatteryLevel =",
    BatteryLevel    = 87,
    SignalStrength  = 4,
    SignalQuality   = "Good",
    TotalLocationPoints = 342,
    LastLocation    = new LocationData { Latitude = 40.7128, Longitude = -74.0060, AccuracyMeters = 5 },
    TotalDistanceKm = 1245.6,
    TotalJourneys   = 23,
    ActiveJourneys  = 1,
    SelfTest        = new DeviceSelfTestResult { Passed = true, Details = "All subsystems nominal" },
    GeneratedAt     = DateTime.UtcNow
};

// Simple health check
if (report.IsOnline && report.BatteryLevel > 20 && report.SignalStrength >= 3)
{
    Console.WriteLine($"Device {report.DeviceName} is healthy.");
}
```

### Example 2: Consuming a report for UI display
```csharp
// Assume `latestReport` is obtained from a monitoring service
var latestReport = telemetryService.GetLatestDiagnostics(deviceId);

var statusText = latestReport.IsOnline
    ? "Online"
    : $"Offline (last seen {latestReport.TimeSinceLastContact.Hours}h {latestReport.TimeSinceLastContact.Minutes}m ago)";

var locationInfo = latestReport.LastLocation.HasValue
    ? $"{latestReport.LastLocation.Value.Latitude:F5}, {latestReport.LastLocation.Value.Longitude:F5}"
    : "No fix";

Console.WriteLine($"{latestReport.DeviceName} ({latestReport.DeviceId})");
Console.WriteLine($"Status: {statusText}");
Console.WriteLine($"Battery: {latestReport.BatteryLevel}%");
Console.WriteLine($"Signal: {latestReport.SignalQuality} ({latestReport.SignalStrength})");
Console.WriteLine($"Location: {locationInfo}");
Console.WriteLine($"Distance traveled: {latestReport.TotalDistanceKm:F1} km");
```

## Notes
- **Nullable members**: `IpAddress`, `LastLocation`, and `SelfTest` can be `null`. Consumers must check for null before accessing properties to avoid `NullReferenceException`.
- **Derived values**: `TimeSinceLastContact` is computed from `LastSeen` and the current system time; if the system clock is adjusted backward, the value may temporarily become negative. Applications should treat negative spans as zero.
- **Range expectations**: While the type does not enforce ranges, values such as `BatteryLevel` (0‑100) and `SignalStrength` (hardware‑specific) are expected to stay within sensible bounds; out‑of‑range values indicate a malfunctioning sensor or corrupted data.
- **Thread safety**: The class contains only mutable fields/properties with no internal locking. Instances are **not** thread‑safe; concurrent reads and writes from multiple threads require external synchronization (e.g., locking or using immutable snapshots).
- **Immutability considerations**: For scenarios where the report is published to multiple consumers, consider copying the instance or using an immutable wrapper after population to prevent accidental mutation.
- **Serialization**: All members are public and have straightforward types, making the type suitable for JSON/XML serialization without custom converters, provided nullable members are handled according to the serializer’s null‑handling settings.
