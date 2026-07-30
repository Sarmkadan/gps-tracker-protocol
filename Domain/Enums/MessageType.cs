#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Response message types from devices.
/// </summary>
public enum MessageType
{
    Unknown = 0,
    Ack = 1,
    Error = 2,
    LocationUpdate = 3,
    Status = 4,
    Alarm = 5,
    Heartbeat = 6
}
