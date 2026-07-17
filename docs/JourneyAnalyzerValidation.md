# JourneyAnalyzerValidation

`JourneyAnalyzerValidation` provides static methods for validating journey data structures in the GPS tracker protocol. It ensures that journey data conforms to expected formats and constraints before processing, offering both validation checks and exception-based enforcement mechanisms.

## API

### Validate

#### `Validate(Journey journey)`
Validates the provided `Journey` object for structural and semantic correctness.

- **Parameters**: `journey` - The journey to validate.
- **Returns**: `IReadOnlyList<string>` containing error messages for any validation failures. Returns an empty list if valid.
- **Throws**: `ArgumentNullException` if `journey` is null.

#### `Validate(IEnumerable<Location> locations)`
Validates a sequence of GPS locations for consistency and validity.

- **Parameters**: `locations` - The GPS locations to validate.
- **Returns**: `IReadOnlyList<string>` containing error messages for invalid or inconsistent location data.
- **Throws**: `ArgumentNullException` if `locations` is null.

#### `Validate(string journeyId)`
Validates a journey identifier for format and existence.

- **Parameters**: `journeyId` - The journey identifier to validate.
- **Returns**: `IReadOnlyList<string>` containing error messages if the identifier is invalid or not found.
- **Throws**: `ArgumentNullException` if `journeyId` is null.

### IsValid

#### `IsValid(Journey journey)`
Checks whether the provided `Journey` object is valid.

- **Parameters**: `journey` - The journey to check.
- **Returns**: `true` if the journey passes all validation rules; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `journey` is null.

#### `IsValid(IEnumerable<Location> locations)`
Checks whether the provided GPS locations are valid.

- **Parameters**: `locations` - The GPS locations to check.
- **Returns**: `true` if all locations are valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `locations` is null.

#### `IsValid(string journeyId)`
Checks whether the provided journey identifier is valid.

- **Parameters**: `journeyId` - The journey identifier to check.
- **Returns**: `true` if the identifier is valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `journeyId` is null.

### EnsureValid

#### `EnsureValid(Journey journey)`
Throws an exception if the provided `Journey` object is invalid.

- **Parameters**: `journey` - The journey to validate.
- **Returns**: `void`.
- **Throws**: `ValidationException` if validation fails. `ArgumentNullException` if `journey` is null.

#### `EnsureValid(IEnumerable<Location> locations)`
Throws an exception if the provided GPS locations are invalid.

- **Parameters**: `locations` - The GPS locations to validate.
- **Returns**: `void`.
- **Throws**: `ValidationException` if any location is invalid. `ArgumentNullException` if `locations` is null.

#### `EnsureValid(string journeyId)`
Throws an exception if the provided journey identifier is invalid.

- **Parameters**: `journeyId` - The journey identifier to validate.
- **Returns**: `void`.
- **Throws**: `ValidationException` if the identifier is invalid. `ArgumentNullException` if `journeyId` is null.

## Usage

### Validate and Handle Errors
```csharp
var journey = new Journey { Id = "TRIP123", Locations = GetGpsPoints() };
var errors = JourneyAnalyzerValidation.Validate(journey);

if (errors.Any())
{
    Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
}
else
{
    Console.WriteLine("Journey is valid.");
}
```

### Enforce Validity with Exception Handling
```csharp
try
{
    JourneyAnalyzerValidation.EnsureValid(GetJourneyId());
    ProcessJourney(GetJourneyId());
}
catch (ValidationException ex)
{
    Log.Error($"Invalid journey ID: {ex.Message}");
}
```

## Notes

- All methods are static and do not maintain state, making them inherently thread-safe for concurrent use.
- `Validate` methods return empty lists for valid inputs, allowing callers to distinguish between valid and invalid states without exception handling.
- `EnsureValid` methods throw `ValidationException` for invalid inputs, suitable for fail-fast scenarios.
- Null arguments passed to any method will result in `ArgumentNullException`, ensuring explicit error handling for missing data.
- Validation rules may include checks for chronological order of locations, coordinate validity, and journey ID format compliance.
