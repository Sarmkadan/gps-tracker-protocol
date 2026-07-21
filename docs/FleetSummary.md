# FleetSummary

Provides a high-level abstraction over fleet operations, exposing asynchronous methods to register, retrieve, update, and remove vehicles, as well as compute fleet-level analytics and route optimizations. Designed for use within dashboard and backend services to manage vehicle fleets and expose summarized state.

## API

### `FleetSummary`

A sealed record representing a high-level summary of a vehicle fleet, including counts, status distributions, and operational metrics. Immutable and designed for data transfer.

### `FleetDashboardService`

A service class responsible for coordinating fleet operations, including vehicle lifecycle management and analytics computation. Injected into consumers to provide access to fleet functionality.

### `public async Task<FleetVehicle> RegisterVehicleAsync(string vin, string licensePlate, string model)`

Registers a new vehicle in the fleet.

- **Parameters**:
  - `vin`: Unique Vehicle Identification Number.
  - `licensePlate`: Human-readable license plate identifier.
  - `model`: Vehicle model description.
- **Returns**: The newly registered `FleetVehicle` instance.
- **Throws**: `ArgumentException` if `vin` or `licensePlate` is null or empty; `InvalidOperationException` if the VIN already exists.

### `public async Task<FleetVehicle?> GetVehicleAsync(string vin)`

Retrieves a vehicle by its VIN.

- **Parameters**:
  - `vin`: The VIN of the vehicle to retrieve.
- **Returns**: The `FleetVehicle` if found; otherwise, `null`.
- **Throws**: `ArgumentException` if `vin` is null or empty.

### `public async Task<IEnumerable<FleetVehicle>> GetAllVehiclesAsync()`

Retrieves all vehicles in the fleet.

- **Returns**: An enumerable collection of all `FleetVehicle` instances.
- **Throws**: None expected under normal operation.

### `public async Task<FleetVehicle> UpdateVehicleAsync(FleetVehicle vehicle)`

Updates an existing vehicle’s metadata.

- **Parameters**:
  - `vehicle`: The updated `FleetVehicle` instance, identified by VIN.
- **Returns**: The updated `FleetVehicle`.
- **Throws**: `ArgumentNullException` if `vehicle` is `null`; `ArgumentException` if `vehicle.Vin` is null or empty; `KeyNotFoundException` if the VIN does not exist.

### `public async Task<bool> RemoveVehicleAsync(string vin)`

Removes a vehicle from the fleet.

- **Parameters**:
  - `vin`: The VIN of the vehicle to remove.
- **Returns**: `true` if the vehicle was found and removed; otherwise, `false`.
- **Throws**: `ArgumentException` if `vin` is null or empty.

### `public async Task<FleetDashboardSnapshot> GetDashboardSnapshotAsync()`

Retrieves a snapshot of the current fleet state for dashboard display.

- **Returns**: A `FleetDashboardSnapshot` containing fleet-wide metrics and status summaries.
- **Throws**: None expected under normal operation.

### `public async Task<VehicleStatusSummary> GetVehicleStatusAsync(string vin)`

Retrieves the current operational status of a specific vehicle.

- **Parameters**:
  - `vin`: The VIN of the vehicle.
- **Returns**: A `VehicleStatusSummary` describing the vehicle’s state.
- **Throws**: `ArgumentException` if `vin` is null or empty; `KeyNotFoundException` if the VIN does not exist.

### `public async Task<OptimizedRoute> OptimizeRouteAsync(IEnumerable<string> vinList, GeoCoordinate origin)`

Computes an optimized route visiting a set of vehicles from a given origin.

- **Parameters**:
  - `vinList`: Collection of VINs identifying vehicles to visit.
  - `origin`: Starting geographic coordinate.
- **Returns**: An `OptimizedRoute` containing the ordered sequence of stops and total distance.
- **Throws**: `ArgumentNullException` if `vinList` or `origin` is `null`; `ArgumentException` if any VIN is null or empty; `InvalidOperationException` if the VIN list is empty.

### `public async Task<IReadOnlyDictionary<string, double>> ComputeFleetKpisAsync(DateTime from, DateTime to)`

Computes key performance indicators for the fleet over a time range.

- **Parameters**:
  - `from`: Start of the time range (inclusive).
  - `to`: End of the time range (inclusive).
- **Returns**: A read-only dictionary mapping KPI names (e.g., `"avgSpeed"`, `"totalDistance"`) to computed values.
- **Throws**: `ArgumentException` if `from` is after `to`.

### `public async Task<FleetSummary> GetFleetSummaryAsync()`

Retrieves a comprehensive summary of the entire fleet.

- **Returns**: A `FleetSummary` record with fleet-wide statistics and distributions.
- **Throws**: None expected under normal operation.

## Usage
