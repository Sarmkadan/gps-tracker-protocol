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
    /// <summary>
    /// Unknown command type.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Set GPS interval command.
    /// </summary>
    SetGpsInterval = 1,
    /// <summary>
    /// Set reporting server command.
    /// </summary>
    SetReportingServer = 2,
    /// <summary>
    /// Request location command.
    /// </summary>
    RequestLocation = 3,
    /// <summary>
    /// Power off device command.
    /// </summary>
    PowerOff = 4,
    /// <summary>
    /// Set geofence command.
    /// </summary>
    SetGeofence = 5,
    /// <summary>
    /// Clear geofence command.
    /// </summary>
    ClearGeofence = 6,
    /// <summary>
    /// Reset device command.
    /// </summary>
    ResetDevice = 7,
    /// <summary>
    /// Update firmware command.
    /// </summary>
    UpdateFirmware = 8,
    /// <summary>
    /// Request status command.
    /// </summary>
    RequestStatus = 9
}