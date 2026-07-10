# LocationData

`LocationData` represents a single geospatial fix reported by a tracking device. It encapsulates the core GNSS fields (position, speed, bearing, timestamp, accuracy, satellite count) together with protocol identification and an extensible dictionary for vendor‑specific data. The type is used throughout the GPS‑Tracker‑Protocol library to convey raw measurements and derived values such as distance and bearing to another point.

## API

### Id  
**Type:** `string`  
**Purpose:** Unique identifier for the fix, often a UUID or device‑assigned sequence number.  
**Parameters:** None.  
**Return Value:** The identifier string; may be `null` or empty if not supplied by the device.  
**Throws:** None.

### DeviceId  
**Type:** `string`  
**Purpose:** Identifier of the reporting device (e.g., IMEI, serial number).  
**Parameters:** None.  
**Return Value:** The device identifier string; may be `null` or empty if unknown.  
**Throws:** None.

### Latitude  
**Type:** `double`  
**Purpose:** Geographic latitude in decimal degrees, using the WGS84 datum. Positive values denote north of the equator.  
**Parameters:** None.  
**Return Value:** Latitude coordinate.  
**Throws:** None.

### Longitude  
**Type:** `double`  
**Purpose:** Geographic longitude in decimal degrees, using the WGS84 datum. Positive values denote east of the prime meridian.  
**Parameters:** None.  
**Return Value:** Longitude coordinate.  
**Throws:** None.

### Altitude  
**Type:** `double`  
**Purpose:** Height above the WGS84 ellipsoid in meters. Negative values indicate depth below the ellipsoid.  
**Parameters:** None.  
**Return Value:** Altitude measurement.  
**Throws:** None.

### Speed  
**Type:** `double`  
**Purpose:** Instantaneous ground speed in meters per second (m/s).  
**Parameters:** None.  
**Return Value:** Speed value; zero when stationary.  
**Throws:** None.

### Bearing  
**Type:** `double`  
**Purpose:** Direction of travel in degrees true north (0° = north, 90° = east).  
**Parameters:** None.  
**Return Value:** Bearing angle; undefined when speed is zero (value may be arbitrary).  
**Throws:** None.

### Timestamp  
**Type:** `DateTime`  
**Purpose:** UTC time at which the fix was measured.  
**Parameters:** None.  
**Return Value:** DateTime representing the measurement moment.  
**Throws:** None.

### Accuracy  
**Type:** `double`  
**Purpose:** Estimated horizontal position error radius in meters (1‑sigma).  
**Parameters:** None.  
**Return Value:** Accuracy value; larger numbers indicate lower confidence.  
**Throws:** None.

### SatelliteCount  
**Type:** `int`  
**Purpose:** Number of satellites used in the solution.  
**Parameters:** None.  
**Return Value:** Integer count; zero indicates no fix.  
**Throws:** None.

### Protocol  
**Type:** `ProtocolType`  
**Purpose:** Enumeration identifying the messaging protocol that delivered the fix (e.g., NMEA, UBX, RTCM).  
**Parameters:** None.  
**Return Value:** The protocol enum value.  
**Throws:** None.

### ExtendedData  
**Type:** `Dictionary<string, object>`  
**Purpose:** Container for vendor‑specific or optional fields not covered by the standard members. Keys are strings; values can be any serializable type.  
**Parameters:** None.  
**Return Value:** The dictionary; may be empty but never `null`.  
**Throws:** None.

### IsValid  
**Type:** `bool`  
**Purpose:** Indicates whether the fix passes basic validity checks (e.g., latitude/longitude within range, timestamp not in the future).  
**Parameters:** None.  
**Return Value:** `true` if the data is considered usable; otherwise `false`.  
**Throws:** None.

### DistanceTo  
**Type:** `double`  
**Purpose:** Great‑circle distance (in meters) from this fix to another `LocationData` instance supplied as an argument to the calculating method (see usage).  
**Parameters:** None (value is populated by external code).  
**Return Value:** Distance in meters; zero when comparing to itself.  
**Throws:** None.

