# LocationAggregationWorker

Represents a snapshot of aggregated GPS data for a single device over a defined time interval. It encapsulates metrics such as distance traveled, speed extremes, and the number of location fixes used in the aggregation.

## API

### LocationAggregationWorker()
**Purpose**  
Initializes a new instance of the `LocationAggregationWorker` class with default values.

**Parameters**  
None.

**Return Value**  
A new `LocationAggregationWorker` object.

**Exceptions**  
None.

### DeviceId
**Purpose**  
Gets or sets the unique identifier of the GPS device whose data is aggregated.

**Type**  
`string`

**Remarks**  
Typically set once after construction; assigning `null` may lead to undefined behavior when the identifier is later used.

### AggregationTime
**Purpose**  
Gets or sets the timestamp that marks the end of the aggregation window.

**Type**  
`DateTime`

**Remarks**  
Represents the moment at which the aggregation was completed.

### LocationCount
**Purpose**  
Gets or sets the number of individual location fixes that contributed to the aggregation.

**Type**  
`int`

**Remarks**  
Expected to be zero or a positive integer; negative values are semantically invalid.

### TimeSpan
**Purpose**  
Gets or sets the duration covered by the aggregated location points.

**Type**  
`TimeSpan`

**Remarks**  
Should be zero or a positive interval; negative intervals have no meaningful interpretation.

### MaxSpeed
**Purpose**  
Gets or sets the maximum speed observed during the aggregation period.

**Type**  
`double` (units: meters per second)

**Remarks**  
Should be greater than or equal to `MinSpeed`; negative values are not physically meaningful.

### MinSpeed
**Purpose**  
Gets or sets the minimum speed observed during the aggregation period.

**Type**  
`double` (units: meters per second)

**Remarks**  
Should be less than or equal to `MaxSpeed`; negative values are not physically meaningful.

### AverageSpeed
**Purpose**  
Gets or sets the arithmetic mean speed across all location fixes.

**Type**  
`double` (units: meters per second)

**Remarks**  
Typically lies between `MinSpeed` and `MaxSpeed`; assigning a value outside this range may indicate inconsistent data.

### TotalDistance
**Purpose**  
Gets or sets the cumulative distance traveled during the aggregation period.

**Type**  
`double` (units: meters)

**Remarks**  
Should be zero or a positive value; negative distances are not meaningful.

## Usage

### Creating an instance and populating properties
```csharp
var worker = new LocationAggregationWorker
{
    DeviceId = "device-1234",
    AggregationTime = DateTime.UtcNow,
    LocationCount = 250,
    TimeSpan = TimeSpan.FromMinutes(30),
    MaxSpeed = 28.5,
    MinSpeed = 0.0,
    AverageSpeed = 12.3,
    TotalDistance = 22140.0
};
```

### Reading aggregated metrics
```csharp
Console.WriteLine($"Device {worker.DeviceId} traveled {worker.TotalDistance:F0} m "
                  + $"in {worker.TimeSpan.TotalMinutes:F1} min "
                  + f"with avg speed {worker.AverageSpeed:F1} m/s.");
```

## Notes
- The class does not perform any validation on property assignments; callers should ensure that values are semantically correct (e.g., non‑negative counts and durations, sensible speed ranges) to avoid misleading results.
- All members are mutable; concurrent modification from multiple threads without external synchronization can lead to race conditions. For thread‑safe scenarios, treat instances as immutable after initialization or protect access with locks.
- Setting `DeviceId` to `null` or leaving it empty may cause downstream code that relies on the identifier to throw `NullReferenceException` or behave incorrectly.
- The `TimeSpan` property represents a duration, not a point in time; assigning a `TimeSpan` with a negative component will not throw but is logically invalid.
