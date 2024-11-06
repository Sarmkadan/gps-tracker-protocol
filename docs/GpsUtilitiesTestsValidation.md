# GpsUtilitiesTestsValidation

Static utility class containing validation and testing methods for GPS-related values such as coordinates, distances, bearings, speeds, zoom levels, and bounding boxes. Designed for use in unit tests and validation pipelines to ensure GPS data conforms to expected constraints.

## API

### `public static IReadOnlyList<string> ValidateCoordinates(double latitude, double longitude)`

Validates the given latitude and longitude coordinates. Returns a list of validation error messages; empty if valid. Latitude must be between -90 and 90, longitude between -180 and 180.

**Parameters:**
- `latitude` – Latitude in decimal degrees.
- `longitude` – Longitude in decimal degrees.

**Returns:**
- `IReadOnlyList<string>` – Zero or more error messages describing validation failures.

---

### `public static IReadOnlyList<string> ValidateDistance(double meters)`

Validates a distance value in meters. Returns a list of validation error messages; empty if valid. Distance must be non-negative.

**Parameters:**
- `meters` – Distance in meters.

**Returns:**
- `IReadOnlyList<string>` – Zero or more error messages describing validation failures.

---

### `public static IReadOnlyList<string> ValidateBearing(double degrees)`

Validates a bearing in degrees. Returns a list of validation error messages; empty if valid. Bearing must be between 0 and 360.

**Parameters:**
- `degrees` – Bearing in decimal degrees.

**Returns:**
- `IReadOnlyList<string>` – Zero or more error messages describing validation failures.

---
### `public static IReadOnlyList<string> ValidateSpeed(double metersPerSecond)`

Validates a speed value in meters per second. Returns a list of validation error messages; empty if valid. Speed must be non-negative.

**Parameters:**
- `metersPerSecond` – Speed in meters per second.

**Returns:**
- `IReadOnlyList<string>` – Zero or more error messages describing validation failures.

---
### `public static IReadOnlyList<string> ValidateZoomLevel(double level)`

Validates a map zoom level. Returns a list of validation error messages; empty if valid. Zoom level must be non-negative and typically within a reasonable range (e.g., 0 to 22).

**Parameters:**
- `level` – Zoom level as a double.

**Returns:**
- `IReadOnlyList<string>` – Zero or more error messages describing validation failures.

---
### `public static IReadOnlyList<string> ValidateBoundingBox(double west, double south, double east, double north)`

Validates a bounding box defined by west, south, east, and north coordinates. Returns a list of validation error messages; empty if valid. Longitude values must be ordered correctly (west ≤ east), latitude values must be ordered correctly (south ≤ north), and all coordinates must be within valid GPS bounds.

**Parameters:**
- `west` – Western longitude in decimal degrees.
- `south` – Southern latitude in decimal degrees.
- `east` – Eastern longitude in decimal degrees.
- `north` – Northern latitude in decimal degrees.

**Returns:**
- `IReadOnlyList<string>` – Zero or more error messages describing validation failures.

---
### `public static bool IsValidCoordinates(double latitude, double longitude)`

Determines whether the given latitude and longitude coordinates are valid. Latitude must be between -90 and 90, longitude between -180 and 180.

**Parameters:**
- `latitude` – Latitude in decimal degrees.
- `longitude` – Longitude in decimal degrees.

**Returns:**
- `bool` – `true` if valid; otherwise, `false`.

---
### `public static bool IsValidDistance(double meters)`

Determines whether the given distance in meters is valid. Distance must be non-negative.

**Parameters:**
- `meters` – Distance in meters.

**Returns:**
- `bool` – `true` if valid; otherwise, `false`.

---
### `public static bool IsValidBearing(double degrees)`

Determines whether the given bearing in degrees is valid. Bearing must be between 0 and 360.

**Parameters:**
- `degrees` – Bearing in decimal degrees.

**Returns:**
- `bool` – `true` if valid; otherwise, `false`.

---
### `public static bool IsValidSpeed(double metersPerSecond)`

Determines whether the given speed in meters per second is valid. Speed must be non-negative.

**Parameters:**
- `metersPerSecond` – Speed in meters per second.

**Returns:**
- `bool` – `true` if valid; otherwise, `false`.

---
### `public static bool IsValidZoomLevel(double level)`

Determines whether the given zoom level is valid. Zoom level must be non-negative and typically within a reasonable range (e.g., 0 to 22).

**Parameters:**
- `level` – Zoom level as a double.

**Returns:**
- `bool` – `true` if valid; otherwise, `false`.

---
### `public static bool IsValidBoundingBox(double west, double south, double east, double north)`

Determines whether the given bounding box is valid. Longitude values must be ordered correctly (west ≤ east), latitude values must be ordered correctly (south ≤ north), and all coordinates must be within valid GPS bounds.

**Parameters:**
- `west` – Western longitude in decimal degrees.
- `south` – Southern latitude in decimal degrees.
- `east` – Eastern longitude in decimal degrees.
- `north` – Northern latitude in decimal degrees.

**Returns:**
- `bool` – `true` if valid; otherwise, `false`.

---
### `public static void EnsureValidCoordinates(double latitude, double longitude)`

Throws an exception if the given latitude and longitude coordinates are invalid. Latitude must be between -90 and 90, longitude between -180 and 180.

**Parameters:**
- `latitude` – Latitude in decimal degrees.
- `longitude` – Longitude in decimal degrees.

**Throws:**
- `ArgumentOutOfRangeException` – If coordinates are invalid.

---
### `public static void EnsureValidDistance(double meters)`

Throws an exception if the given distance in meters is invalid. Distance must be non-negative.

**Parameters:**
- `meters` – Distance in meters.

**Throws:**
- `ArgumentOutOfRangeException` – If distance is invalid.

---
### `public static void EnsureValidBearing(double degrees)`

Throws an exception if the given bearing in degrees is invalid. Bearing must be between 0 and 360.

**Parameters:**
- `degrees` – Bearing in decimal degrees.

**Throws:**
- `ArgumentOutOfRangeException` – If bearing is invalid.

---
### `public static void EnsureValidSpeed(double metersPerSecond)`

Throws an exception if the given speed in meters per second is invalid. Speed must be non-negative.

**Parameters:**
- `metersPerSecond` – Speed in meters per second.

**Throws:**
- `ArgumentOutOfRangeException` – If speed is invalid.

---
### `public static void EnsureValidZoomLevel(double level)`

Throws an exception if the given zoom level is invalid. Zoom level must be non-negative and typically within a reasonable range (e.g., 0 to 22).

**Parameters:**
- `level` – Zoom level as a double.

**Throws:**
- `ArgumentOutOfRangeException` – If zoom level is invalid.

---
### `public static void EnsureValidBoundingBox(double west, double south, double east, double north)`

Throws an exception if the given bounding box is invalid. Longitude values must be ordered correctly (west ≤ east), latitude values must be ordered correctly (south ≤ north), and all coordinates must be within valid GPS bounds.

**Parameters:**
- `west` – Western longitude in decimal degrees.
- `south` – Southern latitude in decimal degrees.
- `east` – Eastern longitude in decimal degrees.
- `north` – Northern latitude in decimal degrees.

**Throws:**
- `ArgumentOutOfRangeException` – If bounding box is invalid.

## Usage
