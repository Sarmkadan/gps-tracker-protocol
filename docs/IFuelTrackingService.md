# IFuelTrackingService

The `IFuelTrackingService` interface defines a contract for recording, retrieving, and managing fuel consumption events and reports in a GPS tracking system. It is designed to integrate with vehicle telemetry data, allowing applications to log fuel events, query historical records, generate consumption reports, and estimate fuel usage.

## API

### `Task<FuelRecord> RecordFuelEventAsync(DateTimeOffset timestamp, double liters, double? latitude, double? longitude, string? notes)`

Records a new fuel event in the tracking system.

- **Parameters**
  - `timestamp` – The date and time when the fuel event occurred.
  - `liters` – The amount of fuel added, in liters. Must be a positive value.
  - `latitude` – Optional geographic latitude coordinate where the event occurred.
  - `longitude` – Optional geographic longitude coordinate where the event occurred.
  - `notes` – Optional descriptive text about the fuel event (e.g., "Refuel at station X").

- **Return Value**
  Returns a `Task<FuelRecord>` that resolves to the newly created `FuelRecord` with a system-generated identifier and the provided data.

- **Exceptions**
  Throws `ArgumentException` if `liters` is zero or negative.
  Throws `ArgumentNullException` if `timestamp` is `default(DateTimeOffset)`.

---

### `Task<IEnumerable<FuelRecord>> GetRecordsAsync(DateTimeOffset? from, DateTimeOffset? to)`

Retrieves a sequence of fuel records within a specified time range.

- **Parameters**
  - `from` – Optional start of the time range (inclusive). If `null`, defaults to the earliest recorded event.
  - `to` – Optional end of the time range (inclusive). If `null`, defaults to the current time.

- **Return Value**
  Returns a `Task<IEnumerable<FuelRecord>>` containing all matching fuel records, ordered chronologically.

- **Exceptions**
  Throws `ArgumentException` if `from` is later than `to`.

---

### `Task<bool> DeleteRecordAsync(Guid recordId)`

Removes a specific fuel record from the system.

- **Parameters**
  - `recordId` – The unique identifier of the record to delete.

- **Return Value**
  Returns a `Task<bool>` that resolves to `true` if the record existed and was deleted, `false` otherwise.

- **Exceptions**
  None.

---

### `Task<FuelConsumptionReport> GetReportAsync(DateTimeOffset from, DateTimeOffset to)`

Generates a fuel consumption report for a specified period.

- **Parameters**
  - `from` – Start of the reporting period (inclusive).
  - `to` – End of the reporting period (inclusive).

- **Return Value**
  Returns a `Task<FuelConsumptionReport>` containing total liters consumed, average consumption rate, and a breakdown by day.

- **Exceptions**
  Throws `ArgumentException` if `from` is later than `to`.

---

### `double EstimateFuelLiters(double distanceKm, double averageSpeedKmh)`

Estimates fuel consumption in liters based on distance traveled and average speed.

- **Parameters**
  - `distanceKm` – The distance traveled, in kilometers. Must be non-negative.
  - `averageSpeedKmh` – The average speed during travel, in kilometers per hour. Must be non-negative.

- **Return Value**
  Returns an estimated fuel consumption in liters, calculated using a calibrated consumption model.

- **Exceptions**
  Throws `ArgumentException` if either parameter is negative.

## Usage
