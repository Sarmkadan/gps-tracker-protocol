#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Enums;

/// <summary>
/// Command types that can be sent to devices.
/// </summary>
public enum CommandType
{
    Unknown = 0,
    SetGpsInterval = 1,
    SetReportingServer = 2,
    RequestLocation = 3,
    PowerOff = 4,
    SetGeofence = 5,
    ClearGeofence = 6,
    ResetDevice = 7,
    UpdateFirmware = 8,
    RequestStatus = 9
}
