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
    Unknown = 0,
    GT06 = 1,
    H02 = 2,
    TK103 = 3
}
