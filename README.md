// entire file content ...
// ... goes in between

## GpsUtilitiesTests

The `GpsUtilitiesTests` class provides a comprehensive set of unit tests for the `GpsUtilities` class, covering various methods such as calculating distances, bearings, and coordinates, as well as converting between different units. These tests ensure that the `GpsUtilities` class behaves correctly and accurately.

Example usage in a test project:

```csharp
using Xunit;
using GpsTrackerProtocol.Utilities;

public class GpsUtilitiesTestsExample
{
    [Fact]
    public void TestCalculateDistanceKm()
    {
        // Arrange
        var lat1 = 51.5074;
        var lon1 = -0.1278;
        var lat2 = 51.5074;
        var lon2 = -0.1278;

        // Act
        var distance = GpsUtilities.CalculateDistanceKm(lat1, lon1, lat2, lon2);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void TestIsValidCoordinate()
    {
        // Arrange
        var lat = 90;
        var lon = 180;

        // Act
        var isValid = GpsUtilities.IsValidCoordinate(lat, lon);

        // Assert
        isValid.Should().BeTrue();
    }
}
```

// ... rest of README.md content ...
