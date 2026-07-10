# IAnalyticsService

The `IAnalyticsService` interface defines a contract for retrieving analytics data related to GPS tracking devices, journeys, routes, and fleet performance. It provides methods to compute and retrieve aggregated metrics such as journey counts, durations, device activity, and route statistics, enabling applications to analyze tracking data efficiently.

## API

### `AnalyticsService` (implementation)
The concrete implementation of `IAnalyticsService` that provides the actual analytics computation and data retrieval logic. This class is responsible for executing queries against the underlying data store and calculating derived metrics such as averages, totals, and device-specific analytics.

---

### `public AnalyticsService(...)`
Constructs a new instance of the analytics service. The constructor typically accepts dependencies such as data access layers or configuration settings required to perform analytics computations.

**Parameters:**
- `...` (implementation-specific parameters, e.g., `IJourneyRepository journeyRepo`, `IDeviceRepository deviceRepo`, `IAnalyticsCalculator calculator`)

**Exceptions:**
- Throws `ArgumentNullException` if any required dependency is `null`.
- May throw `InvalidOperationException` if required configuration is missing or invalid.

---

### `public async Task<int> GetTotalJourneysAsync()`
Retrieves the total number of recorded journeys across all tracked devices.

**Return value:**
- An integer representing the total number of journeys.

**Exceptions:**
- Throws `InvalidOperationException` if the underlying data store is unavailable or the query fails.

---

### `public async Task<TimeSpan> GetAverageJourneyDurationAsync()`
Computes the average duration of all recorded journeys.

**Return value:**
- A `TimeSpan` representing the average journey duration.

**Exceptions:**
- Throws `InvalidOperationException` if no journeys are available or the computation fails.
- Throws `OverflowException` if the aggregated duration exceeds the maximum representable `TimeSpan`.

---
### `public async Task<string?> GetMostActiveDeviceAsync()`
Retrieves the identifier of the device with the highest number of recorded journeys.

**Return value:**
- A `string` containing the device ID, or `null` if no devices are present.

**Exceptions:**
- Throws `InvalidOperationException` if the underlying data store is unavailable or the query fails.

---
### `public async Task<DeviceAnalytics> GetDeviceAnalyticsAsync(string deviceId)`
Retrieves analytics for a specific device identified by `deviceId`.

**Parameters:**
- `deviceId` (string): The unique identifier of the device.

**Return value:**
- A `DeviceAnalytics` object containing metrics such as total journeys, total distance, average speed, and journey statistics for the specified device.

**Exceptions:**
- Throws `ArgumentException` if `deviceId` is `null` or empty.
- Throws `InvalidOperationException` if the device does not exist or the query fails.

---
### `public async Task<FleetAnalytics> GetFleetAnalyticsAsync()`
Retrieves aggregated analytics for the entire fleet of tracked devices.

**Return value:**
- A `FleetAnalytics` object containing fleet-wide metrics such as total devices, total journeys, total distance, and average performance indicators.

**Exceptions:**
- Throws `InvalidOperationException` if the underlying data store is unavailable or the computation fails.

---
### `public async Task<RouteAnalytics> GetRouteAnalyticsAsync(string routeId)`
Retrieves analytics for a specific route identified by `routeId`.

**Parameters:**
- `routeId` (string): The unique identifier of the route.

**Return value:**
- A `RouteAnalytics` object containing metrics such as total journeys, total distance, average duration, and speed statistics for the specified route.

**Exceptions:**
- Throws `ArgumentException` if `routeId` is `null` or empty.
- Throws `InvalidOperationException` if the route does not exist or the query fails.

---
### `public string DeviceId`
Gets the unique identifier of the device associated with the analytics data.

**Return value:**
- A `string` representing the device ID.

---
### `public DateTime ComputedAt`
Gets the timestamp when the analytics data was computed.

**Return value:**
- A `DateTime` indicating the moment the analytics were calculated.

---
### `public int TotalLocationPoints`
Gets the total number of location points recorded for the associated device or fleet.

**Return value:**
- An integer representing the count of location points.

---
### `public double AverageSpeed`
Gets the average speed across all recorded journeys or location points.

**Return value:**
- A `double` representing the average speed in consistent units (e.g., km/h).

---
### `public double MaxSpeed`
Gets the maximum speed recorded across all journeys or location points.

**Return value:**
- A `double` representing the maximum speed in consistent units.

---
### `public double MinSpeed`
Gets the minimum speed recorded across all journeys or location points.

**Return value:**
- A `double` representing the minimum speed in consistent units.

---
### `public int TotalJourneys`
Gets the total number of journeys recorded for the associated device or fleet.

**Return value:**
- An integer representing the total journey count.

---
### `public double TotalDistance`
Gets the total distance traveled across all journeys or location points.

**Return value:**
- A `double` representing the total distance in consistent units (e.g., kilometers).

---
### `public double TotalDurationHours`
Gets the total duration of all journeys or location points in hours.

**Return value:**
- A `double` representing the total duration in hours.

---
### `public double AverageSatellites`
Gets the average number of satellites used during location tracking.

**Return value:**
- A `double` representing the average satellite count.

---
### `public DateTime ComputedAt`
Gets the timestamp when the analytics data was computed.

**Return value:**
- A `DateTime` indicating the moment the analytics were calculated.

---
### `public int TotalDevices`
Gets the total number of devices in the fleet.

**Return value:**
- An integer representing the device count.

## Usage

### Example 1: Retrieving Fleet-Wide Analytics