### BearingTo  
**Type:** `double`  
**Purpose:** Initial bearing (in degrees true north) from this fix to another `LocationData` instance.  
**Parameters:** None (value is populated by external code).  
**Return Value:** Bearing angle; undefined when the two points coincide.  
**Throws:** None.

### ToString  
**Type:** `string` (override of `object.ToString`)  
**Purpose:** Returns a human‑readable summary of the fix, including Id, timestamp, latitude, longitude, and validity.  
**Parameters:** None.  
**Return Value:** Formatted string.  
**Throws:** None.

## Usage

### Example 1: Creating a fix from raw GNSS data
```csharp
var fix = new LocationData
{
    Id          = Guid.NewGuid().ToString(),
    DeviceId    = "IMEI:123456789012345",
    Latitude    = 37.7749,
    Longitude   = -122.4194,
    Altitude    = 12.3,
    Speed       = 5.2,          // m/s (~11.6 mph)
    Bearing     = 45.0,         // NE
    Timestamp   = DateTime.UtcNow,
    Accuracy    = 3.0,
    SatelliteCount = 9,
    Protocol    = ProtocolType.UBX,
    ExtendedData = new Dictionary<string, object>
    {
        { "hdop", 1.2 },
        { "temperature", 22.5 }
    },
    IsValid = true
};

Console.WriteLine(fix.ToString());
// Output similar to:
// Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6, DeviceId: IMEI:123456789012345, 
// Timestamp: 2025-09-24 14:32:10Z, Lat: 37.7749, Lon: -12222.4194, Valid: True
```

### Example 2: Computing distance and bearing between two fixes
```csharp
LocationData fixA = GetFirstFix();   // assume populated elsewhere
LocationData fixB = GetSecondFix();

// Haversine calculation (simplified)
double R = 6371000; // Earth radius in meters
double φ1 = fixA.Latitude * Math.PI / 180;
double φ2 = fixB.Latitude * Math.PI / 180;
double Δφ = (fixB.Latitude - fixA.Latitude) * Math.PI / 180;
double Δλ = (fixB.Longitude - fixA.Longitude) * Math.PI / 180;

double a = Math.Sin(Δφ/2) * Math.Sin(Δφ/2) +
           Math.Cos(φ1) * Math.Cos(φ2) *
           Math.Sin(Δλ/2) * Math.Sin(Δλ/2);
double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));

fixA.DistanceTo = R * c;

// Bearing calculation
double y = Math.Sin(Δλ) * Math.Cos(φ2);
double x = Math.Cos(φ1) * Math.Sin(φ2) -
           Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(Δλ);
fixA.BearingTo = Math.Atan2(y, x) * 180 / Math.PI;

// Normalize bearing to 0‑360°
if (fixA.BearingTo < 0) fixA.BearingTo += 360;

Console.WriteLine($"Distance: {fixA.DistanceTo:F1} m, Bearing: {fixA.BearingTo:F1}°");
```

## Notes

- **Coordinate validity:** Latitude must be in the range `[-90, 90]` and longitude in `[-180, 180]`. Values outside these ranges will not cause exceptions but will render `IsValid` false if the consumer implements range checks.
- **Timestamp handling:** The `Timestamp` property is expected to be in UTC. Supplying a local `DateTime` without adjusting to UTC may lead to incorrect ordering or age calculations.
- **Speed and bearing when stationary:** When `Speed` is zero, the reported `Bearing` is undefined; consumers should treat bearing as irrelevant in that case.
- **ExtendedData thread‑safety:** The dictionary itself is not thread‑safe. If multiple threads read or write `ExtendedData` concurrently, external synchronization is required.
- **Immutability considerations:** The type exposes only public fields and properties; there is no built‑in immutability guarantee. Consumers should not rely on the instance remaining unchanged after publication unless they enforce it externally.
- **DistanceTo and BearingTo:** These members are intended to be populated by the consumer after calculating the geometric relationship to another fix. They do not perform any validation; setting them to nonsensical values (e.g., negative distance) will not throw exceptions.
- **Protocol enum:** Adding new values to `ProtocolType` does not break existing code; however, a switch statement lacking a `default` case may need updating to handle unknown protocols gracefully.
