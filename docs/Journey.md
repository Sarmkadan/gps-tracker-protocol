# Journey

Represents a tracked journey recorded by a GPS device, including start/end times, waypoints, and computed metrics such as distance and speed.

## API

### `public string Id`
A unique identifier for the journey. Must not be null or empty.

### `public string DeviceId`
Identifier of the device that recorded the journey. Must not be null or empty.

### `public DateTime StartTime`
Timestamp when the journey began. Must be a valid DateTime in the past.

### `public DateTime? EndTime`
Timestamp when the journey ended, if completed. Must not be in the future relative to `StartTime`.

### `public List<LocationData> Waypoints`
Ordered list of geographic points recorded during the journey. May be empty but must not be null.

### `public int Status`
Numeric status code indicating the journey state (e.g., 0 = active, 1 = completed). Valid range is implementation-defined.

### `public Dictionary<string, object> Metadata`
Additional key-value pairs associated with the journey. Must not be null; keys must be non-empty strings.

### `public void AddWaypoint(LocationData point)`
Adds a new geographic point to the journey’s waypoint list.
- **Parameters**: `point` – The location data to append.
- **Throws**: `ArgumentNullException` if `point` is null.

### `public double GetTotalDistance()`
Computes the total distance traveled across all waypoints in meters.
- **Returns**: Distance in meters.
- **Throws**: `InvalidOperationException` if fewer than two waypoints exist.

### `public double GetAverageSpeed()`
Calculates the average speed over the journey in meters per second.
- **Returns**: Average speed in m/s.
- **Throws**: `InvalidOperationException` if the journey has no duration or zero distance.

### `public double GetMaxSpeed()`
Determines the highest instantaneous speed recorded during the journey in meters per second.
- **Returns**: Maximum speed in m/s.
- **Throws**: `InvalidOperationException` if no waypoints exist.

### `public TimeSpan GetDuration()`
Returns the elapsed time between `StartTime` and `EndTime` (or current time if incomplete).
- **Returns**: TimeSpan representing the journey’s duration.
- **Throws**: `InvalidOperationException` if `StartTime` is in the future.

### `public void Complete()`
Marks the journey as completed by setting `EndTime` to the current UTC time.
- **Throws**: `InvalidOperationException` if `EndTime` is already set.

### `public override string ToString()`
Generates a human-readable summary of the journey.
- **Returns**: A string containing `Id`, `DeviceId`, `StartTime`, and status.

## Usage
