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
    /// <summary>Unknown or unrecognized message type.</summary>
    Unknown = 0,
    /// <summary>Acknowledgment message.</summary>
    Ack = 1,
    /// <summary>Error message.</summary>
    Error = 2,
    /// <summary>Location update message.</summary>
    LocationUpdate = 3,
    /// <summary>Status message.</summary>
    Status = 4,
    /// <summary>Alarm message.</summary>
    Alarm = 5,
    /// <summary>Heartbeat message.</summary>
    Heartbeat = 6
}