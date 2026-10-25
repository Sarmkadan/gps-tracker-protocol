# Domain Models

Overview of the core types in `Domain/Models/`. These are the records the parsers produce and the fleet services consume: a device reports frames, frames are parsed into location data, locations are grouped into journeys, and commands and responses flow back to the device.

## Device

`Device` (`Domain/Models/Device.cs`) is the mutable record for a GPS tracking unit. It carries the identity (`Id`, `Imei`, `DeviceName`), the transport protocol (`Protocol`, a `ProtocolType`), and live connection state (`Status`, `LastSeen`, `IpAddress`, `Port`, `ConnectionCount`). `IsValid()` checks that the IMEI is 14 to 16 digits, and `UpdateHeartbeat()` marks the device online, stamps `LastSeen`, and increments the connection count. `IsOffline(TimeSpan)` compares `LastSeen` against a timeout. Operators can attach free-form `Metadata`, and `BatteryLevel` and `SignalStrength` default to "unknown" values (`-1` and `0`). The read-only `DeviceStatusDto` record in the same file is the immutable snapshot returned to health endpoints.

## LocationData

`LocationData` (`Domain/Models/LocationData.cs`) is one parsed GPS fix: latitude and longitude in decimal degrees, altitude in meters, speed in km/h, bearing in degrees, the device timestamp, fix accuracy, satellite count, and the originating `DeviceId` and `Protocol`. Protocol-specific extras go into `ExtendedData`. `IsValid()` enforces the coordinate, bearing, and speed bounds and requires a device ID. `DistanceTo()` returns the Haversine distance in kilometers, and `BearingTo()` returns the initial bearing in degrees. Helper extensions for this type live in `LocationDataExtensions.cs` and `LocationDataValidation.cs`.

## Journey

`Journey` (`Domain/Models/Journey.cs`) is an ordered trip for one device, held as a list of `LocationData` waypoints with a `StartTime` and an optional `EndTime`. `Status` is a plain `int` (0 ongoing, 1 completed, 2 abandoned). `AddWaypoint()` rejects null or invalid points and sets `EndTime` to the timestamp of the point it just added. `GetTotalDistance()`, `GetAverageSpeed()`, `GetMaxSpeed()`, and `GetDuration()` derive summary figures from the waypoints. `Complete()` sets the status to completed, stamps `EndTime`, and writes those summaries into `Metadata`. The same file defines `IdlePeriod`, which describes a stationary stretch inside a journey.

## Command

`Command` (`Domain/Models/Command.cs`) is an instruction to send to a device, such as changing the reporting interval, setting a geofence, or requesting a location. The strongly typed `Type` (a `CommandType`) and the free-form `CommandType` string are separate properties, and `Parameters` holds the values each type needs. `Status` (a `CommandStatus`, starting at `Pending`) tracks progress, while `IsSent`, `SentTime`, `IsAcknowledged`, and `AcknowledgedTime` record the transmission handshake. Retries are bounded by `MaxRetries` (default 3) through `CanRetry()`, which marks the command `Failed` once the limit is reached. `IsValid()` checks that each command type has its required parameters, and `ToFormattedCommand()` renders the text payload for the supported types.

## GpsFrame

`GpsFrame` (`Domain/Models/GpsFrame.cs`) is the raw unit that arrives over TCP or UDP before any parsing. It holds the bytes in `RawData`, the protocol, the source address and port, and the receive timestamp. `IsValid()` checks a per-protocol minimum length and then computes the checksum for GT06, H02, or TK103 on first use, caching the result in `IsValidChecksum`. The helpers `ExtractBytes()`, `ExtractString()`, and `ToHex()` read fields out of the frame by offset, and `Headers` carries protocol-specific metadata. Parsers accept the underlying bytes as `ReadOnlySpan<byte>`, so `GpsFrame` is mainly the envelope the reassembly and dispatch code passes around.

## ResponseMessage

`ResponseMessage` (`Domain/Models/ResponseMessage.cs`) is a reply received from a device, usually to an earlier `Command`. It links back through `DeviceId` and an optional `CommandId`, and `Type` (a `MessageType`) selects how the content is read. `Parse()` splits `Content` on commas and fills `ParsedData` for four kinds of message: acknowledgements, errors (which set `ErrorCode` and `ErrorMessage`), location updates, and status reports. Malformed content is recorded in `IsSuccess` and `ErrorMessage` rather than thrown, but `Parse()` itself throws `InvalidOperationException` when the message has no `DeviceId` or empty `Content`. `IsSuccess` is the overall outcome, and `ReceivedAt` is stamped in UTC when the message is created.
