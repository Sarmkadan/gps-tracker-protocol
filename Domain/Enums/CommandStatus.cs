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
    /// <summary>
    /// The command is pending execution.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// The command has been sent.
    /// </summary>
    Sent = 1,
    /// <summary>
    /// The command has been executed.
    /// </summary>
    Executed = 2,
    /// <summary>
    /// The command failed.
    /// </summary>
    Failed = 3,
    /// <summary>
    /// The command was cancelled.
    /// </summary>
    Cancelled = 4,
    /// <summary>
    /// The command timed out.
    /// </summary>
    TimedOut = 5
}
