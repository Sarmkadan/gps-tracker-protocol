# BatchDataImporterValidation

Provides static methods for validating data intended for batch import operations within the GPS tracker protocol. This type centralizes validation logic, offering both boolean checks and exception-throwing assertions to ensure imported data conforms to expected schemas and constraints before processing.

## API

### Validate Methods

```csharp
public static IReadOnlyList<string> Validate(byte[] data)
public static IReadOnlyList<string> Validate(string json)
public static IReadOnlyList<string> Validate(Stream stream)
```

**Purpose:**  
Performs a full validation pass on the supplied batch import data and returns a collection of human-readable error messages describing every detected violation. An empty list indicates the data is valid.

**Parameters:**  
- `data` (`byte[]`): Raw binary payload representing the batch import.  
- `json` (`string`): A JSON-formatted string containing the batch import definition.  
- `stream` (`Stream`): A readable stream positioned at the start of the batch import data.

**Return Value:**  
`IReadOnlyList<string>` — A read-only list of error strings. The list is empty when no validation errors are found. The order of messages corresponds to the order in which violations were detected.

**Exceptions:**  
- `ArgumentNullException` — Thrown when the input argument is `null`.  
- `ObjectDisposedException` — Thrown by the `Stream` overload if the stream has been closed or disposed prior to the call.  
- `InvalidDataException` — Thrown when the input cannot be parsed at all (e.g., fundamentally malformed JSON or unrecognized binary header), preventing even structural validation.

---

### IsValid Methods

```csharp
public static bool IsValid(byte[] data)
public static bool IsValid(string json)
public static bool IsValid(Stream stream)
```

**Purpose:**  
Provides a lightweight boolean check indicating whether the supplied data passes all validation rules. These methods are optimized to short-circuit on the first encountered error.

**Parameters:**  
- `data` (`byte[]`): Raw binary payload representing the batch import.  
- `json` (`string`): A JSON-formatted string containing the batch import definition.  
- `stream` (`Stream`): A readable stream positioned at the start of the batch import data.

**Return Value:**  
`bool` — `true` if the data is fully valid according to all import rules; `false` otherwise.

**Exceptions:**  
- `ArgumentNullException` — Thrown when the input argument is `null`.  
- `ObjectDisposedException` — Thrown by the `Stream` overload if the stream has been closed or disposed prior to the call.

---

### EnsureValid Methods

```csharp
public static void EnsureValid(byte[] data)
public static void EnsureValid(string json)
public static void EnsureValid(Stream stream)
```

**Purpose:**  
Asserts that the supplied data is valid for batch import. If validation fails, an exception is thrown containing the complete list of detected errors. These methods are intended for use at trust boundaries where invalid data must halt processing immediately.

**Parameters:**  
- `data` (`byte[]`): Raw binary payload representing the batch import.  
- `json` (`string`): A JSON-formatted string containing the batch import definition.  
- `stream` (`Stream`): A readable stream positioned at the start of the batch import data.

**Return Value:**  
`void` — Returns normally only when the data is fully valid.

**Exceptions:**  
- `ArgumentNullException` — Thrown when the input argument is `null`.  
- `ObjectDisposedException` — Thrown by the `Stream` overload if the stream has been closed or disposed prior to the call.  
- `BatchImportValidationException` — Thrown when one or more validation errors are detected. The exception message aggregates all error strings that would have been returned by the corresponding `Validate` call.

---

## Usage

### Example 1: Collecting and Logging All Errors

```csharp
string batchJson = File.ReadAllText("incoming_batch.json");
IReadOnlyList<string> errors = BatchDataImporterValidation.Validate(batchJson);

if (errors.Count > 0)
{
    foreach (string error in errors)
    {
        logger.Warning("Batch import validation error: {Error}", error);
    }
    return;
}

// Proceed with import
importer.Import(batchJson);
```

### Example 2: Fail-Fast Assertion at API Boundary

```csharp
[HttpPost]
public IActionResult UploadBatch([FromBody] string batchPayload)
{
    try
    {
        BatchDataImporterValidation.EnsureValid(batchPayload);
    }
    catch (BatchImportValidationException ex)
    {
        return BadRequest(new { errors = ex.Errors });
    }

    var result = batchService.Process(batchPayload);
    return Ok(result);
}
```

## Notes

- **Stream Ownership:** The `Stream` overloads do not close or dispose the stream after validation. The stream position after the call is unspecified; callers should not rely on it remaining at the original offset. If repositioning is required, the caller must seek the stream back to the desired position before further processing.
- **Thread Safety:** All methods are static and do not mutate shared state. They are safe to call concurrently from multiple threads provided each call operates on its own input instance. The `Stream` overload is only thread-safe if the underlying stream implementation itself supports concurrent reads.
- **Error Aggregation:** `Validate` collects all errors encountered during a full scan, while `IsValid` stops at the first error. When both correctness and performance matter, prefer `IsValid` for early rejection and `Validate` when a complete diagnostic picture is required.
- **Input Duplication:** The `byte[]` and `string` overloads do not modify the input. The `Stream` overload reads from the stream but does not alter the underlying storage. Callers may safely reuse the same input reference after a validation call.
