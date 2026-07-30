#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Device operational status.
/// </summary>
public enum DeviceStatus
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Idle = 3,
    Moving = 4,
    Parked = 5,
    LowBattery = 6,
    SignalLoss = 7
}
