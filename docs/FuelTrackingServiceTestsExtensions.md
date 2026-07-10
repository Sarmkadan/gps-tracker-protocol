# FuelTrackingServiceTestsExtensions

A static utility class providing factory methods and assertion helpers for `FuelTrackingServiceTests` instances. It streamlines test setup by offering pre-configured service objects and simplifies verification of fuel record persistence through dedicated async assertion methods.

## API

### CreateDefault

```csharp
public static FuelTrackingServiceTests CreateDefault()
```

Creates a `FuelTrackingServiceTests` instance with default configuration suitable for most test scenarios. The returned object is fully initialized and ready for use without further setup.

**Returns:** A new `FuelTrackingServiceTests` with default settings.

**Throws:** May throw if the underlying service initialization fails due to missing dependencies or invalid default configuration.

---

### WithRecords

```csharp
public static FuelTrackingServiceTests WithRecords(this FuelTrackingServiceTests service, IEnumerable<FuelRecord> records)
```

Populates the specified service with the given fuel records, returning the same instance for fluent chaining. Existing records in the service are replaced.

**Parameters:**
- `service` — The target `FuelTrackingServiceTests` instance.
- `records` — The collection of `FuelRecord` objects to load into the service.

**Returns:** The same `FuelTrackingServiceTests` instance, enabling method chaining.

**Throws:** `ArgumentNullException` if `service` or `records` is `null`. May throw if any record fails validation during insertion.

---

### WithDateRange

```csharp
public static FuelTrackingServiceTests WithDateRange(this FuelTrackingServiceTests service, DateTime start, DateTime end)
```

Configures the service to operate within a specific date range, returning the instance for fluent chaining. This restricts queries and operations to records falling between `start` and `end`.

**Parameters:**
- `service` — The target `FuelTrackingServiceTests` instance.
- `start` — The inclusive start of the date range.
- `end` — The inclusive end of the date range.

**Returns:** The same `FuelTrackingServiceTests` instance, enabling method chaining.

**Throws:** `ArgumentNullException` if `service` is `null`. `ArgumentException` if `start` is later than `end`.

---

### AssertRecordExistsAsync

```csharp
public static async Task AssertRecordExistsAsync(this FuelTrackingServiceTests service, FuelRecord record)
```

Asynchronously asserts that the specified record exists within the service. The assertion fails if no matching record is found.

**Parameters:**
- `service` — The target `FuelTrackingServiceTests` instance.
- `record` — The `FuelRecord` expected to be present.

**Returns:** A `Task` representing the asynchronous assertion operation.

**Throws:** `ArgumentNullException` if `service` or `record` is `null`. An assertion exception (specific to the testing framework in use) if the record is not found.

---

### AssertRecordDoesNotExistAsync

```csharp
public static async Task AssertRecordDoesNotExistAsync(this FuelTrackingServiceTests service, FuelRecord record)
```

Asynchronously asserts that the specified record does **not** exist within the service. The assertion fails if a matching record is found.

**Parameters:**
- `service` — The target `FuelTrackingServiceTests` instance.
- `record` — The `FuelRecord` expected to be absent.

**Returns:** A `Task` representing the asynchronous assertion operation.

**Throws:** `ArgumentNullException` if `service` or `record` is `null`. An assertion exception if the record is unexpectedly found.

---

## Usage

### Example 1: Basic Setup and Existence Check

```csharp
[Test]
public async Task InsertRecord_ShouldPersist()
{
    var service = FuelTrackingServiceTestsExtensions.CreateDefault();
    var newRecord = new FuelRecord { VehicleId = "VH-001", Liters = 45.0, Timestamp = DateTime.UtcNow };

    await service.InsertAsync(newRecord);
    await service.AssertRecordExistsAsync(newRecord);
}
```

### Example 2: Fluent Configuration with Date Range and Negative Assertion

```csharp
[Test]
public async Task DeleteOldRecords_ShouldRemoveOutsideRange()
{
    var oldRecord = new FuelRecord { VehicleId = "VH-002", Liters = 30.0, Timestamp = new DateTime(2020, 1, 1) };
    var records = new[] { oldRecord };

    var service = FuelTrackingServiceTestsExtensions
        .CreateDefault()
        .WithRecords(records)
        .WithDateRange(new DateTime(2023, 1, 1), new DateTime(2024, 12, 31));

    await service.DeleteOutOfRangeAsync();
    await service.AssertRecordDoesNotExistAsync(oldRecord);
}
```

---

## Notes

- **Fluent chaining:** `WithRecords` and `WithDateRange` return the same instance, allowing multiple configurations to be composed in a single expression. Order matters when both are used—records outside the subsequently applied date range remain in the service but may be excluded from range-bound queries.
- **Assertion semantics:** `AssertRecordExistsAsync` and `AssertRecordDoesNotExistAsync` rely on the testing framework's assertion mechanism. Test methods using these helpers should be marked `async` and return `Task` to properly propagate exceptions.
- **Thread safety:** These methods are designed for single-threaded test execution. Concurrent use of the same `FuelTrackingServiceTests` instance across multiple threads is not supported and may produce race conditions or spurious assertion failures.
- **Record matching:** The assertion methods determine existence by matching on record identity or key fields. If the service implementation uses reference equality, passing a value-identical but distinct object may cause `AssertRecordExistsAsync` to fail unexpectedly.
- **Date range boundaries:** `WithDateRange` treats both `start` and `end` as inclusive. Records with timestamps exactly equal to either boundary are considered within range.
