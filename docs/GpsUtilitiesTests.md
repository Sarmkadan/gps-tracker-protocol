# GpsUtilitiesTests

`GpsUtilitiesTests` is the unit test suite for the `GpsUtilities` static helper class within the `gps-tracker-protocol` project. It validates the correctness of geospatial calculations, coordinate conversions, unit transformations, and bounding-box logic. The class exercises both nominal paths and boundary conditions, ensuring that invalid inputs are handled gracefully and that mathematical results fall within expected tolerances.

## API

### `CalculateDistanceKm_SameCoordinates_ReturnsZero`
Verifies that the distance between two identical geographic points is exactly zero kilometres.  
**Parameters:** None (self-contained test).  
**Returns:** `void` – asserts equality.  
**Throws:** Never throws directly; assertion failures propagate as test-framework exceptions.

### `CalculateDistanceKm_OneDegreeLat_ReturnsApproximatelyOneHundredEleven`
Confirms that moving one degree of latitude yields a distance close to 111 km, the standard approximation.  
**Parameters:** None.  
**Returns:** `void` – asserts approximate equality.  
**Throws:** Never throws directly.

### `CalculateDistanceKm_InvalidCoordinates_ReturnsZero`
Ensures that passing out-of-range latitude or longitude values (e.g., >90° lat, >180° lon) results in a zero-distance return rather than an exception.  
**Parameters:** None.  
**Returns:** `void` – asserts zero.  
**Throws:** Never throws directly.

### `IsValidCoordinate_VariousInputs_ReturnsExpectedResult`
Exercises the coordinate-validation predicate with a matrix of valid and invalid latitude/longitude pairs, confirming the boolean outcome matches the expected classification.  
**Parameters:** None.  
**Returns:** `void` – asserts true/false per case.  
**Throws:** Never throws directly.

### `CalculateBearing_DueNorth_ReturnsZeroDegrees`
Checks that the initial bearing from a point to another point directly north of it is 0°.  
**Parameters:** None.  
**Returns:** `void` – asserts zero.  
**Throws:** Never throws directly.

### `CalculateBearing_DueEast_ReturnsNinetyDegrees`
Checks that the initial bearing from a point to another point directly east of it is 90°.  
**Parameters:** None.  
**Returns:** `void` – asserts 90.  
**Throws:** Never throws directly.

### `CalculateBearing_InvalidCoordinates_ReturnsZero`
Validates that bearing calculation falls back to 0° when either coordinate pair is invalid.  
**Parameters:** None.  
**Returns:** `void` – asserts zero.  
**Throws:** Never throws directly.

### `DmsToDecimal_NorthernLatitude_ReturnsPositiveDecimal`
Tests conversion of a degrees-minutes-seconds string with a Northern-hemisphere suffix (e.g., `N`) to a positive decimal-degree value.  
**Parameters:** None.  
**Returns:** `void` – asserts positive decimal.  
**Throws:** Never throws directly.

### `DmsToDecimal_SouthernLatitude_ReturnsNegativeDecimal`
Tests conversion of a DMS string with a Southern-hemisphere suffix (`S`) to a negative decimal-degree value.  
**Parameters:** None.  
**Returns:** `void` – asserts negative decimal.  
**Throws:** Never throws directly.

### `DmsToDecimal_WesternLongitude_ReturnsNegativeDecimal`
Tests conversion of a DMS string with a Western-hemisphere suffix (`W`) to a negative decimal-degree value.  
**Parameters:** None.  
**Returns:** `void` – asserts negative decimal.  
**Throws:** Never throws directly.

### `KnotsToKmh_OneKnot_ReturnsOnePointEightFiveTwo`
Confirms that 1 knot converts to exactly 1.852 km/h.  
**Parameters:** None.  
**Returns:** `void` – asserts 1.852.  
**Throws:** Never throws directly.

### `KmhToKnots_OnePointEightFiveTwoKmh_ReturnsOneKnot`
Confirms the inverse conversion: 1.852 km/h yields 1 knot.  
**Parameters:** None.  
**Returns:** `void` – asserts 1.0.  
**Throws:** Never throws directly.

### `KmhToMs_ThreePointSixKmh_ReturnsOneMs`
Verifies that 3.6 km/h converts to exactly 1 m/s.  
**Parameters:** None.  
**Returns:** `void` – asserts 1.0.  
**Throws:** Never throws directly.

### `IsWithinBounds_PointInsideBoundingBox_ReturnsTrue`
Asserts that a point whose latitude and longitude fall strictly inside a defined bounding box returns `true`.  
**Parameters:** None.  
**Returns:** `void` – asserts true.  
**Throws:** Never throws directly.

