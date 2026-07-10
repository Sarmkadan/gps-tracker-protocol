# GpsUtilities

`GpsUtilities` is a static utility class providing a collection of geodetic and kinematic conversion functions. It centralizes common calculations such as distance and bearing between coordinates, validation of geographic points, conversion between decimal degrees and degrees-minutes-seconds (DMS), speed unit transformations, map zoom level estimation, and bounding box geometry.

## API

### `public static double CalculateDistanceKm`
Calculates the great-circle distance between two geographic coordinates using the Haversine formula.

| Parameter | Type | Description |
|-----------|------|-------------|
| `latitude1` | `double` | Latitude of the first point in decimal degrees. |
| `longitude1` | `double` | Longitude of the first point in decimal degrees. |
| `latitude2` | `double` | Latitude of the second point in decimal degrees. |
| `longitude2` | `double` | Longitude of the second point in decimal degrees. |

**Returns:** `double` — The distance in kilometers.

**Throws:** No exceptions are thrown; inputs are assumed to be valid numeric values.

---

### `public static double CalculateBearing`
Computes the initial bearing (forward azimuth) from the first coordinate to the second, measured clockwise from true north.

| Parameter | Type | Description |
|-----------|------|-------------|
| `latitude1` | `double` | Latitude of the origin in decimal degrees. |
| `longitude1` | `double` | Longitude of the origin in decimal degrees. |
| `latitude2` | `double` | Latitude of the destination in decimal degrees. |
| `longitude2` | `double` | Longitude of the destination in decimal degrees. |

**Returns:** `double` — The bearing in degrees, normalized to the range [0, 360).

**Throws:** No exceptions are thrown; inputs are assumed to be valid numeric values.

---

### `public static bool IsValidCoordinate`
Determines whether a given latitude and longitude pair represents a valid geographic coordinate.

| Parameter | Type | Description |
|-----------|------|-------------|
| `latitude` | `double` | Latitude value to validate. |
| `longitude` | `double` | Longitude value to validate. |

**Returns:** `bool` — `true` if latitude is within [-90, 90] and longitude is within [-180, 180]; otherwise `false`.

**Throws:** No exceptions are thrown.

---

### `public static bool IsWithinBounds`
Checks whether a target coordinate lies inside a rectangular bounding region defined by minimum and maximum latitude and longitude.

| Parameter | Type | Description |
|-----------|------|-------------|
| `latitude` | `double` | Latitude of the point to test. |
| `longitude` | `double` | Longitude of the point to test. |
| `minLatitude` | `double` | Southern boundary of the bounding box. |
| `maxLatitude` | `double` | Northern boundary of the bounding box. |
| `minLongitude` | `double` | Western boundary of the bounding box. |
| `maxLongitude` | `double` | Eastern boundary of the bounding box. |

**Returns:** `bool` — `true` if the point falls within all four bounds inclusive; otherwise `false`.

**Throws:** No exceptions are thrown.

---

### `public static double DmsToDecimal`
Converts an angle expressed in degrees, minutes, and seconds to decimal degrees.

| Parameter | Type | Description |
|-----------|------|-------------|
| `degrees` | `int` | Whole degrees component. |
| `minutes` | `int` | Minutes component (expected range 0–59). |
| `seconds` | `double` | Seconds component (expected range 0–59.999...). |

**Returns:** `double` — The equivalent value in decimal degrees. The sign of the result matches the sign of `degrees`; if `degrees` is zero, the sign is positive.

**Throws:** No exceptions are thrown. Negative minute or second values are processed algebraically.

---

### `public static (int degrees, int minutes, double seconds) DecimalToDms`
Decomposes a decimal-degree angle into its degrees, minutes, and seconds representation.

| Parameter | Type | Description |
|-----------|------|-------------|
| `decimalDegrees` | `double` | Angle in decimal degrees. |

**Returns:** `(int degrees, int minutes, double seconds)` — A tuple where `degrees` carries the sign, `minutes` is the integer minute component (0–59), and `seconds` is the fractional remainder (0 ≤ seconds < 60).

**Throws:** No exceptions are thrown.

---

### `public static double KnotsToKmh`
Converts a speed from knots to kilometers per hour.

| Parameter | Type | Description |
|-----------|------|-------------|
| `knots` | `double` | Speed in knots. |

**Returns:** `double` — Speed in kilometers per hour.

**Throws:** No exceptions are thrown.

---

### `public static double KmhToKnots`
Converts a speed from kilometers per hour to knots.

| Parameter | Type | Description |
|-----------|------|-------------|
| `kmh` | `double` | Speed in kilometers per hour. |

**Returns:** `double` — Speed in knots.

**Throws:** No exceptions are thrown.

---

### `public static double KmhToMs`
Converts a speed from kilometers per hour to meters per second.

