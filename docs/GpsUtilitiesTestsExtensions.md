# GpsUtilitiesTestsExtensions

`GpsUtilitiesTestsExtensions` is a static utility class within the `gps-tracker-protocol` project that provides helper methods for test code dealing with geographic coordinates. It offers concise factory methods for creating coordinate pairs and bounding boxes, a midpoint calculator, an approximate coordinate validator, and a custom assertion for floating-point coordinate comparisons. These members are designed to reduce boilerplate in unit tests that verify GPS-related logic.

## API

### CreateCoordinate

```csharp
public static (double lat, double lon) CreateCoordinate(double lat, double lon)
```

Creates a coordinate tuple from explicit latitude and longitude values. This is a convenience factory that eliminates the need to construct tuples manually in test arrangements.

**Parameters:**
- `lat` — Latitude in decimal degrees.
- `lon` — Longitude in decimal degrees.

**Returns:** A value tuple `(double lat, double lon)` containing the supplied values.

**Throws:** No exceptions are thrown by this method; it performs no validation on the inputs.

---

### CreateBoundingBox

```csharp
public static (double minLat, double maxLat, double minLon, double maxLon) CreateBoundingBox(double minLat, double maxLat, double minLon, double maxLon)
```

Creates a bounding box tuple from explicit boundary values. Intended for tests that need to define a rectangular geographic area.

**Parameters:**
- `minLat` — Southern boundary latitude.
- `maxLat` — Northern boundary latitude.
- `minLon` — Western boundary longitude.
- `maxLon` — Eastern boundary longitude.

**Returns:** A value tuple `(double minLat, double maxLat, double minLon, double maxLon)` containing the supplied boundaries.

**Throws:** No exceptions are thrown; the method does not validate that `minLat <= maxLat` or `minLon <= maxLon`.

---

### CalculateMidpoint

```csharp
public static (double midpointLat, double midpointLon) CalculateMidpoint(double lat1, double lon1, double lat2, double lon2)
```

Computes the geographic midpoint between two coordinates using a simple arithmetic mean of their latitudes and longitudes. This is a planar approximation suitable for test scenarios where high geodesic accuracy is not required.

**Parameters:**
- `lat1`, `lon1` — Latitude and longitude of the first point.
- `lat2`, `lon2` — Latitude and longitude of the second point.

**Returns:** A value tuple `(double midpointLat, double midpointLon)` representing the averaged latitude and longitude.

**Throws:** No exceptions are thrown. Inputs are not validated for range; the arithmetic mean is computed unconditionally.

---

### IsApproximatelyValidCoordinate

```csharp
public static bool IsApproximatelyValidCoordinate(double lat, double lon)
```

Determines whether the given latitude and longitude fall within the standard geographic coordinate ranges, allowing for a small tolerance beyond the strict boundaries. This is used in tests to loosely validate that a computed coordinate is not wildly out of bounds.

**Parameters:**
- `lat` — Latitude to check.
- `lon` — Longitude to check.

**Returns:** `true` if the latitude is approximately within `[-90, 90]` and the longitude is approximately within `[-180, 180]`, accounting for a tolerance; otherwise `false`.

**Throws:** No exceptions are thrown.

---

### ShouldBeApproximately

```csharp
public static void ShouldBeApproximately(double actual, double expected, double tolerance = 1e-6)
```

A custom assertion that verifies the `actual` value is within `tolerance` of the `expected` value. Designed for floating-point coordinate comparisons in test code where exact equality is unreliable.

**Parameters:**
- `actual` — The computed value to check.
- `expected` — The reference value.
- `tolerance` — The maximum allowed absolute difference. Defaults to `1e-6`.

**Returns:** Void. The method throws if the assertion fails.

**Throws:** Throws an exception (typically an assertion exception such as `AssertionException` or equivalent) when `Math.Abs(actual - expected) > tolerance`.

## Usage

### Example 1: Validating a Coordinate Computation

```csharp
// Arrange
var start = GpsUtilitiesTestsExtensions.CreateCoordinate(47.0, 8.0);
var end   = GpsUtilitiesTestsExtensions.CreateCoordinate(47.5, 8.5);

// Act
var midpoint = GpsUtilitiesTestsExtensions.CalculateMidpoint(start.lat, start.lon, end.lat, end.lon);

// Assert
GpsUtilitiesTestsExtensions.ShouldBeApproximately(midpoint.midpointLat, 47.25);
GpsUtilitiesTestsExtensions.ShouldBeApproximately(midpoint.midpointLon, 8.25);
Assert.True(GpsUtilitiesTestsExtensions.IsApproximatelyValidCoordinate(midpoint.midpointLat, midpoint.midpointLon));
```

### Example 2: Constructing and Checking a Bounding Box

```csharp
// Arrange
var bbox = GpsUtilitiesTestsExtensions.CreateBoundingBox(
    minLat: 45.0, maxLat: 46.0,
    minLon: 7.0,  maxLon: 8.0
);

// Act — simulate a coordinate that should lie inside the box
var insidePoint = GpsUtilitiesTestsExtensions.CreateCoordinate(45.5, 7.5);

// Assert
Assert.True(insidePoint.lat >= bbox.minLat && insidePoint.lat <= bbox.maxLat);
Assert.True(insidePoint.lon >= bbox.minLon && insidePoint.lon <= bbox.maxLon);
```

## Notes

- **No input validation:** `CreateCoordinate`, `CreateBoundingBox`, and `CalculateMidpoint` accept any `double` values without range checks. Callers must ensure meaningful inputs, especially when using `CalculateMidpoint` across the antimeridian or poles, where the arithmetic mean may produce a coordinate that is not the true geodesic midpoint.
- **Planar midpoint approximation:** `CalculateMidpoint` does not account for Earth curvature or spherical geometry. It is suitable for small distances in test data but should not be used where high accuracy is required.
- **Tolerance in `IsApproximatelyValidCoordinate`:** The exact tolerance value is implementation-defined. It is intended to catch grossly invalid results (e.g., latitudes of several hundred degrees) while permitting minor boundary overshoots from rounding.
- **Default tolerance in `ShouldBeApproximately`:** The default `1e-6` corresponds to roughly 0.11 meters in latitude, which is adequate for most GPS test comparisons. Callers can supply a coarser or finer tolerance as needed.
- **Thread safety:** All members are static and operate only on their input parameters without shared mutable state. They are safe to call concurrently from multiple threads.
