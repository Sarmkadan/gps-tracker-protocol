# GeofenceAlertingServiceTests

The `GeofenceAlertingServiceTests` class contains unit tests that verify the behavior of the `GeofenceAlertingService` component within the `gps-tracker-protocol` project. Each test method exercises a specific scenario—such as creating alert rules, processing geofence entry events, acknowledging alerts, and deleting rules—to ensure the service behaves correctly under normal conditions, cooldown suppression, and absence of matching rules.

## API

### `public GeofenceAlertingServiceTests()`

Initializes a new instance of the test class. No parameters are required. The constructor typically sets up any necessary test fixtures, mocks, or instances of `GeofenceAlertingService` used by the test methods.

### `public void CreateAlertRule_ShouldAddRuleForDevice()`

Verifies that calling the rule-creation method on the service results in a rule being associated with the specified device.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: No exceptions are expected under normal test conditions; any unexpected exception indicates a test failure.

### `public void ProcessGeofenceEntered_ShouldFireAlertWhenMatchingRuleExists()`

Confirms that when a geofence entry event is processed and a matching alert rule exists for the device and geofence, the service fires an alert.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: No exceptions are expected.

### `public void ProcessGeofenceEntered_ShouldSuppressAlertWithinCooldown()`

Ensures that if a geofence entry event occurs within the cooldown period of a previously fired alert for the same rule, the service suppresses the duplicate alert.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: No exceptions are expected.

### `public void AcknowledgeAlert_ShouldMarkAlertAsAcknowledged()`

Validates that acknowledging an alert through the service correctly updates its status to acknowledged.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: No exceptions are expected.

### `public void DeleteAlertRule_ShouldRemoveRule()`

Checks that deleting an alert rule from the service removes it and that subsequent geofence entry events no longer trigger alerts for that rule.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: No exceptions are expected.

### `public void ProcessGeofenceEntered_ShouldNotFireAlert_WhenNoMatchingRule()`

Verifies that when a geofence entry event is processed and no alert rule exists for the device or geofence, no alert is fired.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: No exceptions are expected.

## Usage

The test class is designed to be executed by a unit test runner such as xUnit, NUnit, or MSTest. Below are two realistic examples.

**Example 1: Running a single test with xUnit**

```csharp
using Xunit;

public class GeofenceAlertingServiceTests
{
    [Fact]
    public void CreateAlertRule_ShouldAddRuleForDevice()
    {
        // Arrange
        var service = new GeofenceAlertingService(/* dependencies */);
        var deviceId = "device-001";
        var geofenceId = "zone-a";

        // Act
        service.CreateAlertRule(deviceId, geofenceId);

        // Assert
        Assert.True(service.HasRuleForDevice(deviceId, geofenceId));
    }
}
```

**Example 2: Using a test fixture to share service instance**

```csharp
using Xunit;

public class GeofenceAlertingServiceTests : IClassFixture<GeofenceAlertingServiceFixture>
{
    private readonly GeofenceAlertingService _service;

    public GeofenceAlertingServiceTests(GeofenceAlertingServiceFixture fixture)
    {
        _service = fixture.Service;
    }

    [Fact]
    public void ProcessGeofenceEntered_ShouldFireAlertWhenMatchingRuleExists()
    {
        // Arrange
        _service.CreateAlertRule("device-002", "zone-b");
        var entryEvent = new GeofenceEntryEvent("device-002", "zone-b");

        // Act
        var alert = _service.ProcessGeofenceEntered(entryEvent);

        // Assert
        Assert.NotNull(alert);
        Assert.Equal("device-002", alert.DeviceId);
    }
}
```

## Notes

- **Edge Cases**:  
  - The tests assume that the `GeofenceAlertingService` is properly initialized with its dependencies (e.g., a rule store, a cooldown provider). Empty rule sets, duplicate rule creation, and rapid successive entry events are covered by the cooldown suppression test.  
  - The `AcknowledgeAlert_ShouldMarkAlertAsAcknowledged` test should verify that an already‑acknowledged alert remains acknowledged and that acknowledging a non‑existent alert does not cause errors.  
  - The `DeleteAlertRule_ShouldRemoveRule` test should also confirm that deleting a rule that was never added does not throw.

- **Thread Safety**:  
  These tests are executed in a single‑threaded context and do not validate concurrent access to the `GeofenceAlertingService`. If the service is intended to be thread‑safe, additional integration or stress tests should be written separately. The test class itself is not thread‑safe and should not be run in parallel without proper isolation.

- **Test Dependencies**:  
  The test methods rely on the internal state of the service and any injected dependencies. It is recommended to use mock objects or in‑memory implementations to avoid external side effects and to keep tests deterministic.
