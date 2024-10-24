# IGeofenceService

The `IGeofenceService` interface defines the contract for managing and evaluating geographic boundaries within the `gps-tracker-protocol` project. It provides mechanisms to register geofence definitions, determine if specific coordinates fall within a defined boundary, and retrieve a list of geofences proximate to a given location. This service acts as the central logic layer for location-based triggers and spatial queries.

## API

### Properties

#### `string Id`
Gets the unique identifier for the geofence instance. This value is immutable once the geofence is created and is used for tracking and reference operations.

#### `string Name`
Gets or sets the human-readable name assigned to the geofence. This property is mutable and intended for display or logging purposes.

#### `double CenterLatitude`
Gets or sets the latitude coordinate of the geofence center point in decimal degrees.

#### `double CenterLongitude`
Gets or sets the longitude coordinate of the geofence center point in decimal degrees.

#### `double RadiusKm`
Gets or sets the radius of the geofence in kilometers. This value defines the circular boundary around the center point.

#### `DateTime CreatedAt`
Gets the timestamp indicating when the geofence was initially registered with the service.

#### `Dictionary<string, object> Metadata`
Gets or sets a collection of key-value pairs containing arbitrary data associated with the geofence. This allows for extensible context without modifying the core schema.

### Methods

#### `void AddGeofence(GeofenceService geofence)`
Registers a new geofence definition with the service.
*   **Parameters**:
    *   `geofence`: An instance of `GeofenceService` containing the configuration for the new boundary.
*   **Return Value**: None.
*   **Exceptions**: Throws an exception if a geofence with a duplicate `Id` already exists or if the provided `geofence` instance is null.

#### `bool IsInsideGeofence(string id, double latitude, double longitude)`
Evaluates whether a specific geographic coordinate lies within the boundary of a registered geofence.
*   **Parameters**:
    *   `id`: The unique identifier of the geofence to check against.
    *   `latitude`: The latitude of the point to evaluate.
    *   `longitude`: The longitude of the point to evaluate.
*   **Return Value**: Returns `true` if the point is within the radius of the specified geofence; otherwise, `false`.
*   **Exceptions**: Throws an exception if no geofence with the specified `id` is found.

#### `IEnumerable<string> GetNearbyGeofences(double latitude, double longitude, double maxDistanceKm)`
Retrieves a list of geofence identifiers that are within a specified distance from a given coordinate.
*   **Parameters**:
    *   `latitude`: The reference latitude.
    *   `longitude`: The reference longitude.
    *   `maxDistanceKm`: The maximum distance in kilometers from the reference point to include a geofence in the results.
*   **Return Value**: An enumerable collection of geofence `Id` strings. The order of results is not guaranteed.
*   **Exceptions**: Throws an exception if `maxDistanceKm` is negative.

## Usage

### Registering and Evaluating a Geofence
The following example demonstrates how to instantiate a geofence, register it with the service, and verify if a tracker's current location is inside the boundary.

```csharp
// Initialize the service implementation
var service = new GeofenceService();

// Define a new geofence for a warehouse
var warehouseZone = new GeofenceService
{
    Id = "wh-zone-01",
    Name = "Main Warehouse",
    CenterLatitude = 40.7128,
    CenterLongitude = -74.0060,
    RadiusKm = 0.5,
    Metadata = new Dictionary<string, object> { { "Type", "Logistics" } }
};

// Register the geofence
service.AddGeofence(warehouseZone);

// Check if a specific GPS coordinate is inside the zone
double currentLat = 40.7130;
double currentLon = -74.0055;

bool isInside = service.IsInsideGeofence("wh-zone-01", currentLat, currentLon);

if (isInside)
{
    Console.WriteLine("Tracker entered the warehouse zone.");
}
```

### Querying Proximity
This example illustrates how to find all active geofences near a vehicle's current position to determine potential upcoming alerts.

```csharp
// Assume 'service' is already populated with multiple geofences
double vehicleLat = 34.0522;
double vehicleLon = -118.2437;
double searchRadius = 10.0; // Search within 10km

// Retrieve IDs of all geofences within the search radius
var nearbyZones = service.GetNearbyGeofences(vehicleLat, vehicleLon, searchRadius);

Console.WriteLine($"Found {nearbyZones.Count()} nearby geofences:");
foreach (var zoneId in nearbyZones)
{
    // Additional logic can fetch full details using the ID if needed
    Console.WriteLine($"- Zone ID: {zoneId}");
}
```

## Notes

*   **Thread Safety**: The implementation of `IGeofenceService` is not guaranteed to be thread-safe for write operations. While `IsInsideGeofence` and `GetNearbyGeofences` may be safe for concurrent reads depending on the underlying collection implementation, `AddGeofence` modifies the internal state. External synchronization is required if the service is accessed concurrently by multiple threads performing writes or mixed read/write operations.
*   **Coordinate Validation**: The interface does not explicitly enforce latitude (-90 to 90) or longitude (-180 to 180) ranges in the signature. Implementations may throw format exceptions if invalid coordinates are passed to `IsInsideGeofence` or `GetNearbyGeofences`.
*   **Radius Constraints**: A `RadiusKm` value of zero or negative in the `GeofenceService` object passed to `AddGeofence` may result in a geofence that never returns `true` for `IsInsideGeofence`, unless the implementation specifically handles zero-radius points as exact matches.
*   **Metadata Mutability**: The `Metadata` dictionary is a reference type. Modifying the contents of the dictionary after the geofence has been added to the service will affect the stored state directly, as no deep copy is performed during registration.