### `IsWithinBounds_PointOutsideBoundingBox_ReturnsFalse`
Asserts that a point outside the bounding box (by latitude, longitude, or both) returns `false`.  
**Parameters:** None.  
**Returns:** `void` – asserts false.  
**Throws:** Never throws directly.

### `GetBoundingBoxCenter_SymmetricBounds_ReturnsMidpoint`
Validates that the computed centre of a symmetric bounding box equals the arithmetic mean of the min/max latitude and longitude.  
**Parameters:** None.  
**Returns:** `void` – asserts midpoint coordinates.  
**Throws:** Never throws directly.

### `CalculateZoomLevel_ZeroBoundingBox_ReturnsMaxZoom`
Ensures that a degenerate (zero-area) bounding box causes the zoom-level calculation to return the maximum allowed zoom value rather than an error.  
**Parameters:** None.  
**Returns:** `void` – asserts max zoom constant.  
**Throws:** Never throws directly.

## Usage

```csharp
// Example 1: Running the full suite with an NUnit-compatible runner
[TestFixture]
public class GpsUtilitiesTestRunner
{
    [Test]
    public void RunAllGpsUtilitiesTests()
    {
        var tests = new GpsUtilitiesTests();

        tests.CalculateDistanceKm_SameCoordinates_ReturnsZero();
        tests.CalculateDistanceKm_OneDegreeLat_ReturnsApproximatelyOneHundredEleven();
        tests.CalculateDistanceKm_InvalidCoordinates_ReturnsZero();
        tests.IsValidCoordinate_VariousInputs_ReturnsExpectedResult();
        tests.CalculateBearing_DueNorth_ReturnsZeroDegrees();
        tests.CalculateBearing_DueEast_ReturnsNinetyDegrees();
        tests.CalculateBearing_InvalidCoordinates_ReturnsZero();
        tests.DmsToDecimal_NorthernLatitude_ReturnsPositiveDecimal();
        tests.DmsToDecimal_SouthernLatitude_ReturnsNegativeDecimal();
        tests.DmsToDecimal_WesternLongitude_ReturnsNegativeDecimal();
        tests.KnotsToKmh_OneKnot_ReturnsOnePointEightFiveTwo();
        tests.KmhToKnots_OnePointEightFiveTwoKmh_ReturnsOneKnot();
        tests.KmhToMs_ThreePointSixKmh_ReturnsOneMs();
        tests.IsWithinBounds_PointInsideBoundingBox_ReturnsTrue();
        tests.IsWithinBounds_PointOutsideBoundingBox_ReturnsFalse();
        tests.GetBoundingBoxCenter_SymmetricBounds_ReturnsMidpoint();
        tests.CalculateZoomLevel_ZeroBoundingBox_ReturnsMaxZoom();
    }
}
```

```csharp
// Example 2: Selective execution during CI pipeline – validating only coordinate conversion and unit helpers
var tests = new GpsUtilitiesTests();

// Quick sanity checks before integration tests
tests.DmsToDecimal_NorthernLatitude_ReturnsPositiveDecimal();
tests.DmsToDecimal_SouthernLatitude_ReturnsNegativeDecimal();
tests.DmsToDecimal_WesternLongitude_ReturnsNegativeDecimal();
tests.KnotsToKmh_OneKnot_ReturnsOnePointEightFiveTwo();
tests.KmhToKnots_OnePointEightFiveTwoKmh_ReturnsOneKnot();
tests.KmhToMs_ThreePointSixKmh_ReturnsOneMs();
```

## Notes

- **Edge cases:** Several tests explicitly target invalid coordinates (latitude outside ±90°, longitude outside ±180°) and degenerate bounding boxes. The expected behaviour is a safe fallback—zero distance, zero bearing, or maximum zoom—rather than an exception. This implies the underlying `GpsUtilities` methods perform input validation internally.
- **Floating-point tolerance:** Distance and bearing assertions use approximate equality (e.g., `Assert.AreEqual` with a delta) to accommodate floating-point imprecision inherent in trigonometric and spherical-earth formulas.
- **Thread safety:** The test methods are instance methods that do not share mutable state. They can be executed concurrently by test runners without risk of interference. The underlying `GpsUtilities` static methods are expected to be pure functions, making them inherently thread-safe.
- **Test isolation:** Each method is self-contained and does not depend on setup or teardown logic. The class can be instantiated and invoked directly without a fixture context.
- **Zoom-level boundary:** `CalculateZoomLevel_ZeroBoundingBox_ReturnsMaxZoom` guards against division-by-zero or logarithm-of-zero errors when the bounding box has no spatial extent.
