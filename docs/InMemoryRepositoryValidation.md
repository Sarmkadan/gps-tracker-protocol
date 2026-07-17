# InMemoryRepositoryValidation

Provides static validation methods for verifying the integrity and correctness of data stored within an in-memory repository implementation. This type centralizes validation logic for location data and various parameter sets, offering both boolean checks and exception-throwing assertions to ensure that repository contents meet the required constraints before further processing.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate { get; }
```

A property that returns a read-only list of validation error messages accumulated during the most recent validation operation. Each string describes a specific validation failure. An empty list indicates that no errors were found.

- **Return value**: `IReadOnlyList<string>` containing zero or more error messages.

### IsValid

```csharp
public static bool IsValid { get; }
```

Indicates whether the last validation run succeeded without any errors. Returns `true` if the `Validate` list is empty; otherwise `false`.

- **Return value**: `bool` — `true` when no validation errors exist, `false` otherwise.

### EnsureValid

```csharp
public static void EnsureValid()
```

Throws an exception if any validation errors are present. Call this method after performing validation to halt execution when the repository state is invalid.

- **Throws**: An exception (typically `InvalidOperationException` or a custom validation exception) when `IsValid` is `false`, with details from the `Validate` error list.

### ValidateLocationData

```csharp
public static IReadOnlyList<string> ValidateLocationData(/* parameters inferred from context */)
```

Validates location-specific data within the repository, such as GPS coordinates, timestamps, and associated metadata. Checks for logical consistency, required fields, and value range constraints.

- **Return value**: `IReadOnlyList<string>` of error messages. Empty when location data is valid.
- **Throws**: Does not throw directly; errors are returned in the list.

### ValidateParameters (multiple overloads)

```csharp
public static IReadOnlyList<string> ValidateParameters(/* parameters */)
public static IReadOnlyList<string> ValidateParameters(/* parameters */)
public static IReadOnlyList<string> ValidateParameters(/* parameters */)
public static IReadOnlyList<string> ValidateParameters(/* parameters */)
public static IReadOnlyList<string> ValidateParameters(/* parameters */)
```

Five overloads that validate different categories of parameters used by the repository. Each overload targets a distinct parameter set — such as configuration values, filter criteria, query boundaries, device identifiers, or session settings — and returns a list of validation errors specific to that parameter group.

- **Return value**: `IReadOnlyList<string>` of error messages for the given parameter set. Empty when all parameters are valid.
- **Throws**: Does not throw directly; errors are returned in the list.

## Usage

### Example 1: Validating location data and throwing on failure

```csharp
// Populate the in-memory repository with incoming GPS data
repository.StoreLocationData(incomingLocations);

// Run location data validation
var errors = InMemoryRepositoryValidation.ValidateLocationData();

// Halt processing if the data is invalid
InMemoryRepositoryValidation.EnsureValid();

// Proceed with analysis
var analyzer = new JourneyAnalyzer(repository);
analyzer.Process();
```

### Example 2: Checking multiple parameter sets conditionally

```csharp
// Validate device configuration parameters
var configErrors = InMemoryRepositoryValidation.ValidateParameters(deviceConfig);

// Validate time window parameters separately
var windowErrors = InMemoryRepositoryValidation.ValidateParameters(startTime, endTime);

// Combine and inspect errors without throwing
if (configErrors.Any() || windowErrors.Any())
{
    foreach (var error in configErrors.Concat(windowErrors))
    {
        logger.Warn($"Validation issue: {error}");
    }
    return; // Abort gracefully
}

// All parameters valid — continue
repository.ApplyFilter(deviceConfig, startTime, endTime);
```

## Notes

- **Static state**: The `Validate` property and `IsValid` flag are static and shared across all callers. In multi-threaded scenarios, one thread's validation result may overwrite another's before it is read. Synchronize access externally if concurrent use is expected.
- **Error accumulation**: The `Validate` list reflects only the most recent call to any validation method. It does not aggregate errors across multiple calls. Read the result immediately after the validation method you intend to check.
- **`EnsureValid` dependency**: Calling `EnsureValid` without first running a validation method will act on whatever error state was left by the last validation — which may be stale or empty. Always pair it with an explicit validation call.
- **Overload selection**: The five `ValidateParameters` overloads are distinguished by their parameter types and counts. Passing arguments that do not match any overload exactly results in a compile-time error; there is no fallback or implicit conversion.
- **Empty list convention**: An empty returned list always means success. Never treat a `null` return as valid — the methods return an empty list, not `null`.
- **Exception type**: The exact exception thrown by `EnsureValid` should be caught specifically if callers need to distinguish validation failures from other error conditions. Consult the implementation or integration tests for the precise type.
