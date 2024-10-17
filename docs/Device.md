# Device

Represents a GPS tracking device with core telemetry, connection state, and metadata used in the gps-tracker-protocol project.

## API

### `public string Id`
Unique identifier for the device within the system. Immutable after creation.

### `public string Imei`
International Mobile Equipment Identity of the device. Used for cellular identification and tracking.

### `public string DeviceName`
Human-readable name assigned to the device. May be changed by users or operators.

### `public ProtocolType Protocol`
Network protocol used by the device (e.g., TCP, UDP, MQTT). Set at registration and immutable afterward.

### `public DeviceStatus Status`
Current operational status of the device (e.g., Online, Offline, Maintenance). Updated via heartbeat or system events.

### `public DateTime LastSeen`
Timestamp of the most recent communication from the device. Updated on heartbeat or data transmission.

### `public string? IpAddress`
Network address from which the device last connected. May be null if unknown or not applicable.

### `public int Port`
Port number used during the last connection. Valid only when `IpAddress` is not null.

### `public bool IsActive`
Indicates whether the device is currently enabled and expected to transmit data.

### `public Dictionary<string, string> Metadata`
Additional key-value pairs associated with the device (e.g., firmware version, SIM ICCID). Mutable and extensible.

### `public int BatteryLevel`
Estimated battery level percentage (0–100). Reported by the device or inferred from telemetry.

### `public int SignalStrength`
Signal strength indicator (typically in dBm or arbitrary units). Updated with device reports.

### `public int ConnectionCount`
Total number of successful connections established since registration.

### `public DateTime RegistrationDate`
Timestamp when the device was first registered in the system.

### `public bool IsValid`
Indicates whether the device record is considered valid (e.g., passes integrity checks or business rules).

### `public void UpdateHeartbeat()`
Updates `LastSeen` to the current system time and increments internal counters. May throw `InvalidOperationException` if the device is not active.

### `public bool IsOffline()`
Returns `true` if the device has not communicated within the offline threshold (e.g., 5 minutes). Does not throw.

### `public override string ToString()`
Returns a formatted string containing `DeviceName`, `Id`, and `Status`. Overrides `object.ToString()`.

### `public string DeviceId`
Alias for `Id`. Provided for backward compatibility.

### `public string Imei`
Alias for `Imei`. Included for consistency with legacy naming.

## Usage
