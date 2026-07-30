#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Command execution status.
/// </summary>
public enum CommandStatus
{
    Pending = 0,
    Sent = 1,
    Executed = 2,
    Failed = 3,
    Cancelled = 4,
    TimedOut = 5
}
