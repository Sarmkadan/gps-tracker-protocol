# GpsFrameExtensions
The `GpsFrameExtensions` class provides a set of extension methods for working with GPS frames, allowing developers to easily parse and extract relevant information from GPS data. These methods can be used to simplify the process of handling GPS frames and reduce the amount of boilerplate code required.

## API
* `public static DateTime? ParseTimestamp`: Attempts to parse the timestamp from a GPS frame. Returns a `DateTime?` representing the parsed timestamp, or `null` if the timestamp could not be parsed.
* `public static string? GetDeviceId`: Retrieves the device ID from a GPS frame. Returns a `string?` representing the device ID, or `null` if the device ID could not be retrieved.
* `public static int? GetSignalStrength`: Retrieves the signal strength from a GPS frame. Returns an `int?` representing the signal strength, or `null` if the signal strength could not be retrieved.
* `public static string ToDiagnosticString`: Converts a GPS frame to a diagnostic string. Returns a `string` representing the diagnostic information.

## Usage
The following examples demonstrate how to use the `GpsFrameExtensions` class:
```csharp
// Example 1: Parsing a GPS frame
GpsFrame frame = new GpsFrame(...);
DateTime? timestamp = frame.ParseTimestamp();
if (timestamp.HasValue)
{
    Console.WriteLine($"Timestamp: {timestamp.Value}");
}
else
{
    Console.WriteLine("Failed to parse timestamp");
}

// Example 2: Retrieving device ID and signal strength
GpsFrame frame2 = new GpsFrame(...);
string? deviceId = frame2.GetDeviceId();
int? signalStrength = frame2.GetSignalStrength();
Console.WriteLine($"Device ID: {deviceId}, Signal Strength: {signalStrength}");
```

## Notes
When using the `GpsFrameExtensions` class, note that the `ParseTimestamp`, `GetDeviceId`, and `GetSignalStrength` methods may return `null` if the corresponding data is not available or could not be parsed. Additionally, these methods do not throw exceptions, so it is the responsibility of the caller to handle the possibility of `null` return values. The `ToDiagnosticString` method does not throw exceptions and always returns a string. The `GpsFrameExtensions` class is thread-safe, as it only contains static methods that do not modify any shared state. However, the thread-safety of the `GpsFrame` class itself is not guaranteed by the `GpsFrameExtensions` class, and should be evaluated separately.
