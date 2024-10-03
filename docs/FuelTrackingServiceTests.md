# FuelTrackingServiceTests

Unit test class for `FuelTrackingService`, verifying fuel event recording, retrieval, deletion, and reporting functionality.

## API

### `public FuelTrackingServiceTests`
Initializes a new instance of the `FuelTrackingServiceTests` class with required test dependencies (e.g., mock repositories, services, or in-memory stores). This constructor is typically parameterized to inject test doubles or test implementations of `IFuelTrackingRepository`, `IUnitOfWork`, or other collaborators.

### `public async Task RecordFuelEventAsync_ShouldStoreRecordSuccessfully`
Verifies that `FuelTrackingService.RecordFuelEventAsync` persists a valid fuel event to the underlying store. The test constructs a fuel event with positive fuel amount and valid metadata, invokes the service method, and asserts that the record is retrievable via `GetRecordsAsync` with matching properties (e.g., `FuelAmount`, `Timestamp`, `VehicleId`). No exceptions are expected.

### `public async Task RecordFuelEventAsync_ShouldThrowException_WhenFuelAmountIsZeroOrNegative`
Ensures that `FuelTrackingService.RecordFuelEventAsync` throws an `ArgumentException` or `ArgumentOutOfRangeException` when the provided `fuelAmount` is zero or negative. The test invokes the service with invalid inputs and asserts that the expected exception is thrown before any persistence occurs.

### `public async Task GetRecordsAsync_ShouldReturnFilteredRecords`
Confirms that `FuelTrackingService.GetRecordsAsync` returns a filtered collection of fuel records based on optional criteria (e.g., date range, vehicle identifier, or record type). The test seeds multiple records, applies a filter, and asserts that only matching records are returned with correct ordering and pagination.

### `public async Task DeleteRecordAsync_ShouldReturnTrue_WhenRecordExists`
Validates that `FuelTrackingService.DeleteRecordAsync` returns `true` and removes the specified record when it exists in the store. The test inserts a record, invokes deletion with its identifier, and asserts that subsequent retrieval fails and the method returns `true`.

### `public async Task GetReportAsync_ShouldCalculateCorrectTotals`
Checks that `FuelTrackingService.GetReportAsync` computes aggregate totals (e.g., total fuel consumed, average consumption, or cost) accurately across a set of records. The test populates the store with known records, invokes the report method, and asserts that totals match expected values derived from the input data.

### `public void EstimateFuelLiters_ShouldReturnZero_WhenInputsAreInvalid`
Ensures that `FuelTrackingService.EstimateFuelLiters` returns `0` when input parameters are invalid (e.g., negative distance, zero fuel efficiency, or null inputs). The test invokes the method with edge-case inputs and asserts the result is `0` without throwing.

## Usage
