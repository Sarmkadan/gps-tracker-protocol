// ... (rest of README.md remains the same)

## DomainAndServiceTestsExtensions

The `DomainAndServiceTestsExtensions` class provides a set of extension methods to create test entities with realistic default values for testing. These methods validate inputs and generate properly initialized objects suitable for unit testing.

Here's an example usage:

```csharp
using GpsTrackerProtocol.Tests;

public class MyTest
{
    public void TestCreateValidLocation()
    {
        // Arrange
        var _ = new object(); // Unused parameter for extension method syntax

        // Act
        var location = _.CreateValidLocation();

        // Assert
        Assert.NotNull(location);
        Assert.IsType<LocationData>(location);
        // Additional assertions on location properties...
    }

    public void TestCreateValidDevice()
    {
        var _ = new object();
        var device = _.CreateValidDevice();
        Assert.NotNull(device);
        // Additional assertions on device properties...
    }

    public void TestCreateDeviceWithImei(string imei)
    {
        var _ = new object();
        var device = _.CreateDeviceWithImei(imei);
        Assert.NotNull(device);
        // Additional assertions on device properties...
    }

    public void TestCreateValidGpsFrame()
    {
        var _ = new object();
        var frame = _.CreateValidGpsFrame();
        Assert.NotNull(frame);
        // Additional assertions on frame properties...
    }

    public void TestCreateOfflineDevice(TimeSpan staleThreshold)
    {
        var _ = new object();
        var device = _.CreateOfflineDevice(staleThreshold);
        Assert.NotNull(device);
        // Additional assertions on device properties...
    }

    public void TestCreateInvalidLocation(double latitude, double longitude)
    {
        var _ = new object();
        var location = _.CreateInvalidLocation(latitude, longitude);
        Assert.NotNull(location);
        // Additional assertions on location properties...
    }

    public void TestCreateLocationWithBearing(double bearing)
    {
        var _ = new object();
        var location = _.CreateLocationWithBearing(bearing);
        Assert.NotNull(location);
        // Additional assertions on location properties...
    }

    public void TestCreateLocationWithSpeed(double speed)
    {
        var _ = new object();
        var location = _.CreateLocationWithSpeed(speed);
        Assert.NotNull(location);
        // Additional assertions on location properties...
    }

    public void TestCreateDeviceWithNetworkInfo(string ipAddress, int port)
    {
        var _ = new object();
        var device = _.CreateDeviceWithNetworkInfo(ipAddress, port);
        Assert.NotNull(device);
        // Additional assertions on device properties...
    }
}
```