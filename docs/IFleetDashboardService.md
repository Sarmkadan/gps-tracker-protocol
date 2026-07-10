# IFleetDashboardService

A service contract that provides fleet management capabilities, including vehicle registration, retrieval, updates, removal, and fleet-level analytics such as dashboard snapshots, status summaries, route optimization, and KPI computation.

## API

### `RegisterVehicleAsync`
Registers a new vehicle into the fleet.

- **Parameters**
  - `vehicle`: The `FleetVehicle` instance representing the vehicle to register.
- **Return value**
  - A `Task<FleetVehicle>` that resolves to the registered vehicle, including any system-generated identifiers.
- **Exceptions**
  - Throws `ArgumentNullException` if `vehicle` is `null`.
  - Throws `InvalidOperationException` if the vehicle identifier already exists in the fleet.

---

### `GetVehicleAsync`
Retrieves a single vehicle by its unique identifier.

- **Parameters**
  - `vehicleId`: The unique identifier of the vehicle to retrieve.
- **Return value**
  - A `Task<FleetVehicle?>` that resolves to the vehicle if found, or `null` otherwise.
- **Exceptions**
  - Throws `ArgumentException` if `vehicleId` is `null` or whitespace.

---

### `GetAllVehiclesAsync`
Retrieves all vehicles currently registered in the fleet.

- **Return value**
  - A `Task<IEnumerable<FleetVehicle>>` that resolves to an enumerable of all registered vehicles.
- **Exceptions**
  - None.

---
### `UpdateVehicleAsync`
Updates an existing vehicle’s attributes.

- **Parameters**
  - `vehicle`: The `FleetVehicle` instance containing updated attributes. The `vehicleId` property must match an existing vehicle.
- **Return value**
  - A `Task<FleetVehicle>` that resolves to the updated vehicle.
- **Exceptions**
  - Throws `ArgumentNullException` if `vehicle` is `null`.
  - Throws `KeyNotFoundException` if no vehicle with the specified `vehicleId` exists.
  - Throws `InvalidOperationException` if the update would violate uniqueness constraints on indexed fields.

---
### `RemoveVehicleAsync`
Removes a vehicle from the fleet.

- **Parameters**
  - `vehicleId`: The unique identifier of the vehicle to remove.
- **Return value**
  - A `Task<bool>` that resolves to `true` if the vehicle was found and removed, `false` otherwise.
- **Exceptions**
  - Throws `ArgumentException` if `vehicleId` is `null` or whitespace.

---
### `GetDashboardSnapshotAsync`
Retrieves a snapshot of the current fleet state for dashboard display.

- **Return value**
  - A `Task<FleetDashboardSnapshot>` that resolves to a snapshot containing aggregated fleet metrics.
- **Exceptions**
  - None.

---
### `GetVehicleStatusAsync`
Retrieves a summary of operational statuses across the fleet.

- **Return value**
  - A `Task<VehicleStatusSummary>` that resolves to a breakdown of vehicle statuses (e.g., online, offline, idle).
- **Exceptions**
  - None.

---
### `OptimizeRouteAsync`
Computes an optimized route for a set of vehicles based on current positions and destinations.

- **Parameters**
  - `vehicleIds`: The set of vehicle identifiers to include in the route optimization.
  - `destinations`: A dictionary mapping vehicle identifiers to target destinations.
- **Return value**
  - A `Task<OptimizedRoute>` that resolves to the optimized route plan, including waypoints and sequence.
- **Exceptions**
  - Throws `ArgumentNullException` if `vehicleIds` or `destinations` is `null`.
  - Throws `ArgumentException` if `vehicleIds` is empty or if any key in `destinations` is not present in `vehicleIds`.

---
### `ComputeFleetKpisAsync`
Computes key performance indicators for the entire fleet.

- **Return value**
  - A `Task<IReadOnlyDictionary<string, double>>` that resolves to a read-only dictionary of KPI names to computed values (e.g., average speed, total distance, idle time).
- **Exceptions**
  - None.

## Usage

### Register and retrieve a vehicle
