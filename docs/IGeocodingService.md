# IGeocodingService

The `IGeocodingService` interface provides functionality for reverse geocoding—converting geographic coordinates (latitude and longitude) into human-readable address information. It is used to retrieve location details such as address, city, country, and a display name based on provided coordinates. This service is essential for applications requiring location-based context, such as mapping, navigation, or geofencing.

## API

### `GeocodingService`
The concrete implementation of `IGeocodingService` that performs reverse geocoding operations.

### `Task<GeocodingResult> ReverseGeocodeAsync()`
Asynchronously retrieves address information for the stored `Latitude` and `Longitude` values.

**Returns:**
A `Task<GeocodingResult>` containing the geocoding result, including address components (`Address`, `City`, `Country`, `DisplayName`) and a `Success` flag indicating whether the operation completed successfully.

**Throws:**
- `HttpRequestException` if the underlying geocoding service fails to respond.
- `ArgumentException` if `Latitude` or `Longitude` are outside valid ranges (`[-90, 90]` and `[-180, 180]`, respectively).

### `Task<bool> IsInRegionAsync()`
Asynchronously determines whether the stored coordinates fall within a predefined region (e.g., a city, country, or custom boundary).

**Returns:**
A `Task<bool>` indicating `true` if the coordinates are within the region, otherwise `false`.

**Throws:**
- `InvalidOperationException` if the region data is unavailable or misconfigured.
- `HttpRequestException` if the region-checking service fails to respond.

### `double Latitude`
The latitude coordinate (in decimal degrees) used for geocoding operations. Must be within `[-90, 90]`.

### `double Longitude`
The longitude coordinate (in decimal degrees) used for geocoding operations. Must be within `[-180, 180]`.

### `string Address`
The full street address derived from the reverse geocoding operation. May be `null` or empty if the operation fails or no address is found.

### `string City`
The city or locality derived from the reverse geocoding operation. May be `null` or empty if unavailable.

### `string Country`
The country derived from the reverse geocoding operation. May be `null` or empty if unavailable.

### `string DisplayName`
A human-readable concatenation of address components (e.g., "123 Main St, Springfield, USA"). May be `null` or empty if the operation fails.

### `bool Success`
Indicates whether the last reverse geocoding operation completed successfully. `false` if an error occurred or no valid address was found.

## Usage

### Example 1: Reverse Geocoding Coordinates
