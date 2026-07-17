# StringExtensionsValidation

A utility class that provides validation helpers for string parsing and device identification in the GPS tracker protocol library. It validates parsed numeric values from NMEA sentences, device IDs, IMEI numbers, hexadecimal strings, and color codes, offering both diagnostic and exception-throwing validation methods.

## API

### IReadOnlyList<string> Validate(double parsedValue, string originalString, double defaultValue = 0)

Validates a parsed double value against the original string that was parsed.

- **parsedValue**: The parsed double value to validate.
- **originalString**: The original string that was parsed. Must not be null.
- **defaultValue**: The default value used when parsing failed. Defaults to 0.
- **Returns**: A list of validation problems; empty list if the value is valid.
- **Throws**: `ArgumentNullException` if `originalString` is null.

### IReadOnlyList<string> Validate(int parsedValue, string originalString, int defaultValue = 0)

Validates a parsed integer value against the original string that was parsed.

- **parsedValue**: The parsed integer value to validate.
- **originalString**: The original string that was parsed. Must not be null.
- **defaultValue**: The default value used when parsing failed. Defaults to 0.
- **Returns**: A list of validation problems; empty list if the value is valid.
- **Throws**: `ArgumentNullException` if `originalString` is null.

### IReadOnlyList<string> ValidateImei(string imeiValue)

Validates an IMEI string format.

- **imeiValue**: The IMEI string to validate. Must not be null.
- **Returns**: A list of validation problems; empty list if the IMEI is valid.
- **Throws**: `ArgumentNullException` if `imeiValue` is null.

### IReadOnlyList<string> ValidateDeviceId(string deviceIdValue)

Validates a device ID string format.

- **deviceIdValue**: The device ID string to validate. Must not be null.
- **Returns**: A list of validation problems; empty list if the device ID is valid.
- **Throws**: `ArgumentNullException` if `deviceIdValue` is null.

### IReadOnlyList<string> Validate(int maxLength, string suffix = "...")

Validates truncation parameters for string truncation operations.

- **maxLength**: The maximum length for truncation. Must be greater than zero.
- **suffix**: The suffix to append when truncating. Defaults to `"..."`. Must not be null, empty, or whitespace.
- **Returns**: A list of validation problems; empty list if the parameters are valid.
- **Throws**: `ArgumentNullException` if `suffix` is null.

### IReadOnlyList<string> ValidateHex(string hexValue)

Validates a hexadecimal string for conversion to a byte array.

- **hexValue**: The hex string to validate. Must not be null.
- **Returns**: A list of validation problems; empty list if the hex string is valid.
- **Throws**: `ArgumentNullException` if `hexValue` is null.

### IReadOnlyList<string> ValidateDeviceIdForSanitization(string deviceId)

Validates a device ID string for sanitization purposes. Equivalent to `ValidateDeviceId`.

- **deviceId**: The device ID string to validate. Must not be null.
- **Returns**: A list of validation problems; empty list if the device ID is valid.
- **Throws**: `ArgumentNullException` if `deviceId` is null.

### IReadOnlyList<string> ValidateColor(string colorValue)

Validates a hexadecimal color string.

- **colorValue**: The hex color string to validate. Must not be null.
- **Returns**: A list of validation problems; empty list if the color is valid.
- **Throws**: `ArgumentNullException` if `colorValue` is null.

### bool IsValid(double parsedValue, string originalString, double defaultValue = 0)

Determines whether the parsed double value is valid.

- **parsedValue**: The parsed double value.
- **originalString**: The original string that was parsed.
- **defaultValue**: The default value used when parsing failed. Defaults to 0.
- **Returns**: `true` if the value is valid; otherwise, `false`.

### bool IsValid(int parsedValue, string originalString, int defaultValue = 0)

Determines whether the parsed integer value is valid.

- **parsedValue**: The parsed integer value.
- **originalString**: The original string that was parsed.
- **defaultValue**: The default value used when parsing failed. Defaults to 0.
- **Returns**: `true` if the value is valid; otherwise, `false`.

### bool IsValidImei(string imeiValue)

Determines whether the IMEI string is valid.

- **imeiValue**: The IMEI string to validate.
- **Returns**: `true` if the IMEI is valid; otherwise, `false`.


### bool IsValidDeviceId(string deviceIdValue)

Determines whether the device ID string is valid.

- **deviceIdValue**: The device ID string to validate.
- **Returns**: `true` if the device ID is valid; otherwise, `false`.


### bool IsValid(int maxLength, string suffix = "...")

