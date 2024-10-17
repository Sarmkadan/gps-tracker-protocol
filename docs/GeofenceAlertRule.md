# GeofenceAlertRule

Represents a rule that triggers geofence-related alerts for a specific device when certain conditions are met. Used to monitor device entry/exit from predefined geographic boundaries and enforce cooldown periods between consecutive alerts.

## API

### Properties

- **`Id`** (string)
  Unique identifier for the alert rule. Read-only; assigned when the rule is created.

- **`DeviceId`** (string)
  Identifier of the device to which this rule applies. Required; must correspond to an existing device.

- **`GeofenceId`** (string)
  Identifier of the geofence associated with this rule. Required; must reference a valid geofence definition.

- **`AlertType`** (GeofenceAlertType)
  Type of alert to trigger (e.g., entry, exit, speed violation). Determines the condition under which the alert fires.

- **`Cooldown`** (TimeSpan)
  Minimum duration that must elapse between consecutive alerts for the same rule and device. Prevents alert spam; must be non-negative.

- **`IsEnabled`** (bool)
  Indicates whether the rule is active. When `false`, the rule is ignored during processing.

- **`Description`** (string)
  Human-readable explanation of the rule’s purpose or behavior. Optional; may be empty.

- **`CreatedAt`** (DateTime)
  Timestamp when the rule was created. Set automatically; not modifiable.

- **`RuleId`** (string)
  Identifier of the parent rule definition (if this is an instance of a reusable rule). Optional; may be `null`.

- **`Latitude`** (double)
  Geographic latitude coordinate associated with the alert (e.g., point of entry/exit). Valid range: `-90.0` to `90.0`.

- **`Longitude`** (double)
  Geographic longitude coordinate associated with the alert. Valid range: `-180.0` to `180.0`.

- **`Speed`** (double)
  Speed value recorded when the alert was triggered (in device-specific units). May be negative or zero depending on context.

- **`FiredAt`** (DateTime)
  Timestamp when the alert was triggered. Set automatically when the rule condition is met; not modifiable.

- **`AcknowledgedAt`** (DateTime?)
  Timestamp when the alert was acknowledged, if applicable. `null` if not yet acknowledged.

- **`Status`** (GeofenceAlertStatus)
  Current state of the alert (e.g., pending, acknowledged, resolved). Reflects lifecycle of the alert instance.

- **`Notes`** (string)
  Additional context or operator comments attached to the alert. Optional; may be empty or updated after creation.

## Usage
