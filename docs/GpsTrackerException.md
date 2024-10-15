# GpsTrackerException

The `GpsTrackerException` is the base exception type used throughout the **gps-tracker-protocol** library to signal errors that occur while parsing, validating, for protocol messages. It captures the raw protocol payload, protocol type, checksum information, and device/command context, and can contain nested exception types that provide more specific failure details (e.g., parsing, checksum, device, command, or validation errors).

## API

### Constructors

#### `GpsTrackerException(string message)`
Initializes a new instance of the `GpsTrackerException` class with a specified error message.  
- **Parameters**  
  - `message`: A string that describes the error.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

#### `GpsTrackerException()`
Initializes a new instance of the `GpsTrackerException` class with a default system‑supplied message.  
- **Parameters**  
  - None.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

### Properties

#### `string? RawData`
Gets or sets the raw protocol data that caused the exception.  
- **Return Value**  
  - A nullable string containing the raw data, or `null` if not available.

#### `ProtocolType Protocol`
Gets or sets the protocol type associated with the exception.  
- **Return Value**  
  - A value of the `ProtocolType` enumeration.

#### `ParseException ParseException`
Gets or sets a `ParseException` instance related to the error.  
- **Return Value**  
  - A `ParseException` object, or `null` if none.

#### `ParseException ParseException`
*(Duplicate member as appears in the source.)*  
Gets or sets a `ParseException` instance related to the error.  
- **Return Value**  
  - A `ParseException` object, or `null` if none.

#### `string? ExpectedChecksum`
Gets or sets the expected checksum value.  
- **Return Value**  
  - A nullable string containing the expected checksum, or `null` if not applicable.

#### `string? ActualChecksum`
Gets or sets the actual checksum value computed from the data.  
- **Return Value**Return Value**  
  - A nullable string containing the actual checksum, or `null` if not applicable.

#### `ChecksumException ChecksumException`
Gets or sets a `ChecksumException` instance related to the error.  
- **Return Value**  
  - A `ChecksumException` object, or `null` if none.

#### `string? DeviceId`
Gets or sets the identifier of the device that generated the problematic data.  
- **Return Value**  
  - A nullable string containing the device ID, or `null` if not available.

#### `string? CommandId`
Gets or sets the identifier of the command associated with the exception.  
- **Return Value**  
  - A nullable string containing the command ID, or `null` if not available.

#### `int ErrorCode`
Gets or sets the numeric error code associated with the exception.  
- **Return Value**  
  - An integer error code.

#### `string? FieldName`
Gets or sets the name of the field that failed validation.  
- **Return Value**  
  - A nullable string containing the field name, or `null` if not applicable.

#### `object? FieldValue`
Gets or sets the value of the field that caused the validation error.  
- **Return Value**  
  - An object representing the field value, or `null` if not applicable.

### Nested Exception Types

#### `DeviceException(string message)`
Constructor for the nested `DeviceException` class.  
- **Parameters**  
  - `message`: A string that describes the device‑related error.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

#### `DeviceException()`
Parameterless constructor`
Parameterless constructor for the nested `DeviceException` class.  
- **Parameters**  
  - None.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

#### `CommandException(string message)`
Constructor for the nested `CommandException` class.  
- **Parameters**  
  - `message`: A string that describes the command‑related error.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

#### `CommandException() constructor`
Parameterless constructor for the nested `CommandException` class.  
- **Parameters**  
  - None.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

#### `ValidationException(string message)`
Constructor for the nested `ValidationException` class.  
- **Parameters**  
  - `message`: A string that describes the validation‑related error.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

#### `ValidationException() constructor`
Parameterless constructor for the nested `ValidationException` class.  
- **Parameters**  
  - None.  
- **Return Value**  
  - None (constructor).  
- **Exceptions**  
  - None.

## Usage

### Example 1: Throwing a GpsTrackerException with protocol details

```csharp
using GpsTrackerProtocol;

public void ProcessMessage(string rawMessage, ProtocolType protocol)
{
    // Simulate a checksum mismatch
    string expected = "AB12";
    string actual   = "CD34";

    if (expected != actual)
    {
        var ex = new GpsTrackerException("Checksum validation failed.")
        {
            RawData       = rawMessage,
            Protocol      = protocol,
            ExpectedChecksum = expected,
            ActualChecksum   = actual
        };
        throw ex;
    }
    // normal processing...
}
```

### Example 2: Catching and inspecting a GpsTrackerException

```csharp
using GpsTrackerProtocol;

try
{
    // Code that may throw GpsTrackerException or its nested types
    ParseIncomingData();
}
catch (GpsTrackerException gex) when (gex.DeviceId != null)
{
    // Handle device‑specific problems
    Console.WriteLine($"Device {gex.DeviceId} caused an error.");
    if (gex.ParseException != null)
    {
        Console.WriteLine($"Parse error: {gex.ParseException.Message}");
    }
}
catch (GpsTrackerException gex)
{
    // General fallback
    Console.WriteLine($"GPS tracker error: {gex.Message}");
    Console.WriteLine($"Raw data: {gex.RawData}");
}
```

## Notes

- The `RawData`, `ExpectedChecksum`, `ActualChecksum`, `DeviceId`, `CommandId`, `FieldName`, and `FieldValue` properties are nullable; consumers should check for `null` before use.  
- The `Protocol` property defaults to the underlying default value of `ProtocolType` if not explicitly set.  
- Nested exception properties (`ParseException`, `ChecksumException`, etc.) may be `null` when the corresponding error condition is not present.  
- Exception objects are intended to be immutable after construction; however, because the class exposes settable properties, concurrent modification of the same instance from multiple threads could lead to race conditions. It is recommended to treat instances as immutable after they are thrown or to synchronize access if mutation is required after creation.  
- The duplicate `ParseException` member appears twice in the source metadata; both entries refer to the same conceptual property and should be considered redundant.  
- No members of this type throw exceptions during normal property access or construction; all validation is deferred to the consumer.
