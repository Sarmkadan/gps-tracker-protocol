# ReplayOptions

The `ReplayOptions` type encapsulates the configuration and runtime state of a GPS track replay session. It defines parameters that control the replay speed, the portion of the original track to replay, and optional time rebasing, while also exposing the generated replay frames and computed metrics such as elapsed time, cumulative distance, and duration.

## API

- **`SpeedMultiplier`** (`double`)  
  Gets or sets the speed multiplier applied during replay. A value of `1.0` replays at the original speed. Setting this to a value less than or equal to zero may throw an `ArgumentOutOfRangeException`.

- **`StartIndex`** (`int`)  
  Gets or sets the zero-based index of the first frame in the original track to include in the replay. Must be less than or equal to `EndIndex` and within the bounds of the source data; otherwise an `ArgumentOutOfRangeException` may be thrown.

- **`EndIndex`** (`int`)  
  Gets or sets the zero-based index of the last frame in the original track to include in the replay. Must be greater than or equal to `StartIndex` and within bounds; otherwise an `ArgumentOutOfRangeException` may be thrown.

- **`RebaseToUtc`** (`DateTime?`)  
  Gets or sets an optional UTC `DateTime` to which all replay timestamps are rebased. When set, the original timestamps are shifted so that the first replayed frame aligns with this value. If the value is not in UTC, an `ArgumentException` may be thrown.

- **`Index`** (`int`)  
  Gets the current zero-based index within the `Frames` list during replay. This value is updated as the replay progresses.

- **`Location`** (`LocationData`)  
  Gets the current geographic location (latitude, longitude, altitude, etc.) corresponding to the current replay frame.

- **`ReplayTimestamp`** (`DateTime`)  
  Gets the current timestamp of the replay, adjusted for speed multiplier and optional rebasing.

- **`ElapsedReplay`** (`TimeSpan`)  
  Gets the total elapsed time of the replay so far, computed from the start of the replay session.

- **`CumulativeDistanceKm`** (`double`)  
  Gets the cumulative distance traveled in kilometers up to the current replay frame.

- **`JourneyId`** (`string`)  
  Gets or sets the identifier for the journey or trip being replayed.

- **`DeviceId`** (`string`)  
  Gets or sets the identifier of the device that recorded the original track.

- **`Options`** (`ReplayOptions`)  
  Gets the options instance that was used to configure this replay. This property may refer to the same object or a copy, depending on the implementation.

- **`Frames`** (`IReadOnlyList<ReplayFrame>`)  
  Gets the read-only list of `ReplayFrame` objects generated for the replay. Each frame contains location, timestamp, and other derived data.

- **`TotalDistanceKm`** (`double`)  
  Gets the total distance of the replayed portion in kilometers, computed from the original track data.

- **`OriginalDuration`** (`TimeSpan`)  
  Gets the original duration of the replayed portion (from `StartIndex` to `EndIndex`) without speed adjustment.

- **`ReplayDuration`** (`TimeSpan`)  
  Gets the duration of the replay after applying the speed multiplier.

- **`GeneratedAt`** (`DateTime`)  
  Gets the UTC timestamp when the replay frames were generated.

## Usage

### Example 1: Configuring and iterating over a replay

```csharp
var options = new ReplayOptions
{
    SpeedMultiplier = 2.0,
    StartIndex = 0,
    EndIndex = 500,
    RebaseToUtc = DateTime.UtcNow,
    JourneyId = "trip-2024-01-15",
    DeviceId = "gps-001"
};

// Assume the replay is generated and frames are populated.
foreach (var frame in options.Frames)
{
    Console.WriteLine($"Frame {options.Index}: {options.ReplayTimestamp} " +
                      $"at ({options.Location.Latitude}, {options.Location.Longitude}) " +
                      $"Distance: {options.CumulativeDistanceKm:F2} km");
}
```

### Example 2: Using rebasing to align timestamps

```csharp
var options = new ReplayOptions
{
    StartIndex = 10,
    EndIndex = 200,
    RebaseToUtc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc),
    SpeedMultiplier = 1.0
};

// After generation, the first frame's ReplayTimestamp will be 2024-06-01 12:00:00 UTC.
var firstTimestamp = options.Frames[0].Timestamp; // original timestamp
var rebasedTimestamp = options.Frames[0].ReplayTimestamp; // rebased to 12:00 UTC
```

## Notes

- **Edge cases:**  
  - Setting `SpeedMultiplier` to zero or a negative value will cause an exception.  
  - If `StartIndex` is greater than `EndIndex`, the replay will have no frames and `Frames` will be empty.  
  - Providing a `RebaseToUtc` value with `DateTimeKind.Local` or `Unspecified` may throw an `ArgumentException`; always use `DateTimeKind.Utc`.  
  - When `Frames` is empty, properties such as `Location`, `ReplayTimestamp`, and `CumulativeDistanceKm` may return default values or throw an `InvalidOperationException` depending on the implementation.

- **Thread safety:**  
  Instances of `ReplayOptions` are not thread-safe. Concurrent reads and writes to mutable properties (`SpeedMultiplier`, `StartIndex`, `EndIndex`, `RebaseToUtc`, `JourneyId`, `DeviceId`) from multiple threads can lead to inconsistent state. Read-only properties (`Frames`, `Location`, `ReplayTimestamp`, etc.) may be safely read after the replay is fully generated, but the class provides no synchronization guarantees. External locking is required if the same instance is accessed from multiple threads.
