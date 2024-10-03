# DomainAndServiceTests

Unit tests for the GPS tracker protocol domain model and service layer, validating business rules for location data, device state, and frame parsing.

## API

### LocationData Tests

#### `LocationData_IsValid_WithValidCoordinatesAndDeviceId_ReturnsTrue`
Validates that a `LocationData` instance with latitude within [-90, 90], longitude within [-180, 180], non-empty device ID, and non-negative speed is considered valid. Returns `true` on success; no exceptions are thrown.

#### `LocationData_IsValid_WithLatitudeExceedingNinetyDegrees_ReturnsFalse`
Validates that a `LocationData` instance with latitude > 90 or < -90 is considered invalid. Returns `false`; no exceptions are thrown.

#### `LocationData_IsValid_WithNegativeSpeed_ReturnsFalse`
Validates that a `LocationData` instance with negative speed is considered invalid. Returns `false`; no exceptions are thrown.

#### `LocationData_IsValid_WithEmptyDeviceId_ReturnsFalse`
Validates that a `LocationData` instance with an empty or whitespace device ID is considered invalid. Returns `false`; no exceptions are thrown.

#### `LocationData_IsValid_BearingExceedingThreeSixtyDegrees_ReturnsFalse`
Validates that a `LocationData` instance with bearing > 360 or < 0 is considered invalid. Returns `false`; no exceptions are thrown.

#### `LocationData_DistanceTo_SameLocation_ReturnsZero`
Computes the geodesic distance between two identical `LocationData` instances. Returns `0`; no exceptions are thrown.

#### `LocationData_DistanceTo_LondonToManchester_ReturnsExpectedDistance`
Computes the geodesic distance between London (51.5074° N, 0.1278° W) and Manchester (53.4808° N, 2.2426° W). Returns the expected distance in kilometers; no exceptions are thrown.

#### `LocationData_BearingTo_DueNorthPoint_ReturnsZeroDegrees`
Computes the initial bearing from a point to a due north point (same longitude, higher latitude). Returns `0` degrees; no exceptions are thrown.

### Device Tests

#### `Device_IsValid_WithFifteenDigitImei_ReturnsTrue`
Validates that a `Device` instance with a 15-digit numeric IMEI is considered valid. Returns `true`; no exceptions are thrown.

#### `Device_IsValid_WithAlphaNumericImei_ReturnsFalse`
Validates that a `Device` instance with an alphanumeric IMEI is considered invalid. Returns `false`; no exceptions are thrown.

#### `Device_IsValid_WithTooShortImei_ReturnsFalse`
Validates that a `Device` instance with an IMEI shorter than 15 digits is considered invalid. Returns `false`; no exceptions are thrown.

#### `Device_IsValid_WithEmptyId_ReturnsFalse`
Validates that a `Device` instance with an empty or whitespace ID is considered invalid. Returns `false`; no exceptions are thrown.

#### `Device_UpdateHeartbeat_SetsStatusToOnline`
Updates the device heartbeat and verifies that the `Status` property is set to `Online`. Returns void; no exceptions are thrown.

#### `Device_UpdateHeartbeat_WithIpAndPort_UpdatesNetworkInfo`
Updates the device heartbeat with an IP address and port, and verifies that the `LastIp` and `LastPort` properties are updated accordingly. Returns void; no exceptions are thrown.

#### `Device_IsOffline_AfterRecentHeartbeat_ReturnsFalse`
Verifies that a device with a recent heartbeat (within the offline threshold) is not considered offline. Returns `false`; no exceptions are thrown.

#### `Device_IsOffline_WithStaleLastSeen_ReturnsTrue`
Verifies that a device with a `LastSeen` timestamp older than the offline threshold is considered offline. Returns `true`; no exceptions are thrown.

### GpsFrame Tests

#### `GpsFrame_IsValid_ValidGT06FrameWithChecksum_ReturnsTrue`
Validates that a syntactically correct GT06 frame with a valid checksum is considered valid. Returns `true`; no exceptions are thrown.

#### `GpsFrame_IsValid_GT06FrameTooShort_ReturnsFalse`
Validates that a GT06 frame shorter than the minimum required length is considered invalid. Returns `false`; no exceptions are thrown.

#### `GpsFrame_IsValid_EmptyRawData_ReturnsFalse`
Validates that an empty raw data buffer is considered an invalid GT06 frame. Returns `false`; no exceptions are thrown.

#### `GpsFrame_IsValid_InvalidChecksum_ReturnsFalse`
Validates that a GT06 frame with an incorrect checksum is considered invalid. Returns `false`; no exceptions are thrown.

## Usage
