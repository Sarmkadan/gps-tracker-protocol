# IGeofenceAlertingService

The `IGeofenceAlertingService` interface defines the contract for managing geofence‑based alerts within the GPS tracker protocol. It enables callers to create, retrieve, and delete alert rules, query active and historical alerts, acknowledge alerts, and feed geofence transition events into the service for evaluation.

## API

### GeofenceAlertingService  
**Purpose:** Initializes a new instance of the service implementation.  
**Parameters:** As defined by the concrete constructor (typically dependencies such as a repository or clock).  
**Return:** A new `IGeofenceAlertingService` instance.  
**Throws:** May throw `ArgumentNullException` if required dependencies are `null`, or other exceptions dictated by the implementation.

### CreateAlertRule  
**Purpose:** Registers a new geofence alert rule for a device.  
**Parameters:** As defined by the method signature (commonly a device identifier and a rule definition).  
**Return:** The created `GeofenceAlertRule` object, including any system‑assigned identifiers.  
**Throws:**  
- `ArgumentException` if the rule definition is invalid or missing required fields.  
- `InvalidOperationException` if the service is not in a state that permits rule creation.  
- Implementation‑specific exceptions for persistence failures.

### DeleteAlertRule  
**Purpose:** Removes an existing geofence alert rule.  
**Parameters:** As defined by the method signature (typically a rule identifier).  
**Return:** `void`.  
**Throws:**  
- `KeyNotFoundException` if the specified rule does not exist.  
- `ArgumentException` if the identifier is invalid.  
- `InvalidOperationException` if the rule cannot be deleted due to ongoing alerts.

### GetRulesForDevice  
**Purpose:** Retrieves all alert rules associated with a particular device.  
**Parameters:** As defined by the method signature (commonly a device identifier).  
**Return:** An `IReadOnlyList<GeofenceAlertRule>` containing the rules; empty list if none exist.  
**Throws:**  
- `ArgumentException` if the device identifier is invalid.  
- `InvalidOperationException` if the service is unable to query the rule store.

### GetActiveAlerts  
**Purpose:** Returns the currently active geofence alerts across all devices (or filtered per implementation).  
**Parameters:** None.  
**Return:** An `IReadOnlyList<GeofenceAlert>` representing alerts that have not been acknowledged or cleared.  
**Throws:**  
- `InvalidOperationException` if the service has not been initialized or is disabled.

### GetAlertHistory  
**Purpose:** Returns a historical list of geofence alerts, optionally filtered by time or device.  
**Parameters:** As defined by the method signature (often a device identifier and/or date range).  
**Return:** An `IReadOnlyList<GeofenceAlert>` containing past alerts.  
**Throws:**  
- `ArgumentOutOfRangeException` if supplied date range is invalid.  
- `ArgumentException` if filter parameters are invalid.  
- `InvalidOperationException` if the history store is unavailable.

### AcknowledgeAlert  
**Purpose:** Marks a specific alert as acknowledged, preventing further notifications.  
**Parameters:** As defined by the method signature (typically an alert identifier).  
**Return:** `true` if the alert was successfully acknowledged; `false` if the alert could not be found or was already acknowledged.  
**Throws:**  
- `ArgumentException` if the alert identifier is invalid.  
- `InvalidOperationException` if the service cannot update the alert state.

### ProcessGeofenceEntered  
**Purpose:** Notifies the service that a device has entered a geofenced area, triggering rule evaluation.  
**Parameters:** As defined by the method signature (commonly a device identifier and a geofence identifier).  
**Return:** `void`.  
**Throws:**  
- `InvalidOperationException` if the service is unable to process the event (e.g., missing rule).  
- `ArgumentException` if either identifier is invalid.

### ProcessGeofenceExited  
**Purpose:** Notifies the service that a device has exited a geofenced area, triggering rule evaluation.  
**Parameters:** As defined by the method signature (commonly a device identifier and a geofence identifier).  
**Return:** `void`.  
**Throws:**  
- `InvalidOperationException` if the service is unable to process the event.  
- `ArgumentException` if either identifier is invalid.

## Usage

### Example 1: Creating a rule and handling a geofence entry
```csharp
IGeofenceAlertingService alertingService = new GeofenceAlertingService(/* dependencies */);

// Define a rule for device "ABC123" that triggers when entering geofence "HOME_ZONE"
var rule = alertingService.CreateAlertRule(
    deviceId: "ABC123",
    geofenceId: "HOME_ZONE",
    alertOnEnter: true,
    alertOnExit: false);

// Later, when the device reports entering the geofence:
alertingService.ProcessGeofenceEntered(deviceId: "ABC123", geofenceId: "HOME_ZONE");

// Check for any newly active alerts
IReadOnlyList<GeofenceAlert> active = alertingService.GetActiveAlerts();
foreach (var alert in active)
{
    Console.WriteLine($"Active alert: {alert.Id} for device {alert.DeviceId}");
}
```

### Example 2: Retrieving history and acknowledging alerts
```csharp
IGeofenceAlertingService alertingService = new GeofenceAlertingService(/* dependencies */);

// Get alerts from the last 24 hours for device "XYZ789"
var history = alertingService.GetAlertHistory(
    deviceId: "XYZ789",
    startTime: DateTime.UtcNow.AddHours(-24),
    endTime: DateTime.UtcNow);

Console.WriteLine($"Found {history.Count} historical alerts.");

// Acknowledge the most recent unacknowledged alert, if any
var latest = history.OrderByDescending(a => a.Timestamp).FirstOrDefault();
if (latest != null && !latest.IsAcknowledged)
{
    bool acknowledged = alertingService.AcknowledgeAlert(alertId: latest.Id);
    Console.WriteLine(latest.Id
        ? $"Alert {latest.Id} acknowledged."
        : $"Failed to acknowledge alert {latest.Id}.");
}
```

## Notes
- **Thread safety:** The interface does not guarantee thread‑safe implementations. Callers should synchronize access if the same service instance is used from multiple threads concurrently.  
- **Null arguments:** Passing `null` for any identifier or complex parameter is likely to result in an `ArgumentNullException`; implementations should validate arguments and throw appropriately.  
- **Idempotency:** `AcknowledgeAlert` may be called multiple times on the same alert; subsequent calls should return `false` without throwing.  
- **Event ordering:** `ProcessGeofenceEntered` and `ProcessGeofenceExited` should be called in chronological order as reported by the device; out‑of‑order events may produce undefined alert states.  
- **Resource disposal:** If the concrete service holds unmanaged resources (e.g., database connections), callers should dispose of the instance via the appropriate mechanism (e.g., implementing `IDisposable`).  
- **Exception propagation:** Persistence or infrastructure failures are allowed to propagate as implementation‑specific exceptions; callers should handle them according to the application’s error‑handling policy.
