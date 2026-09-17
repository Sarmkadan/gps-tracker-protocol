#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Geofence alert types.
/// </summary>
public enum GeofenceAlertType
{
    /// <summary>
    /// Indicates entering a geofence.
    /// </summary>
    Enter = 1,
    /// <summary>
    /// Indicates exiting a geofence.
    /// </summary>
    Exit = 2,
    /// <summary>
    /// Indicates dwelling within a geofence for a specified time.
    /// </summary>
    DwellTime = 3
}
