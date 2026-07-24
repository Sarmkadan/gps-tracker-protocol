# GPS Fix Quality / Plausibility Filtering

## Overview

This document describes the GPS fix quality filtering feature implemented to reject garbage GPS points before they are stored in the repository.

## Problem Statement

Tracking devices routinely emit invalid GPS points that corrupt journey data:
- **Null island coordinates** (0,0) on cold start
- **Invalid coordinates** (latitude > 90, longitude < -180)
- **No GPS fix flag** (satellite count issues)
- **Timestamps in 1970 or the future** (device clock problems)
- **Physically impossible jumps** (>300 km/h between points)
- **Impossible altitudes** (>9000m or <-500m)

## Solution

The `LocationSanityFilter` service applies comprehensive quality checks before accepting location points into journeys.

## Implementation Details

### 1. FilterDecision Enum

Defines all possible filter outcomes:

```csharp
public enum FilterDecision
{
    Accepted,           // Point passed all checks
    NullIsland,          // Point is at (0,0)
    InvalidCoordinates,   // Coordinates out of bounds
    NoGpsFix,            // No valid GPS fix
    InvalidTimestamp,      // Timestamp is in future or too far past
    ImpossibleSpeed,       // Speed between points is impossible
    ImpossibleAltitude,    // Altitude is out of reasonable range
    ImpossibleBearing      // Bearing change is impossible
}
```

### 2. FilterResult Struct

Returns structured filtering results:

```csharp
public record struct FilterResult(
    bool IsAccepted,
    FilterDecision Decision,
    string? Details = null
);
```

### 3. ILocationSanityFilter Interface

```csharp
public interface ILocationSanityFilter
{
    FilterResult FilterLocation(
        LocationData location,
        LocationData? previousLocation = null,
        double maxSpeedKmh = 300.0
    );

    LocationSanityFilterConfig Configuration { get; }
}
```

### 4. LocationSanityFilter Implementation

The default implementation checks:

1. **Null Island Rejection**: Points at (0,0) are rejected
2. **Coordinate Validation**: Latitude [-90,90], Longitude [-180,180]
3. **GPS Fix Validation**: 
   - Rejects negative satellite counts
   - Accepts 0 satellites only for cold start scenarios with valid coordinates/timestamps
   - Requires ≥3 satellites for normal operation
4. **Timestamp Validation**:
   - Rejects default(DateTime)
   - Rejects timestamps in the future (>1 hour skew by default)
   - Rejects timestamps more than 1 year in the past
5. **Altitude Validation**: [-500m, 9000m]
6. **Speed Validation**:
   - Calculates actual speed between consecutive points
   - Rejects speeds >300 km/h (configurable)
   - Handles time differences correctly

### 5. JourneyService Integration

The `JourneyService.AddWaypointAsync()` method now:

```csharp
public async Task<bool> AddWaypointAsync(string journeyId, LocationData location)
{
    // ... existing validation ...

    // Apply GPS fix quality filtering
    var previousLocation = journey.Waypoints.Count > 0 ? journey.Waypoints[^1] : null;
    var filterResult = _locationSanityFilter.FilterLocation(location, previousLocation);

    if (!filterResult.IsAccepted)
    {
        // Location was rejected - return false
        return false;
    }

    journey.AddWaypoint(location);
    await _journeyRepository.UpdateAsync(journey);
    return true;
}
```

## Configuration

The filter supports configurable thresholds via `LocationSanityFilterConfig`:

```csharp
public record LocationSanityFilterConfig
{
    public double MaxSpeedKmh { get; init; } = 300.0;
    public double MaxAltitudeMeters { get; init; } = 9000.0;
    public double MinAltitudeMeters { get; init; } = -500.0;
    public double MaxTimestampSkewHours { get; init; } = 1.0;
    public bool RejectNoSatelliteFix { get; init; } = true;
    public bool RejectNullIsland { get; init; } = true;
    public bool RejectInvalidCoordinates { get; init; } = true;
}
```

## Usage Examples

### Basic Usage

```csharp
var filter = new LocationSanityFilter();
var result = filter.FilterLocation(location);

if (result.IsAccepted)
{
    // Store the location
}
else
{
    // Log rejection reason: result.Decision
}
```

### With Previous Point (for speed validation)

```csharp
var result = filter.FilterLocation(currentLocation, previousLocation);
```

### Custom Configuration

```csharp
var config = new LocationSanityFilterConfig
{
    MaxSpeedKmh = 1000, // For aircraft tracking
    RejectNullIsland = false // Allow null island for testing
};
var filter = new LocationSanityFilter(config);
```

## Dependency Injection

The filter is automatically registered in `DependencyInjection.cs`:

```csharp
services.AddSingleton<ILocationSanityFilter, LocationSanityFilter>();
```

It's injected into `JourneyService` automatically.

## Testing

Comprehensive tests are provided in:
- `tests/gps-tracker-protocol.Tests/Services/LocationSanityFilterTests.cs`

Test coverage includes:
- Null location handling
- Valid location acceptance
- Null island rejection
- Invalid coordinate rejection
- GPS fix validation
- Future/past timestamp rejection
- Altitude validation
- Speed validation between points
- Cold start scenarios
- Configuration flexibility

Run tests:
```bash
dotnet test tests/gps-tracker-protocol.Tests/gps-tracker-protocol.Tests.csproj \
    --filter "FullyQualifiedName~LocationSanityFilterTests"
```

## Metrics Integration

In a production environment, rejected points should be:
1. Counted in metrics (Prometheus/Grafana)
2. Logged for debugging (structured logging)
3. Optionally sent to error tracking (Sentry/ELK)

The filter returns detailed rejection reasons that can be used for metric labels.

## Performance

- **Time Complexity**: O(1) per location point
- **Space Complexity**: O(1)
- **Dependencies**: None (pure computation)
- **Thread Safe**: Yes (stateless service)

## Backward Compatibility

- Existing code continues to work (filter is optional in JourneyService constructor)
- No breaking changes to existing APIs
- Graceful degradation (returns false for rejected points instead of throwing)

## Future Enhancements

Possible improvements:
1. Add configurable region-specific coordinate bounds
2. Support for geofencing pre-filtering
3. Adaptive speed thresholds based on device type
4. Machine learning-based anomaly detection
5. Integration with existing validation pipeline

## Files Modified/Created

### New Files:
- `Services/ILocationSanityFilter.cs` - Interface definition
- `Services/LocationSanityFilter.cs` - Implementation
- `tests/gps-tracker-protocol.Tests/Services/LocationSanityFilterTests.cs` - Tests
- `examples/GpsFilteringDemo.cs` - Usage examples
- `docs/LOCATION_SANITY_FILTER.md` - This documentation

### Modified Files:
- `Services/JourneyService.cs` - Added filtering to AddWaypointAsync
- `Configuration/DependencyInjection.cs` - Registered filter as singleton

## Validation

All changes compile successfully:
```bash
dotnet build gps-tracker-protocol.sln
```

All new tests pass:
```bash
dotnet test tests/gps-tracker-protocol.Tests/gps-tracker-protocol.Tests.csproj \
    --filter "FullyQualifiedName~LocationSanityFilterTests"
```

No existing tests were broken by this change.
