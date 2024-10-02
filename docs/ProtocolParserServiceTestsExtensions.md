# ProtocolParserServiceTestsExtensions

`ProtocolParserServiceTestsExtensions` provides a collection of static extension methods designed to streamline the unit testing of GPS protocol parsing services within the `gps-tracker-protocol` project. These utilities facilitate the rapid generation of test `GpsFrame` objects and provide specialized, reusable assertion logic for verifying the correct parsing of coordinates and timestamps from raw protocol data.

## API

### `CreateTestFrame` (Overload 1)
Creates a basic `GpsFrame` instance populated with default test data.
- **Returns**: A new `GpsFrame` instance initialized with default values.

### `CreateTestFrame` (Overload 2)
Creates a `GpsFrame` instance with specific payload data for focused testing scenarios.
- **Parameters**: `string data` - The raw protocol payload string.
- **Returns**: A new `GpsFrame` instance configured with the provided payload.

### `ShouldParseCoordinatesAsync`
Asserts that the provided `IProtocolParserService` correctly parses latitude and longitude from the given raw data.
- **Parameters**:
  - `IProtocolParserService parser` - The parser instance under test.
  - `string data` - The raw protocol payload.
  - `double expectedLat` - The expected latitude.
  - `double expectedLon` - The expected longitude.
- **Exceptions**: Throws an assertion exception if the parsed coordinates do not match the expected values.

### `ShouldParseTimestampAsync`
Asserts that the provided `IProtocolParserService` correctly parses the timestamp from the given raw data.
- **Parameters**:
  - `IProtocolParserService parser` - The parser instance under test.
  - `string data` - The raw protocol payload.
  - `DateTime expectedTimestamp` - The expected timestamp.
- **Exceptions**: Throws an assertion exception if the parsed timestamp does not match the expected value.

## Usage

### Creating a Test Frame and Asserting Coordinates
```csharp
[Fact]
public async Task TestParser_Coordinates()
{
    var parser = new GpsProtocolParser();
    var frame = ProtocolParserServiceTestsExtensions.CreateTestFrame("raw_payload_data");

    await ProtocolParserServiceTestsExtensions.ShouldParseCoordinatesAsync(parser, "raw_payload_data", 45.0, -93.0);
}
```

### Validating Timestamp Parsing
```csharp
[Fact]
public async Task TestParser_Timestamp()
{
    var parser = new GpsProtocolParser();
    var expectedDate = new DateTime(2026, 7, 10, 12, 0, 0);

    await ProtocolParserServiceTestsExtensions.ShouldParseTimestampAsync(parser, "raw_timestamp_payload", expectedDate);
}
```

## Notes

- **Thread Safety**: These extension methods are thread-safe, as they do not maintain internal state and rely on the thread-safety of the underlying `IProtocolParserService` implementation.
- **Edge Cases**: When using `CreateTestFrame`, ensure that any payload data provided adheres to the expected protocol format; passing null or malformed data may result in `ArgumentNullException` or `FormatException` depending on the underlying implementation.
- **Assertion Failures**: The assertion methods (`ShouldParseCoordinatesAsync` and `ShouldParseTimestampAsync`) are designed for use in standard unit test suites and will throw assertion exceptions upon failure.
