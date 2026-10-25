# LocationDataService

`LocationDataService` is the concrete implementation of `ILocationDataService` (see `Services/LocationDataService.cs`). It stores GPS location readings for tracked devices, answers queries over them (latest fix, history, time range, radius), computes travelled distance over a period, and removes old readings.

It is registered as a singleton through `AddGpsTrackerServices()` in `Configuration/DependencyInjection.cs`, which exposes it as `ILocationDataService`. Its only dependency is `IUnitOfWork`, from which it takes the `LocationData` and `Devices` repositories.

## Responsibilities

- **Ingestion:** Validates a `LocationData` with `LocationData.IsValid()`, confirms the device exists, fills in `Timestamp` with the current UTC time when it is unset, and persists the reading.
- **Device heartbeat:** After a successful store, calls `Device.UpdateHeartbeat()` and saves the device, so the device's "last seen" state follows its newest reading.
- **Latest position:** Returns the most recent reading for a device.
- **History and time range:** Returns a device's readings as a history (newest first, capped) or as the readings inside a start/end time window.
- **Proximity:** Returns readings within a radius of a coordinate.
- **Distance:** Sums the Haversine distance (`LocationData.DistanceTo`) between consecutive readings in a time range.
- **Cleanup:** Deletes readings older than a cutoff date.

Argument checks are done in the service before any repository call, so invalid input never reaches storage.

## API

### Constructor

- **`LocationDataService(IUnitOfWork unitOfWork)`**  
  Takes `unitOfWork.LocationData` and `unitOfWork.Devices`. Normally created by the DI container, not by hand.

### Methods

All methods are asynchronous and accept an optional `CancellationToken`. Constants used below: `ConfigConstants.MAX_LOCATION_HISTORY` is `10000`; history defaults to a `limit` of `100`.

- **`Task<LocationData> StoreLocationAsync(LocationData location, CancellationToken cancellationToken = default)`**  
  Validates the reading, checks that its device exists, sets `Timestamp` to `DateTime.UtcNow` if it equals `default`, stores it, then updates the device's heartbeat.  
  *Returns:* The stored location as returned by the repository.  
  *Throws:* `ArgumentNullException` if `location` is `null`; `ValidationException` if `location.IsValid()` is `false`; `DeviceException` if no device with `location.DeviceId` exists.

- **`Task<LocationData?> GetLatestLocationAsync(string deviceId, CancellationToken cancellationToken = default)`**  
  Retrieves the most recent reading for the device.  
  *Returns:* The latest reading, or `null` if the device has none.  
  *Throws:* `ArgumentException` if `deviceId` is null, empty, or whitespace.

- **`Task<IEnumerable<LocationData>> GetLocationHistoryAsync(string deviceId, int limit = 100, CancellationToken cancellationToken = default)`**  
  Loads the device's readings, orders them by `Timestamp` descending, and takes the first `Math.Min(limit, 10000)`.  
  *Returns:* A list of readings, newest first; empty if there are none. A `limit` above 10000 is silently reduced to 10000.  
  *Throws:* `ArgumentException` if `deviceId` is blank or `limit` is not greater than 0.

- **`Task<IEnumerable<LocationData>> GetLocationsByTimeRangeAsync(string deviceId, DateTime start, DateTime end, CancellationToken cancellationToken = default)`**  
  Returns the device's readings in the given range. The service does not reorder the result; ordering is whatever the repository returns.  
  *Returns:* The readings in the range.  
  *Throws:* `ArgumentException` if `deviceId` is blank or `start` is not earlier than `end`.

- **`Task<IEnumerable<LocationData>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default)`**  
  Returns readings from any device within `radiusKm` of the coordinate, using the repository's `GetWithinRadiusAsync`.  
  *Returns:* The readings inside the radius.  
  *Throws:* `ValidationException` if `latitude` or `longitude` is outside the `MeasurementBounds` limits; `ArgumentException` if `radiusKm` is not positive.

- **`Task<double> CalculateTravelDistanceAsync(string deviceId, DateTime start, DateTime end, CancellationToken cancellationToken = default)`**  
  Gets the readings for the period through `GetLocationsByTimeRangeAsync`, sorts them by `Timestamp` ascending, and sums `DistanceTo` between each consecutive pair. Readings are not filtered for jumps or noise; use `LocationSanityFilter` beforehand if that matters.  
  *Returns:* Total distance in kilometres, or `0` when fewer than two readings exist.  
  *Throws:* The same exceptions as `GetLocationsByTimeRangeAsync`.

- **`Task<int> CleanupOldDataAsync(DateTime olderThan, CancellationToken cancellationToken = default)`**  
  Deletes all readings with a timestamp before `olderThan`.  
  *Returns:* The number of readings deleted.  
  *Throws:* `ArgumentException` if `olderThan` is not earlier than the current UTC time.

## Example usage

Resolve the service from the container, store a reading for a device that is already registered, then read the results back:

```csharp
using GpsTrackerProtocol.Configuration;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddGpsTrackerServices()
    .BuildServiceProvider();

var locations = services.GetRequiredService<ILocationDataService>();

// The device "863719041234567" must already exist, or StoreLocationAsync throws DeviceException.
var reading = new LocationData
{
    DeviceId = "863719041234567",
    Latitude = 50.4501,
    Longitude = 30.5234,
    Speed = 42.0,
    Bearing = 180.0,
    SatelliteCount = 9
    // Timestamp left as default: the service sets it to UtcNow.
};

var stored = await locations.StoreLocationAsync(reading);

var latest = await locations.GetLatestLocationAsync("863719041234567");
Console.WriteLine($"Last fix: {latest?.Latitude}, {latest?.Longitude} at {latest?.Timestamp:u}");

var recent = await locations.GetLocationHistoryAsync("863719041234567", limit: 50);

var from = DateTime.UtcNow.AddDays(-1);
var to = DateTime.UtcNow;
var km = await locations.CalculateTravelDistanceAsync("863719041234567", from, to);
Console.WriteLine($"Travelled {km:F2} km in the last 24 hours");

var nearby = await locations.GetLocationsNearbyAsync(50.45, 30.52, radiusKm: 5);

// Remove readings older than 90 days.
var deleted = await locations.CleanupOldDataAsync(DateTime.UtcNow.AddDays(-90));
```

## Notes

- **Heartbeat side effect:** Every successful `StoreLocationAsync` updates the device's heartbeat. Any code that reads device liveness depends on this.
- **Timestamp handling:** Only `default` timestamps are replaced. A reading with an explicit timestamp keeps it, including a timestamp in the future.
- **Ordering differs by method:** `GetLocationHistoryAsync` and `CalculateTravelDistanceAsync` sort in memory. `GetLocationsByTimeRangeAsync` and `GetLocationsNearbyAsync` return the repository's order.
- **Exception types:** Some argument checks throw `ArgumentException` without a parameter name (time range and cleanup). Catch `ArgumentException` rather than matching on the parameter name.
- **Storage:** The DI registration uses `InMemoryUnitOfWork`, so readings are lost on restart unless a persistent `IUnitOfWork` is registered instead.

## Related

- `ILocationDataService` - the interface this class implements (`docs/ILocationDataService.md`).
- `LocationData` - the reading model, with `IsValid()` and `DistanceTo()` (`docs/LocationData.md`).
- `DeviceService` - registers and manages the devices that readings are stored against.
- `AnalyticsService` and `LocationAggregationWorker` - consumers of this service.
