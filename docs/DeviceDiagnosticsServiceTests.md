# DeviceDiagnosticsServiceTests

The `DeviceDiagnosticsServiceTests` class provides a comprehensive suite of unit tests designed to validate the functionality of the `DeviceDiagnosticsService` within the `gps-tracker-protocol` project. These tests ensure that diagnostic queries and self-test routines operate correctly under various device states, including healthy operation, low battery scenarios, weak signal conditions, and cases involving non-existent device identifiers.

## API

| Member | Purpose |
| :--- | :--- |
| `GetDiagnosticsAsync_ShouldReturnCorrectConnectivityInfo` | Verifies that `GetDiagnosticsAsync` retrieves and aggregates accurate connectivity status for a known device. |
| `GetDiagnosticsAsync_ShouldReturnNull_WhenDeviceNotFound` | Confirms that `GetDiagnosticsAsync` returns `null` when queried with a device ID that is not present in the system. |
| `RunSelfTestAsync_ShouldPassForHealthyDevice` | Validates that `RunSelfTestAsync` returns a success status when all device operating parameters are within normal thresholds. |
| `RunSelfTestAsync_ShouldWarnAboutLowBattery` | Validates that `RunSelfTestAsync` correctly triggers a warning status when the device battery level is reported below the operational threshold. |
| `RunSelfTestAsync_ShouldWarnAboutWeakSignal` | Validates that `RunSelfTestAsync` correctly triggers a warning status when the device signal strength falls below the required threshold. |
| `GetDiagnosticsAsync_ShouldIncludeCorrectLocationCount` | Confirms that the diagnostic data returned by `GetDiagnosticsAsync` accurately reflects the count of cached location records for the target device. |
| `GetDiagnosticsAsync_ShouldClassifySignalCorrectly` | Verifies that `GetDiagnosticsAsync` correctly maps raw signal strength numeric values into their appropriate qualitative classifications. |

## Usage

The following examples assume the use of a testing framework such as xUnit.

### Example 1: Verifying healthy device diagnostics
```csharp
[Fact]
public async Task TestHealthyDeviceDiagnostics()
{
    // Arrange
    var service = new DeviceDiagnosticsService(mockRepository);
    var deviceId = "device-123";

    // Act
    var result = await service.GetDiagnosticsAsync(deviceId);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.IsConnected);
    Assert.Equal("Excellent", result.SignalClassification);
}
```

### Example 2: Verifying self-test handling of low battery
```csharp
[Fact]
public async Task TestSelfTestLowBatteryWarning()
{
    // Arrange
    var service = new DeviceDiagnosticsService(mockRepository);
    var deviceId = "device-low-battery";

    // Act
    var result = await service.RunSelfTestAsync(deviceId);

    // Assert
    Assert.False(result.Passed);
    Assert.Contains("LowBattery", result.Warnings);
}
```

## Notes

- **Edge Cases**: These tests cover boundary conditions, such as device IDs not present in the repository, battery levels at the exact threshold of low-battery categorization, and edge cases for signal strength classification. Ensure test mocks are configured to cover these scenarios comprehensively.
- **Thread Safety**: While the `DeviceDiagnosticsService` should be implemented to be thread-safe for production use, these tests are designed to be run in isolation. If tests are configured to run in parallel, ensure that any shared mock repository state is managed appropriately to prevent cross-test interference.
- **Asynchronous Execution**: All listed methods are `async` and return `Task`, reflecting the asynchronous nature of the underlying service calls to the storage or communication layer. Ensure test runners are configured to await these tasks correctly.
