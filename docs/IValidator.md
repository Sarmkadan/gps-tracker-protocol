# IValidator

The `IValidator` interface defines the core contract for validating data structures within the `gps-tracker-protocol`. It provides a structured approach to inspecting and validating various entity types—including frames, locations, and devices—by executing them through a defined `ValidationPipeline`. The interface maintains the internal state of the validation process, allowing consumers to determine if a validation operation succeeded and to retrieve associated error details.

## API

### Properties

*   **`ValidationPipeline`**: Gets or sets the `ValidationPipeline` instance that contains the rules and logic for the validation process.
*   **`IsValid`**: A boolean property that returns `true` if the most recent validation operation resulted in no errors; otherwise, returns `false`.
*   **`Errors`**: A `List<string>` containing all error messages generated during the last validation operation.

### Methods

*   **`ValidateFrame`**: Performs validation on a provided frame object and returns a `ValidationResult`.
*   **`ValidateLocation`**: Performs validation on a provided location object and returns a `ValidationResult`.
*   **`ValidateDevice`**: Performs validation on a provided device object and returns a `ValidationResult`.
*   **`Validate`** (Overload 1): Performs a general validation operation and returns a `ValidationResult`.
*   **`Validate`** (Overload 2): Performs a general validation operation based on provided parameters and returns a `ValidationResult`.
*   **`Validate`** (Overload 3): Performs a general validation operation based on provided parameters and returns a `ValidationResult`.
*   **`GetErrorMessage`**: Returns a string representation of the error message associated with the current validation state or a specific error condition.

## Usage

### Example 1: Validating a GPS Frame
```csharp
public void ProcessIncomingData(IValidator validator, Frame frame)
{
    ValidationResult result = validator.ValidateFrame(frame);

    if (result.Success)
    {
        // Proceed with processing the validated frame
    }
    else
    {
        // Handle validation failure
        string message = validator.GetErrorMessage();
        Logger.LogWarning($"Frame validation failed: {message}");
    }
}
```

### Example 2: Checking Validator State
```csharp
public void ValidateBatch(IValidator validator, IEnumerable<object> items)
{
    foreach (var item in items)
    {
        validator.Validate(item);
        
        if (!validator.IsValid)
        {
            foreach (var error in validator.Errors)
            {
                Console.WriteLine($"Error: {error}");
            }
            // Break or handle as necessary for the specific protocol requirements
            break;
        }
    }
}
```

## Notes

*   **Thread Safety**: Implementations of `IValidator` are not guaranteed to be thread-safe. If the validator is used in a multi-threaded context (e.g., within a singleton service handling concurrent requests), it is recommended to either use a factory to create scoped validator instances or ensure the implementation provides thread-safe access to its internal state (specifically `Errors`, `IsValid`, and the `ValidationPipeline`).
*   **Null Inputs**: Passing null objects to `ValidateFrame`, `ValidateLocation`, or `ValidateDevice` may result in a `NullReferenceException` or a `ValidationResult` indicating failure, depending on the specific implementation. Always verify entity existence before calling validation methods.
*   **Empty Pipelines**: If the `ValidationPipeline` property is not configured or is empty, the `Validate` methods may return a successful `ValidationResult` by default, as there are no rules to evaluate.
