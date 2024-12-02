# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## GpsUtilitiesTestsValidation

The `GpsUtilitiesTestsValidation` class provides a set of utility methods for validating GPS-related data. It includes methods for checking the validity of coordinates, distance, bearing, speed, zoom level, and bounding box coordinates. These methods can be used to ensure that GPS data is correct and consistent.

## GpsUtilitiesTestsExtensions

The `GpsUtilitiesTestsExtensions` class provides a set of extension methods that simplify common GPS-related assertions and calculations in unit tests. It offers helpers for creating bounding boxes, validating coordinate ranges, comparing coordinates with tolerance, computing midpoints, and checking approximate validity of coordinates.

Example usage:

```csharp
using GpsTrackerProtocol.Tests;

public class Example
{
    public void Demo()
    {
        var fixture = new GpsUtilitiesTests();

        // Create a bounding box around San Francisco
        var box = fixture.CreateBoundingBox(37.7749, -122.4194, 0.5, 0.5);
        Console.WriteLine($"Box: {box.minLat}..{box.maxLat}, {box.minLon}..{box.maxLon}");

        // Validate coordinates
        var coord = fixture.CreateCoordinate(37.7749, -122.4194);
        fixture.ShouldBeApproximately(coord.lat, coord.lon, 37.7749, -122.4194);

        // Calculate midpoint
        var mid = fixture.CalculateMidpoint(37.7749, -122.4194, 37.7849, -122.4094);
        Console.WriteLine($"Midpoint: {mid.midpointLat}, {mid.midpointLon}");

        // Check validity
        bool isValid = fixture.IsApproximatelyValidCoordinate(coord.lat, coord.lon);
        Console.WriteLine($"Coordinate valid: {isValid}");
    }
}
```
