#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain;

/// <summary>
/// Supported GPS tracker protocol types.
/// </summary>
public enum ProtocolType
{
    /// <summary>
    /// Unknown protocol type.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// GT06 protocol type.
    /// </summary>
    GT06 = 1,
    /// <summary>
    /// H02 protocol type.
    /// </summary>
    H02 = 2,
    /// <summary>
    /// TK103 protocol type.
    /// </summary>
    TK103 = 3
}

/// <summary>
/// Device operational status.
/// </summary>
public enum DeviceStatus
{
    /// <summary>
    /// Unknown device status.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Device is online.
    /// </summary>
    Online = 1,
    /// <summary>
    /// Device is offline.
    /// </summary>
    Offline = 2,
    /// <summary>
    /// Device is idle.
    /// </summary>
    Idle = 3,
    /// <summary>
    /// Device is moving.
    /// </summary>
    Moving = 4,
    /// <summary>
    /// Device is parked.
    /// </summary>
    Parked = 5,
    /// <summary>
    /// Device has low battery.
    /// </summary>
    LowBattery = 6,
    /// <summary>
    /// Device has signal loss.
    /// </summary>
    SignalLoss = 7
}

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
    /// Power off command.
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

/// <summary>
/// Command execution status.
/// </summary>
public enum CommandStatus
{
    /// <summary>
    /// Command is pending.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// Command has been sent.
    /// </summary>
    Sent = 1,
    /// <summary>
    /// Command has been executed.
    /// </summary>
    Executed = 2,
    /// <summary>
    /// Command has failed.
    /// </summary>
    Failed = 3,
    /// <summary>
    /// Command has been cancelled.
    /// </summary>
    Cancelled = 4,
    /// <summary>
    /// Command has timed out.
    /// </summary>
    TimedOut = 5
}

/// <summary>
/// Response message types from devices.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Unknown message type.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Acknowledgment message.
    /// </summary>
    Ack = 1,
    /// <summary>
    /// Error message.
    /// </summary>
    Error = 2,
    /// <summary>
    /// Location update message.
    /// </summary>
    LocationUpdate = 3,
    /// <summary>
    /// Status message.
    /// </summary>
    Status = 4,
    /// <summary>
    /// Alarm message.
    /// </summary>
    Alarm = 5,
    /// <summary>
    /// Heartbeat message.
    /// </summary>
    Heartbeat = 6
}

/// <summary>
/// Data transmission modes.
/// </summary>
public enum TransmissionMode
{
    /// <summary>
    /// TCP transmission mode.
    /// </summary>
    TCP = 1,
    /// <summary>
    /// UDP transmission mode.
    /// </summary>
    UDP = 2,
    /// <summary>
    /// GPRS transmission mode.
    /// </summary>
    GPRS = 3,
    /// <summary>
    /// LTE transmission mode.
    /// </summary>
    LTE = 4
}

/// <summary>
/// Geofence alert types.
/// </summary>
public enum GeofenceAlertType
{
    /// <summary>
    /// Enter geofence alert.
    /// </summary>
    Enter = 1,
    /// <summary>
    /// Exit geofence alert.
    /// </summary>
    Exit = 2,
    /// <summary>
    /// Dwell time geofence alert.
    /// </summary>
    DwellTime = 3
}

/// <summary>
/// Alarm types reported by devices.
/// </summary>
public enum AlarmType
{
    /// <summary>
    /// SOS alarm.
    /// </summary>
    Sos = 1,
    /// <summary>
    /// Overspeed alarm.
    /// </summary>
    Overspeed = 2,
    /// <summary>
    /// Harsh braking alarm.
    /// </summary>
    HarshBraking = 3,
    /// <summary>
    /// Towing alarm.
    /// </summary>
    Towing = 4,
    /// <summary>
    /// Fatigue driving alarm.
    /// </summary>
    FatigueDriving = 5,
    /// <summary>
    /// Collision alarm.
    /// </summary>
    Collision = 6,
    /// <summary>
    /// Power cut off alarm.
    /// </summary>
    PowerCutOff = 7,
    /// <summary>
    /// Low battery alarm.
    /// </summary>
    LowBattery = 8,
    /// <summary>
    /// GPS signal loss alarm.
    /// </summary>
    GpsSignalLoss = 9,
    /// <summary>
    /// Geofence violation alarm.
    /// </summary>
    GeofenceViolation = 10
}