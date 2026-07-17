# DomainAndServiceTestsValidation

A static utility class providing validation functionality for domain and service test scenarios within the GPS tracker protocol implementation. This class offers methods to validate various types of input data and ensure their correctness before processing, with both query-based validation (`IsValid`, `Validate`) and assertion-based validation (`EnsureValid`) approaches.

## API

### Validate Methods

```csharp
public static IReadOnlyList<string> Validate(string input)
```
Validates a string input and returns a read-only list of validation error messages. Returns an empty list if the input is valid.

```csharp
public static IReadOnlyList<string> Validate(int value)
```
Validates an integer value and returns a read-only list of validation error messages. Returns an empty list if the value is valid.

```csharp
public static IReadOnlyList<string> Validate(DateTime dateTime)
```
Validates a DateTime value and returns a read-only list of validation error messages. Returns an empty list if the DateTime is valid.

```csharp
public static IReadOnlyList<string> Validate(GpsTrackerProtocolOptions options)
```
Validates GpsTrackerProtocolOptions and returns a read-only list of validation error messages. Returns an empty list if the options are valid.

### IsValid Methods

```csharp
public static bool IsValid(string input)
```
Determines whether a string input is valid. Returns `true` if valid, `false` otherwise.

```csharp
public static bool IsValid(int value)
```
Determines whether an integer value is valid. Returns `true` if valid, `false` otherwise.

```csharp
public static bool IsValid(DateTime dateTime)
```
Determines whether a DateTime value is valid. Returns `true` if valid, `false` otherwise.

```csharp
public static bool IsValid(GpsTrackerProtocolOptions options)
```
Determines whether GpsTrackerProtocolOptions are valid. Returns `true` if valid, `false` otherwise.

### EnsureValid Methods

```csharp
public static void EnsureValid(string input)
```
Validates a string input and throws an exception if invalid. No return value.

```csharp
public static void EnsureValid(int value)
```
Validates an integer value and throws an exception if invalid. No return value.

```csharp
public static void EnsureValid(DateTime dateTime)
```
Validates a DateTime value and throws an exception if invalid. No return value.

```csharp
public static void EnsureValid(GpsTrackerProtocolOptions options)
```
Validates GpsTrackerProtocolOptions and throws an exception if invalid. No return value.

## Usage

```csharp
// Example 1: Validating protocol options before configuration
var options = new GpsTrackerProtocolOptions 
{ 
    ServerPort = 8080,
    MaxBufferSize = 1024
};

if (DomainAndServiceTestsValidation.IsValid(options))
{
    Console.WriteLine("Options are valid, proceeding with configuration");
    // Proceed with tracker setup
}
else
{
    var errors = DomainAndServiceTestsValidation.Validate(options);
    Console.WriteLine($"Configuration errors: {string.Join(", ", errors)}");
}
```

```csharp
// Example 2: Ensuring data validity with exception handling
try
{
    string deviceId = "GPS_001";
    DateTime timestamp = DateTime.UtcNow;
    int signalStrength = 85;
    
    DomainAndServiceTestsValidation.EnsureValid(deviceId);
    DomainAndServiceTestsValidation.EnsureValid(timestamp);
    DomainAndServiceTestsValidation.EnsureValid(signalStrength);
    
    // Process validated data
    ProcessGpsData(deviceId, timestamp, signalStrength);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## Notes

- All methods are static and thread-safe for concurrent access
- `Validate` methods return empty collections rather than null for valid inputs
- `EnsureValid` methods throw `ArgumentException` or derived exceptions when validation fails
- DateTime validation considers both date/time values and timezone information
- Integer validation may include range checks specific to GPS protocol constraints
- String validation typically includes null/empty checks and format validation
- Validation rules are designed to match GPS tracker protocol specifications
- Performance considerations: `IsValid` methods are optimized for quick boolean checks, while `Validate` methods provide detailed error information at slightly higher cost
