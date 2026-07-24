#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpsTrackerProtocol.Services;

using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol;

/// <summary>
/// Default implementation of location sanity filtering for GPS tracking data.
/// Filters out garbage GPS points: null island, invalid coordinates, no fix,
/// impossible speeds, and timestamps in the past or future.
/// </summary>
public class LocationSanityFilter : ILocationSanityFilter
{
    private readonly LocationSanityFilterConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationSanityFilter"/> class.
    /// </summary>
    /// <param name="config">Optional configuration. If null, uses default values.</param>
    public LocationSanityFilter(LocationSanityFilterConfig? config = null)
    {
        _config = config ?? new LocationSanityFilterConfig();
    }

    /// <inheritdoc/>
    public LocationSanityFilterConfig Configuration => _config;

    /// <inheritdoc/>
    public FilterResult FilterLocation(LocationData location, LocationData? previousLocation = null, double maxSpeedKmh = 300.0)
    {
        ArgumentNullException.ThrowIfNull(location);

        // Apply maxSpeedKmh parameter override if provided
        double effectiveMaxSpeed = maxSpeedKmh > 0 ? maxSpeedKmh : _config.MaxSpeedKmh;

        // Check 1: Null island rejection (0,0 coordinates)
        if (_config.RejectNullIsland && IsNullIsland(location))
        {
            return new FilterResult(
                false,
                FilterDecision.NullIsland,
                "Location is at null island (0,0)"
            );
        }

        // Check 2: Invalid coordinates rejection
        if (_config.RejectInvalidCoordinates && !IsValidCoordinates(location))
        {
            return new FilterResult(
                false,
                FilterDecision.InvalidCoordinates,
                "Location has invalid coordinates (latitude: ${location.Latitude}, longitude: ${location.Longitude})"
            );
        }

        // Check 3: No GPS fix rejection
        if (_config.RejectNoSatelliteFix && !HasValidGpsFix(location))
        {
            return new FilterResult(
                false,
                FilterDecision.NoGpsFix,
                "Location has no valid GPS fix (satellite count: ${location.SatelliteCount})"
            );
        }

        // Check 4: Timestamp validation
        if (!IsValidTimestamp(location))
        {
            return new FilterResult(
                false,
                FilterDecision.InvalidTimestamp,
                "Location timestamp is invalid (${location.Timestamp:O})"
            );
        }

        // Check 5: Altitude validation
        if (!IsValidAltitude(location))
        {
            return new FilterResult(
                false,
                FilterDecision.ImpossibleAltitude,
                "Location altitude is invalid (${location.Altitude}m)"
            );
        }

        // Check 6: Speed validation (only if we have a previous location)
        if (previousLocation != null && !IsValidSpeed(location, previousLocation, effectiveMaxSpeed))
        {
            return new FilterResult(
                false,
                FilterDecision.ImpossibleSpeed,
                "Location speed is impossible (${location.Speed} km/h between points)"
            );
        }

        // All checks passed
        return new FilterResult(true, FilterDecision.Accepted);
    }

    /// <summary>
    /// Checks if a location is at the null island (0,0).
    /// </summary>
    private static bool IsNullIsland(LocationData location)
    {
        // Use epsilon comparison to account for floating point precision
        const double epsilon = 1e-6;
        return Math.Abs(location.Latitude) < epsilon && Math.Abs(location.Longitude) < epsilon;
    }

    /// <summary>
    /// Checks if coordinates are within valid bounds.
    /// </summary>
    private static bool IsValidCoordinates(LocationData location)
    {
        return location.Latitude >= MeasurementBounds.MIN_LATITUDE &&
               location.Latitude <= MeasurementBounds.MAX_LATITUDE &&
               location.Longitude >= MeasurementBounds.MIN_LONGITUDE &&
               location.Longitude <= MeasurementBounds.MAX_LONGITUDE;
    }

    /// <summary>
    /// Checks if the location has a valid GPS fix.
    /// </summary>
    private bool HasValidGpsFix(LocationData location)
    {
        // A valid GPS fix typically requires at least 3 satellites
        // Some devices report 0 satellites when they have a fix but haven't acquired satellites yet
        // We'll use a more lenient approach: accept 0 satellites only if coordinates are valid
        // and the timestamp is reasonable
        if (location.SatelliteCount < 0)
        {
            return false;
        }

        // If we have a reasonable satellite count, accept it
        if (location.SatelliteCount >= 3)
        {
            return true;
        }

        // For 0-2 satellites, only accept if coordinates are valid and timestamp is reasonable
        // This handles cold start scenarios where the device has a fix but hasn't acquired satellites yet
        return location.SatelliteCount >= 0 && IsValidCoordinates(location) && IsValidTimestamp(location);
    }

    /// <summary>
    /// Checks if the timestamp is valid (not in the future or too far in the past).
    /// </summary>
    private bool IsValidTimestamp(LocationData location)
    {
        var now = DateTime.UtcNow;
        var maxSkew = TimeSpan.FromHours(_config.MaxTimestampSkewHours);

        if (location.Timestamp == default)
        {
            return false;
        }

        // Check if timestamp is in the future (beyond reasonable skew)
        if (location.Timestamp > now.Add(maxSkew))
        {
            return false;
        }

        // Check if timestamp is too far in the past (more than 1 year)
        if (location.Timestamp < now.AddYears(-1))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the altitude is within valid bounds.
    /// </summary>
    private bool IsValidAltitude(LocationData location)
    {
        return location.Altitude >= _config.MinAltitudeMeters &&
               location.Altitude <= _config.MaxAltitudeMeters;
    }

    /// <summary>
    /// Checks if the speed between this location and the previous location is plausible.
    /// </summary>
    private bool IsValidSpeed(LocationData current, LocationData previous, double maxSpeedKmh)
    {
        if (current.Speed < 0)
        {
            return false;
        }

        // Calculate time difference in hours
        var timeDiffHours = (current.Timestamp - previous.Timestamp).TotalHours;

        // If time difference is 0 or negative, we can't calculate speed
        if (timeDiffHours <= 0)
        {
            return true; // Accept if no time difference (stationary point)
        }

        // Calculate distance between points in km
        var distanceKm = previous.DistanceTo(current);

        // Calculate speed in km/h
        var calculatedSpeed = distanceKm / timeDiffHours;

        // Check if the calculated speed exceeds the maximum
        // Allow some tolerance for GPS error (within 10% of max speed)
        if (calculatedSpeed > maxSpeedKmh * 1.1)
        {
            return false;
        }

        // Also check if the reported speed is wildly different from calculated speed
        // This catches cases where the device reports an impossible speed
        if (current.Speed > maxSpeedKmh * 1.5 && calculatedSpeed < maxSpeedKmh * 0.5)
        {
            return false;
        }

        return true;
    }
}
