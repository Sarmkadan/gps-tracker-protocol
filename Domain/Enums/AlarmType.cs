#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Alarm types reported by devices.
/// </summary>
public enum AlarmType
{
    Sos = 1,
    Overspeed = 2,
    HarshBraking = 3,
    Towing = 4,
    FatigueDriving = 5,
    Collision = 6,
    PowerCutOff = 7,
    LowBattery = 8,
    GpsSignalLoss = 9,
    GeofenceViolation = 10
}
