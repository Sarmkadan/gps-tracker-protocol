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
    /// <summary>Indicates an SOS alarm.</summary>
    Sos = 1,
    /// <summary>Indicates an overspeed alarm.</summary>
    Overspeed = 2,
    /// <summary>Indicates a harsh braking alarm.</summary>
    HarshBraking = 3,
    /// <summary>Indicates a towing alarm.</summary>
    Towing = 4,
    /// <summary>Indicates a fatigue driving alarm.</summary>
    FatigueDriving = 5,
    /// <summary>Indicates a collision alarm.</summary>
    Collision = 6,
    /// <summary>Indicates a power cut off alarm.</summary>
    PowerCutOff = 7,
    /// <summary>Indicates a low battery alarm.</summary>
    LowBattery = 8,
    /// <summary>Indicates a GPS signal loss alarm.</summary>
    GpsSignalLoss = 9,
    /// <summary>Indicates a geofence violation alarm.</summary>
    GeofenceViolation = 10
}