| Parameter | Type | Description |
|-----------|------|-------------|
| `kmh` | `double` | Speed in kilometers per hour. |

**Returns:** `double` — Speed in meters per second.

**Throws:** No exceptions are thrown.

---

### `public static int CalculateZoomLevel`
Estimates an appropriate map zoom level for a given bounding box width in kilometers, suitable for displaying the entire extent on a typical screen.

| Parameter | Type | Description |
|-----------|------|-------------|
| `widthKm` | `double` | The east-west span of the bounding box in kilometers. |

**Returns:** `int` — A zoom level integer (typically in the range 0–20). Smaller values correspond to a wider view; larger values to a closer view.

**Throws:** No exceptions are thrown. Very small or zero widths may produce high zoom levels.

---

### `public static (double latitude, double longitude) GetBoundingBoxCenter`
Computes the geographic center of a rectangular bounding box defined by its southern, northern, western, and eastern edges.

| Parameter | Type | Description |
|-----------|------|-------------|
| `minLatitude` | `double` | Southern boundary latitude. |
| `maxLatitude` | `double` | Northern boundary latitude. |
| `minLongitude` | `double` | Western boundary longitude. |
| `maxLongitude` | `double` | Eastern boundary longitude. |

**Returns:** `(double latitude, double longitude)` — The midpoint latitude and longitude.

**Throws:** No exceptions are thrown. The calculation is a simple arithmetic mean and does not account for antimeridian wrapping.

## Usage

### Example 1: Validating a coordinate and computing distance/bearing to a destination

```csharp
double lat = 48.8566;   // Paris
double lon = 2.3522;
double destLat = 40.6892; // New York (Statue of Liberty approximate)
double destLon = -74.0445;

if (GpsUtilities.IsValidCoordinate(lat, lon) && GpsUtilities.IsValidCoordinate(destLat, destLon))
{
    double distance = GpsUtilities.CalculateDistanceKm(lat, lon, destLat, destLon);
    double bearing = GpsUtilities.CalculateBearing(lat, lon, destLat, destLon);

    Console.WriteLine($"Distance: {distance:F2} km");
    Console.WriteLine($"Initial bearing: {bearing:F2}°");
}
else
{
    Console.WriteLine("One or both coordinates are invalid.");
}
```

### Example 2: Converting DMS to decimal, checking bounds, and finding a bounding box center

```csharp
// Convert a DMS coordinate (e.g., 37°25'19.1"N, 122°05'06"W) to decimal degrees
double latDecimal = GpsUtilities.DmsToDecimal(37, 25, 19.1);
double lonDecimal = GpsUtilities.DmsToDecimal(-122, 5, 6.0);

// Define a bounding box around the San Francisco Bay Area
double minLat = 37.0;
double maxLat = 38.0;
double minLon = -123.0;
double maxLon = -122.0;

bool inside = GpsUtilities.IsWithinBounds(latDecimal, lonDecimal, minLat, maxLat, minLon, maxLon);
Console.WriteLine($"Point inside bounding box: {inside}");

var center = GpsUtilities.GetBoundingBoxCenter(minLat, maxLat, minLon, maxLon);
Console.WriteLine($"Bounding box center: {center.latitude:F4}, {center.longitude:F4}");

// Estimate zoom level for the bounding box width
double widthKm = GpsUtilities.CalculateDistanceKm(minLat, minLon, minLat, maxLon);
int zoom = GpsUtilities.CalculateZoomLevel(widthKm);
Console.WriteLine($"Suggested zoom level: {zoom}");
```

## Notes

- All methods are static and stateless; the class is safe to call from multiple threads concurrently without synchronization.
- `CalculateDistanceKm` and `CalculateBearing` assume a spherical Earth model. Accuracy is sufficient for most consumer-grade GPS applications but degrades slightly over very long distances or near the poles.
- `IsWithinBounds` performs inclusive comparisons. A point exactly on the boundary is considered inside.
- `DmsToDecimal` preserves the sign of the `degrees` parameter. Passing negative values for `minutes` or `seconds` is mathematically accepted but may produce results that do not correspond to standard DMS notation.
- `DecimalToDms` returns `minutes` as an absolute value between 0 and 59 and `seconds` as a non-negative fractional remainder. The `degrees` component carries the original sign.
- `GetBoundingBoxCenter` computes the arithmetic mean of the edges. It does not handle bounding boxes that cross the antimeridian (±180° longitude); such cases require pre-normalization by the caller.
- `CalculateZoomLevel` returns an integer suitable for common web map tile schemes. The exact mapping between width and zoom level is implementation-defined and may be calibrated for a specific screen resolution or tile size.
- Speed conversion methods use fixed conversion factors and do not guard against non-finite or extreme input values.
