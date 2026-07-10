# ILocationDataService

`ILocationDataService` defines the contract for storing, retrieving, and analyzing location data in the GPS tracker protocol system. It provides asynchronous operations for persisting location readings, querying historical or time-bound data, and computing derived metrics such as travel distance and data cleanup.

## API

### `LocationDataService`

The concrete implementation of `ILocationDataService` that handles storage and retrieval of location data using the configured data access layer.

### `Task<LocationData> StoreLocationAsync(LocationData location)`

Stores a new location record in the system.

- **Parameters**: `location` – The `LocationData` instance to store, containing timestamp, coordinates, and optional metadata.
- **Return value**: A `Task<LocationData>` resolving to the stored `LocationData` after persistence.
- **Exceptions**: Throws `ArgumentNullException` if `location` is `null`. May throw `InvalidOperationException` if the storage backend is unavailable or the location data is malformed.

### `Task<LocationData?> GetLatestLocationAsync()`

Retrieves the most recent location record stored in the system.

- **Return value**: A `Task<LocationData?>` resolving to the latest `LocationData` if available, or `null` if no records exist.
- **Exceptions**: May throw `InvalidOperationException` if the storage backend is unreachable or returns inconsistent data.

### `Task<IEnumerable<LocationData>> GetLocationHistoryAsync()`

Retrieves the complete history of stored location data, ordered chronologically.

- **Return value**: A `Task<IEnumerable<LocationData>>` containing all stored location records, ordered from oldest to newest.
- **Exceptions**: May throw `InvalidOperationException` if the storage backend fails during retrieval.

### `Task<IEnumerable<LocationData>> GetLocationsByTimeRangeAsync(DateTime start, DateTime end)`

Retrieves all location records within a specified time range.

- **Parameters**:
  - `start` – The inclusive start of the time range.
  - `end` – The inclusive end of the time range.
- **Return value**: A `Task<IEnumerable<LocationData>>` containing all matching location records, ordered chronologically.
- **Exceptions**: Throws `ArgumentException` if `start` is after `end`. May throw `InvalidOperationException` if the storage backend is unavailable.

### `Task<IEnumerable<LocationData>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusKm)`

Retrieves location records within a geographic radius around a given coordinate.

- **Parameters**:
  - `latitude` – The reference latitude in decimal degrees.
  - `longitude` – The reference longitude in decimal degrees.
  - `radiusKm` – The search radius in kilometers.
- **Return value**: A `Task<IEnumerable<LocationData>>` containing all locations within the specified radius, ordered by distance ascending.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `radiusKm` is negative. May throw `InvalidOperationException` if the geospatial index is unavailable.

### `Task<double> CalculateTravelDistanceAsync(Guid deviceId, DateTime start, DateTime end)`

Calculates the total travel distance for a device between two timestamps using its stored location history.

- **Parameters**:
  - `deviceId` – The unique identifier of the device.
  - `start` – The inclusive start time.
  - `end` – The inclusive end time.
- **Return value**: A `Task<double>` resolving to the total distance traveled in kilometers.
- **Exceptions**: Throws `ArgumentException` if `start` is after `end`. May throw `InvalidOperationException` if no locations exist for the device or the storage backend fails.

### `Task<int> CleanupOldDataAsync(TimeSpan retentionPeriod)`

Removes location records older than a specified retention period.

- **Parameters**: `retentionPeriod` – The minimum age of records to retain; older records are deleted.
- **Return value**: A `Task<int>` resolving to the number of records deleted.
- **Exceptions**: May throw `InvalidOperationException` if the cleanup operation cannot be completed due to backend errors.

## Usage
