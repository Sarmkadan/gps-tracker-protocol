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
    Enter = 1,
    Exit = 2,
    DwellTime = 3
}
