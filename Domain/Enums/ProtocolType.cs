#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Supported GPS tracker protocol types.
/// </summary>
public enum ProtocolType
{
    /// <summary>
    /// Unspecified or unrecognized protocol type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// GT06 protocol, commonly used by Chinese GPS trackers.
    /// </summary>
    GT06 = 1,

    /// <summary>
    /// H02 protocol, a text-based protocol used by various GPS trackers.
    /// </summary>
    H02 = 2,

    /// <summary>
    /// TK103 protocol, widely used by vehicle GPS trackers.
    /// </summary>
    TK103 = 3
}
