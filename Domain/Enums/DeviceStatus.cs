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
    /// <summary>
    /// The status is unknown or not yet determined.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The device is online and communicating.
    /// </summary>
    Online = 1,
    /// <summary>
    /// The device is offline or not communicating.
    /// </summary>
    Offline = 2,
    /// <summary>
    /// The device is powered on but not moving (stationary).
    /// </summary>
    Idle = 3,
    /// <summary>
    /// The device is in motion.
    /// </summary>
    Moving = 4,
    /// <summary>
    /// The device is stopped and parked.
    /// </summary>
    Parked = 5,
    /// <summary>
    /// The device battery is low.
    /// </summary>
    LowBattery = 6,
    /// <summary>
    /// The device has lost signal (e.g., GPS or cellular).
    /// </summary>
    SignalLoss = 7
}