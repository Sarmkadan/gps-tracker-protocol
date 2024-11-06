# GeofenceAlertRuleExtensions

`GeofenceAlertRuleExtensions` provides static extension methods for evaluating and summarizing geofence alert rules within the GPS tracker protocol domain. These utilities determine whether a rule is active, whether it can fire an alert under current conditions, whether it matches a specific device–geofence pair, and how to produce a human-readable summary of the rule's configuration.

## API

### `CanFireAlert`

```csharp
public static bool CanFireAlert(this GeofenceAlertRule rule, DateTime currentTime, bool isDeviceInsideGeofence)
```

Determines whether the given alert rule is eligible to fire an alert at the specified time, given the device's current position relative to the geofence.

**Parameters:**
- `rule` — The `GeofenceAlertRule` instance being evaluated.
- `currentTime` — The UTC or local timestamp against which time-based constraints (e.g., schedule windows) are checked.
- `isDeviceInsideGeofence` — `true` if the device is currently inside the geofence; `false` otherwise.

**Returns:** `true` if all conditions required for the rule to fire are satisfied; otherwise `false`.

**Throws:** `ArgumentNullException` when `rule` is `null`.

---

### `IsActiveRule`

```csharp
public static bool IsActiveRule(this GeofenceAlertRule rule, DateTime currentTime)
```

Checks whether the rule is considered active at the given time, independent of the device's current location. This typically evaluates schedule and enabled-state constraints.

**Parameters:**
- `rule` — The `GeofenceAlertRule` instance being evaluated.
- `currentTime` — The timestamp used to validate time-based activation windows.

**Returns:** `true` if the rule is active; otherwise `false`.

**Throws:** `ArgumentNullException` when `rule` is `null`.

---

### `GetDisplaySummary`

```csharp
public static string GetDisplaySummary(this GeofenceAlertRule rule)
```

Produces a concise, human-readable summary of the rule's key properties — such as geofence name, alert type, schedule, and direction (entry/exit) — suitable for display in logs, notifications, or user interfaces.

**Parameters:**
- `rule` — The `GeofenceAlertRule` instance to summarize.

**Returns:** A non-null, non-empty string describing the rule.

**Throws:** `ArgumentNullException` when `rule` is `null`. May throw `InvalidOperationException` if required internal data is missing or inconsistent.

---

### `MatchesDeviceAndGeofence`

```csharp
public static bool MatchesDeviceAndGeofence(this GeofenceAlertRule rule, string deviceId, string geofenceId)
```

Determines whether the rule is explicitly associated with the given device identifier and geofence identifier. This is a direct identity match, not a spatial or temporal evaluation.

**Parameters:**
- `rule` — The `GeofenceAlertRule` instance being tested.
- `deviceId` — The unique identifier of the device.
- `geofenceId` — The unique identifier of the geofence.

**Returns:** `true` if the rule's configured device and geofence identifiers both match the supplied values; otherwise `false`.

**Throws:** `ArgumentNullException` when `rule` is `null`. Behavior with null or empty `deviceId`/`geofenceId` is implementation-defined and may return `false` or throw `ArgumentException`.

## Usage

### Example 1: Evaluating whether to send an alert

```csharp
GeofenceAlertRule rule = geofenceAlertService.GetRule(ruleId);
DateTime now = DateTime.UtcNow;
bool deviceInside = locationService.IsDeviceInsideGeofence(device.Id, geofence.Id);

if (rule.IsActiveRule(now) && rule.CanFireAlert(now, deviceInside))
{
    string summary = rule.GetDisplaySummary();
    notificationService.SendAlert(device.Id, summary);
}
```

### Example 2: Filtering rules for a specific device–geofence pair

```csharp
string targetDeviceId = "device-42";
string targetGeofenceId = "geo-7";

var matchingRules = allRules
    .Where(r => r.MatchesDeviceAndGeofence(targetDeviceId, targetGeofenceId))
    .Where(r => r.IsActiveRule(DateTime.UtcNow))
    .ToList();

foreach (var rule in matchingRules)
{
    logger.Info($"Active rule matched: {rule.GetDisplaySummary()}");
}
```

## Notes

- **Order of evaluation:** `IsActiveRule` should typically be checked before `CanFireAlert`, as the latter may assume the rule is active and only layer on additional device-location constraints.
- **Time sensitivity:** Both `IsActiveRule` and `CanFireAlert` are time-dependent. Callers should use a consistent `DateTime` value (usually UTC) across related checks to avoid race conditions around schedule boundaries.
- **Null handling:** All methods throw `ArgumentNullException` when the `rule` argument is `null`. Callers should guard against null references before invocation.
- **Thread safety:** These methods are pure static functions with no observable side effects or shared mutable state. They are safe to call concurrently from multiple threads, provided the `GeofenceAlertRule` instance itself is not being mutated during the call.
- **Summary format:** The string returned by `GetDisplaySummary` is intended for display purposes only. Its exact format may vary across versions and should not be parsed programmatically.
- **Identifier matching:** `MatchesDeviceAndGeofence` performs exact, case-sensitive matching. Callers must ensure identifiers are normalized before comparison if case-insensitive matching is required.
