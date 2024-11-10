# KalmanLocationSmootherExtensions

Provides extension methods for applying Kalman filtering and statistical smoothing to sequences of location data. These methods are designed to reduce sensor noise from GPS readings, yielding more stable speed and position estimates without requiring external state management.

## API

### GetSmoothedSpeedKmh

```csharp
public static double? GetSmoothedSpeedKmh(this LocationData current, LocationData previous)
```

Estimates the smoothed instantaneous speed in kilometers per hour between two consecutive location samples using a Kalman-derived correction.

| Parameter   | Type         | Description                                      |
|-------------|--------------|--------------------------------------------------|
| `current`   | `LocationData` | The most recent location sample.               |
| `previous`  | `LocationData` | The immediately preceding location sample.     |

**Returns:** A nullable `double` representing the smoothed speed in km/h. Returns `null` if either parameter is `null`, or if the time delta between the two samples is zero or negative.

**Throws:** No exceptions are thrown by this method; invalid input produces a `null` result.

---

### SmoothBatch

```csharp
public static IEnumerable<LocationData> SmoothBatch(this IEnumerable<LocationData> locations)
```

Applies a forward-pass Kalman smoothing algorithm to an entire ordered sequence of location data points. Each output point has its latitude, longitude, and speed fields adjusted based on the filter's state propagation.

| Parameter   | Type                              | Description                                          |
|-------------|-----------------------------------|------------------------------------------------------|
| `locations` | `IEnumerable<LocationData>`       | A chronologically ordered sequence of raw locations. |

**Returns:** A lazy `IEnumerable<LocationData>` where each element is a smoothed copy of the corresponding input. The enumeration preserves the original order.

**Throws:** `ArgumentNullException` if `locations` is `null`. The enumeration itself does not throw, but deferred execution means exceptions from underlying iterators will surface during traversal.

---

### SmoothOrFallback

```csharp
public static LocationData SmoothOrFallback(this LocationData current, LocationData previous)
```

Produces a smoothed version of `current` by applying a single-step Kalman correction using `previous` as the prior state. If smoothing cannot be performed, the original `current` instance is returned unchanged.

| Parameter   | Type         | Description                                      |
|-------------|--------------|--------------------------------------------------|
| `current`   | `LocationData` | The location to smooth.                        |
| `previous`  | `LocationData` | The preceding location used for prediction.    |

**Returns:** A `LocationData` instance with adjusted coordinates and speed when smoothing succeeds; otherwise the unmodified `current` object. Never returns `null` as long as `current` is non-null.

**Throws:** `ArgumentNullException` if `current` is `null`. A `null` `previous` is treated as unavailable and triggers the fallback path without throwing.

---

### GetAverageSpeedKmh

```csharp
public static double? GetAverageSpeedKmh(this IEnumerable<LocationData> locations)
```

Calculates the arithmetic mean of the smoothed speeds across a batch of location data points, expressed in kilometers per hour.

| Parameter   | Type                              | Description                                      |
|-------------|-----------------------------------|--------------------------------------------------|
| `locations` | `IEnumerable<LocationData>`       | A sequence of location samples.                  |

**Returns:** The average speed in km/h as a nullable `double`. Returns `null` if the sequence is empty or contains no points with valid speed values.

**Throws:** `ArgumentNullException` if `locations` is `null`.

## Usage

### Example 1: Real-time smoothing of incoming GPS fixes

```csharp
LocationData? lastLocation = null;

void OnGpsFixReceived(LocationData rawFix)
{
    if (lastLocation != null)
    {
        LocationData smoothed = rawFix.SmoothOrFallback(lastLocation);
        double? speed = smoothed.GetSmoothedSpeedKmh(lastLocation);
        
        UpdateDisplay(smoothed, speed);
    }
    
    lastLocation = rawFix;
}
```

### Example 2: Post-processing a recorded track

```csharp
IEnumerable<LocationData> rawTrack = LoadTrackFromStorage();

IEnumerable<LocationData> smoothedTrack = rawTrack.SmoothBatch();

double? overallAverage = smoothedTrack.GetAverageSpeedKmh();

foreach (var point in smoothedTrack)
{
    WriteToCsv(point);
}

Console.WriteLine($"Average speed: {overallAverage?.ToString("F1") ?? "N/A"} km/h");
```

## Notes

- **Ordering requirement:** `SmoothBatch` and `GetAverageSpeedKmh` assume the input sequence is already sorted chronologically. Feeding unordered data will produce mathematically valid but physically meaningless results.
- **Null handling:** Methods accepting `LocationData` arguments treat `null` previous points as missing history and either return `null` or fall back to the raw current value. Methods accepting `IEnumerable<LocationData>` throw immediately on a `null` collection reference.
- **Deferred execution:** `SmoothBatch` returns a lazily evaluated sequence. The smoothing computation runs each time the enumerable is iterated; callers should materialize the result with `.ToList()` if multiple passes are needed.
- **Thread safety:** All methods are static and operate solely on their arguments without shared mutable state. They are safe to call concurrently from multiple threads provided the caller-managed `LocationData` instances are not mutated during the call.
- **Edge cases:** Zero or negative time deltas between consecutive points cause speed calculations to return `null`. Empty sequences yield `null` from `GetAverageSpeedKmh`. `SmoothOrFallback` with a `null` current throws, while a `null` previous simply returns the current point unmodified.
