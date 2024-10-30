# FleetDashboardOptions

Configuration class that holds global settings for the fleet-dashboard UI and routing engine. These values control default behavior such as the optimization algorithm, fuel-price assumptions, speed limits, route complexity, cache durations, and fallback strategies.

## API

### `DefaultAlgorithm`
Gets or sets the default route-optimization algorithm used when the user does not specify one.
Type: `RouteOptimizationAlgorithm`
Default: `RouteOptimizationAlgorithm.QuickestRoute`
No exceptions are thrown; invalid values are clamped to the nearest valid enum entry.

### `DefaultFuelPricePerLiter`
Gets or sets the assumed fuel price (in currency per liter) used in cost calculations.
Type: `double`
Unit: currency / liter
Default: 1.85
Must be ≥ 0; negative values are treated as 0.

### `AverageRoadSpeedKmh`
Gets or sets the average road speed (in km/h) assumed for travel-time estimates when detailed segment data is unavailable.
Type: `double`
Unit: km/h
Default: 60.0
Must be > 0; values ≤ 0 are coerced to 0.01.

### `MaxStopsPerRoute`
Gets or sets the maximum number of delivery or waypoint stops allowed on a single optimized route.
Type: `int`
Default: 50
Must be ≥ 1; values < 1 are set to 1.

### `MaxFleetSize`
Gets or sets the maximum number of vehicles that can be dispatched simultaneously.
Type: `int`
Default: 200
Must be ≥ 1; values < 1 are set to 1.

### `SnapshotCacheTtl`
Gets or sets the time-to-live for cached fleet snapshots used to avoid repeated database queries.
Type: `TimeSpan`
Default: 5 minutes
Must be ≥ TimeSpan.Zero; negative values are treated as zero.

### `EnableDistanceBasedFallback`
Gets or sets a value indicating whether the system should automatically fall back to distance-based routing when traffic or historical data are unavailable.
Type: `bool`
Default: `true`

### `LowFuelThresholdLiters`
Gets or sets the fuel volume (in liters) below which a vehicle is considered low on fuel and triggers a refuel recommendation.
Type: `double`
Unit: liters
Default: 20.0
Must be ≥ 0; negative values are treated as 0.
