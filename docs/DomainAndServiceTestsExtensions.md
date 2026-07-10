# DomainAndServiceTestsExtensions

The `DomainAndServiceTestsExtensions` class provides a suite of static extension methods designed to simplify the instantiation and population of core domain entities within the `gps-tracker-protocol` project. These methods facilitate the creation of consistent, valid, and specifically configured test data, reducing boilerplate code in unit and integration tests and ensuring that test scenarios utilize standardized object representations.

## API

### `CreateValidLocation`
Generates a `LocationData` instance populated with default, valid coordinates and timestamp values.
*   **Returns:** A `LocationData` object with valid properties.

### `CreateValidDevice`
Generates a `Device` instance populated with default, valid identification and state properties.
*   **Returns:** A `Device` object with valid properties.

### `CreateDeviceWithImei`
Generates a `Device` instance with a specific International Mobile Equipment Identity (IMEI).
*   **Parameters:** `string imei` - The IMEI to assign to the device.
*   **Returns:** A `Device` object configured with the specified IMEI.

### `CreateValidGpsFrame`
Generates a `GpsFrame` instance populated with standard, valid telemetry data.
*   **Returns:** A `GpsFrame` object with valid properties.

### `CreateOfflineDevice`
Generates a `Device` instance pre-configured to represent an offline state.
*   **Returns:** A `Device` object with a status property indicating the device is offline.

### `CreateInvalidLocation`
Generates a `LocationData` instance populated with invalid data, suitable for testing validation logic.
*   **Returns:** A `LocationData` object with out-of-range or malformed properties.

### `CreateLocationWithBearing`
Generates a `LocationData` instance with a specified bearing value.
*   **Parameters:** `double bearing` - The bearing value in degrees.
*   **Returns:** A `LocationData` object configured with the specified bearing.

### `CreateLocationWithSpeed`
Generates a `LocationData` instance with a specified speed value.
*   **Parameters:** `double speed` - The speed value in meters per second (or unit applicable to the domain model).
*   **Returns:** A `LocationData` object configured with the specified speed.

### `CreateDeviceWithNetworkInfo`
Generates a `Device` instance populated with valid network connectivity information.
*   **Returns:** A `Device` object configured with default network-related fields.

## Usage

```csharp
// Example 1: Creating a valid device and a corresponding location frame
var device = DomainAndServiceTestsExtensions.CreateValidDevice();
var location = DomainAndServiceTestsExtensions.CreateValidLocation();

// Proceed with testing logic
Assert.NotNull(device);
Assert.True(location.IsValid());
```

```csharp
// Example 2: Testing device validation with an invalid location
var invalidLocation = DomainAndServiceTestsExtensions.CreateInvalidLocation();
var result = LocationValidator.Validate(invalidLocation);

Assert.False(result.Success);
```

## Notes

*   **Thread Safety:** As these methods generate new instances and do not rely on shared mutable state, they are thread-safe and can be utilized concurrently within parallel test execution environments.
*   **Data Consistency:** While these methods generate valid data by default, tests requiring specific edge-case behavior (e.g., boundary conditions for latitude or speed) should be configured explicitly after instantiation if the extension methods do not provide sufficient parameterization.
*   **Implementation Dependency:** Changes to the underlying domain model constructors or property requirements may necessitate updates to these extension methods to maintain test data integrity.
