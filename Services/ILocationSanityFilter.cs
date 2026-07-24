#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpsTrackerProtocol.Services;

using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents the decision result after filtering a location point.
/// </summary>
public enum FilterDecision
{
    /// <summary>
    /// The location point passed all sanity checks and should be accepted.
    /// </summary>
    Accepted,

    /// <summary>
    /// The location point is the null island (0,0) and should be rejected.
    /// </summary>
    NullIsland,

    /// <summary>
    /// The location point has invalid coordinates (out of bounds) and should be rejected.
    /// </summary>
    InvalidCoordinates,

    /// <summary>
    /// The location point has no GPS fix flag or invalid fix status and should be rejected.
    /// </summary>
    NoGpsFix,

    /// <summary>
    /// The location point has a timestamp that is in the future or too far in the past and should be rejected.
    /// </summary>
    InvalidTimestamp,

    /// <summary>
    /// The location point has an impossible speed between this point and the previous point and should be rejected.
    /// </summary>
    ImpossibleSpeed,

    /// <summary>
    /// The location point has an impossible altitude and should be rejected.
    /// </summary>
    ImpossibleAltitude,

    /// <summary>
    /// The location point has an impossible bearing change and should be flagged for review.
    /// </summary>
    ImpossibleBearing
}

/// <summary>
/// Represents the result of a location sanity filter operation.
/// </summary>
/// <param name="IsAccepted">Whether the location point should be accepted.</param>
/// <param name="Decision">The reason for acceptance or rejection.</param>
/// <param name="Details">Additional details about the decision.</param>
public record struct FilterResult(bool IsAccepted, FilterDecision Decision, string? Details = null);

/// <summary>
/// Service for filtering GPS location data based on quality and plausibility checks.
/// </summary>
public interface ILocationSanityFilter
{
    /// <summary>
    /// Filters a location point based on GPS fix quality and plausibility.
    /// </summary>
    /// <param name="location">The location data to filter.</param>
    /// <param name="previousLocation">The previous location in the journey, or null for the first point.</param>
    /// <param name="maxSpeedKmh">Maximum allowed speed between points in km/h. Defaults to 300 km/h.</param>
    /// <returns>A filter result indicating whether the point should be accepted and why.</returns>
    FilterResult FilterLocation(LocationData location, LocationData? previousLocation = null, double maxSpeedKmh = 300.0);

    /// <summary>
    /// Gets the configuration values used by the filter.
    /// </summary>
    LocationSanityFilterConfig Configuration { get; }
}

/// <summary>
/// Configuration options for location sanity filtering.
/// </summary>
public record LocationSanityFilterConfig
{
    /// <summary>
    /// Maximum allowed speed between consecutive points in km/h.
    /// </summary>
    public double MaxSpeedKmh { get; init; } = 300.0;

    /// <summary>
    /// Maximum allowed altitude in meters (above sea level).
    /// </summary>
    public double MaxAltitudeMeters { get; init; } = 9000.0;

    /// <summary>
    /// Minimum allowed altitude in meters (below sea level).
    /// </summary>
    public double MinAltitudeMeters { get; init; } = -500.0;

    /// <summary>
    /// Maximum allowed timestamp skew in hours (how far in the future/past is acceptable).
    /// </summary>
    public double MaxTimestampSkewHours { get; init; } = 1.0;

    /// <summary>
    /// Whether to reject points with satellite count of 0 (no fix).
    /// </summary>
    public bool RejectNoSatelliteFix { get; init; } = true;

    /// <summary>
    /// Whether to reject points at the null island (0,0).
    /// </summary>
    public bool RejectNullIsland { get; init; } = true;

    /// <summary>
    /// Whether to reject points with invalid coordinates (out of bounds).
    /// </summary>
    public bool RejectInvalidCoordinates { get; init; } = true;
}