Determines whether the truncation parameters are valid.

- **maxLength**: The maximum length for truncation.
- **suffix**: The suffix to append when truncating. Defaults to `"..."`.
- **Returns**: `true` if the parameters are valid; otherwise, `false`.


### bool IsValidHex(string hexValue)

Determines whether the hex string is valid for conversion.

- **hexValue**: The hex string to validate.
- **Returns**: `true` if the hex string is valid; otherwise, `false`.


### bool IsValidColor(string colorValue)

Determines whether the color string is valid.

- **colorValue**: The hex color string to validate.
- **Returns**: `true` if the color is valid; otherwise, `false`.


### void EnsureValid(double parsedValue, string originalString, double defaultValue = 0)

Ensures that the parsed double value is valid, throwing an exception if not.

- **parsedValue**: The parsed double value.
- **originalString**: The original string that was parsed.
- **defaultValue**: The default value used when parsing failed. Defaults to 0.
- **Throws**: `ArgumentException` if the value is invalid, with a message listing all validation problems.

### void EnsureValid(int parsedValue, string originalString, int defaultValue = 0)

Ensures that the parsed integer value is valid, throwing an exception if not.

- **parsedValue**: The parsed integer value.
- **originalString**: The original string that was parsed.
- **defaultValue**: The default value used when parsing failed. Defaults to 0.
- **Throws**: `ArgumentException` if the value is invalid, with a message listing all validation problems.

### void EnsureValidImei(string imeiValue)

Ensures that the IMEI string is valid, throwing an exception if not.

- **imeiValue**: The IMEI string to validate.
- **Throws**: `ArgumentException` if the IMEI is invalid, with a message listing all validation problems.

### void EnsureValidDeviceId(string deviceIdValue)

Ensures that the device ID string is valid, throwing an exception if not.

- **deviceIdValue**: The device ID string to validate.
- **Throws**: `ArgumentException` if the device ID is invalid, with a message listing all validation problems.

### void EnsureValid(int maxLength, string suffix = "...")

Ensures that the truncation parameters are valid, throwing an exception if not.

- **maxLength**: The maximum length for truncation.
- **suffix**: The suffix to append when truncating. Defaults to `"..."`.
- **Throws**: `ArgumentException` if the parameters are invalid, with a message listing all validation problems.

### void EnsureValidHex(string hexValue)

Ensures that the hex string is valid for conversion, throwing an exception if not.

- **hexValue**: The hex string to validate.
- **Throws**: `ArgumentException` if the hex string is invalid, with a message listing all validation problems.

### void EnsureValidColor(string colorValue)

Ensures that the color string is valid, throwing an exception if not.

- **colorValue**: The hex color string to validate.
- **Throws**: `ArgumentException` if the color is invalid, with a message listing all validation problems.

## Usage

```csharp
// Example 1: Validating parsed NMEA values
var latitudeString = "40.7128";
var latitudeParsed = latitudeString.ParseDouble();

var latitudeProblems = StringExtensionsValidation.Validate(latitudeParsed, latitudeString, double.NaN);
if (latitudeProblems.Count > 0)
{
    Console.WriteLine("Latitude parsing issues:");
    foreach (var problem in latitudeProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}

// Example 2: Validating device IDs before use
var deviceId = "TRACKER-DEV-001";
var deviceIdProblems = StringExtensionsValidation.ValidateDeviceId(deviceId);

if (deviceIdProblems.Count == 0)
{
    Console.WriteLine($"Device ID '{deviceId}' is valid for use");
}
else
{
    Console.WriteLine("Device ID validation failed:");
    foreach (var problem in deviceIdProblems)
    {
        Console.WriteLine($"- {problem}");
    }
}
```

## Notes

- All validation methods are thread-safe as they operate on local state only.
- Methods that accept `originalString` treat it as required input; passing null throws `ArgumentNullException` immediately.
- The `Validate` overloads for parsed values check whether the parsed value equals the default value when the original string is non-empty, indicating a potential parsing failure.
- Device ID validation allows alphanumeric characters, dashes (`-`), and underscores (`_`), with a maximum length of 50 characters.
- IMEI validation accepts strings between 15 and 20 digits, enforcing digit-only content.
- Hexadecimal validation removes common separators (hyphens and spaces) before checking for an even number of characters.
- Color validation supports both 6-character (RGB) and 8-character (RGBA) hexadecimal formats, with or without a leading `#`.
- The `EnsureValid*` methods provide a convenient way to validate and throw in one operation, useful for guard clauses in public APIs.