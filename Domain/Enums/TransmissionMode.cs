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
    /// <summary>
    /// Transmission Control Protocol (TCP) mode.
    /// </summary>
    TCP = 1,
    /// <summary>
    /// User Datagram Protocol (UDP) mode.
    /// </summary>
    UDP = 2,
    /// <summary>
    /// General Packet Radio Service (GPRS) mode.
    /// </summary>
    GPRS = 3,
    /// <summary>
    /// Long-Term Evolution (LTE) mode.
    /// </summary>
    LTE = 4
}
