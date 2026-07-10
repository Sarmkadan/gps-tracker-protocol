# IJsonFormatter

`IJsonFormatter` is the core serialization contract for the GPS tracker protocol. It defines a set of `Format` overloads that produce protocol-compliant JSON strings from individual position records, and a generic `Deserialize<T>` method that reconstructs typed objects from raw JSON. Implementations are expected to handle the full set of position fields—coordinates, speed, bearing, altitude, accuracy, satellite metadata, and device/frame identifiers—along with the raw hex payload and server-side reception timestamp.

## API

### `public JsonFormatter`

The concrete implementation type that satisfies this interface. It is exposed as a public member so consumers can instantiate or inject it directly without relying on interface-based resolution alone.

### `public string Format`

Overload that serializes the current state of the formatter into a JSON string. The exact signature varies across overloads, but all return a complete JSON representation of the position record. No parameters are required; the method reads from the instance properties. Throws `InvalidOperationException` when required fields (`DeviceId`, `Timestamp`, `Protocol`, `FrameId`, `RawDataHex`, `ReceivedAt`) have not been set.

### `public string Format`

Additional overload accepting a subset of fields as arguments. Produces a JSON string from the supplied values rather than from instance state. Throws `ArgumentNullException` when any required string argument is null or empty.

### `public string Format`

Overload that accepts a complete position object and returns its JSON representation. Useful when the caller already holds a populated data transfer object. Throws `ArgumentNullException` if the argument is null.

### `public string Format`

Overload designed for partial updates, accepting a delta object alongside baseline values. Merges the provided fields with existing instance state and returns the resulting JSON. Throws `InvalidOperationException` when the merge would leave required fields unset.

### `public T Deserialize<T>`

Generic deserialization method that reconstructs an instance of `T` from a JSON string. The type parameter `T` must be a class with a parameterless constructor and properties matching the protocol schema. Returns the populated object. Throws `JsonException` when the input is malformed or cannot be mapped to `T`. Throws `ArgumentNullException` when the input string is null.

### `public required string DeviceId`

Unique identifier of the tracking device. Must be set before any `Format` call that reads instance state; otherwise serialization throws.

### `public required string Timestamp`

Device-reported timestamp for the position fix. Expected in ISO 8601 format. Required for serialization.

### `public double Latitude`

WGS84 latitude in decimal degrees. Positive for north, negative for south. Defaults to `0.0` when not set.

### `public double Longitude`

WGS84 longitude in decimal degrees. Positive for east, negative for west. Defaults to `0.0`.

### `public double Speed`

Ground speed in meters per second. Defaults to `0.0`.

### `public double Bearing`

Course over ground in degrees from true north (0–360). Defaults to `0.0`.

### `public double Altitude`

Altitude above WGS84 ellipsoid in meters. Defaults to `0.0`.

### `public double Accuracy`

Estimated horizontal accuracy of the fix in meters. Defaults to `0.0`.

### `public int SatelliteCount`

Number of satellites used in the position solution. Defaults to `0`.

### `public required string Protocol`

Protocol identifier string (e.g., `"gt06"`, `"teltonika"`). Required for serialization.

### `public required string FrameId`

Unique identifier for the frame or message within the device session. Required for serialization.

### `public required string Protocol`

Duplicate declaration of the protocol identifier. Both must be set to the same value; implementations typically synchronize them internally.

### `public required string RawDataHex`

The original raw payload received from the device, encoded as an uppercase hexadecimal string. Required for serialization.

### `public required string ReceivedAt`

Server-side timestamp recording when the frame was ingested. Expected in ISO 8601 format. Required for serialization.

## Usage

### Example 1: Building and formatting a complete position record

```csharp
var formatter = new JsonFormatter
{
    DeviceId = "DEV8675309",
    Timestamp = "2025-03-15T08:22:17Z",
    Latitude = 52.5200,
    Longitude = 13.4050,
    Speed = 12.5,
    Bearing = 187.3,
    Altitude = 34.1,
    Accuracy = 3.8,
    SatelliteCount = 14,
    Protocol = "gt06",
    FrameId = "0x4A2F",
    RawDataHex = "78780D0103588990662464160001E9F30D0A",
    ReceivedAt = "2025-03-15T08:22:19.452Z"
};

string json = formatter.Format();
// json contains all fields serialized per protocol schema
```

### Example 2: Deserializing a received JSON payload

```csharp
string incomingJson = """
{
    "deviceId": "DEV8675309",
    "timestamp": "2025-03-15T08:22:17Z",
    "latitude": 52.5200,
    "longitude": 13.4050,
    "speed": 12.5,
    "bearing": 187.3,
    "altitude": 34.1,
    "accuracy": 3.8,
    "satelliteCount": 14,
    "protocol": "gt06",
    "frameId": "0x4A2F",
    "rawDataHex": "78780D0103588990662464160001E9F30D0A",
    "receivedAt": "2025-03-15T08:22:19.452Z"
}
""";

PositionRecord record = formatter.Deserialize<PositionRecord>(incomingJson);
Console.WriteLine(record.Latitude);  // 52.52
```

## Notes

- **Required fields**: `DeviceId`, `Timestamp`, `Protocol`, `FrameId`, `RawDataHex`, and `ReceivedAt` are all marked `required`. Any `Format` overload that reads instance state will throw `InvalidOperationException` if these have not been assigned. The overloads that accept parameters validate their arguments at call time and throw `ArgumentNullException` for null or empty strings.
- **Duplicate `Protocol` members**: The interface declares `Protocol` twice. Implementations are expected to keep both in sync; setting one should propagate to the other. Consumers should treat them as a single logical field and avoid assigning conflicting values.
- **Default numeric values**: All double fields and `SatelliteCount` default to zero. A position at (0.0, 0.0) with zero speed and zero satellites is technically valid serialized output, but consumers should validate semantic plausibility separately—this interface does not enforce GPS fix validity.
- **Thread safety**: `JsonFormatter` is not guaranteed to be thread-safe. The instance properties are mutable, and concurrent reads/writes or overlapping calls to state-reading `Format` overloads may produce inconsistent JSON. Callers should synchronize access when sharing an instance across threads, or prefer the stateless `Format` and `Deserialize<T>` overloads that operate on explicit arguments.
- **Deserialization constraints**: `Deserialize<T>` requires `T` to have a parameterless constructor and property names that match the JSON keys (case-insensitive by default). It throws `JsonException` for mismatched schemas, missing required properties in the JSON, or invalid value types. Hexadecimal strings like `RawDataHex` are deserialized as-is; no validation of hex encoding is performed beyond string presence.
