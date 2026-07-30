#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Data transmission modes.
/// </summary>
public enum TransmissionMode
{
    TCP = 1,
    UDP = 2,
    GPRS = 3,
    LTE = 4
}
