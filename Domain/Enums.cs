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
    Unknown = 0,
    GT06 = 1,
    H02 = 2,
    TK103 = 3
}

/// <summary>
/// Device operational status.
/// </summary>
public enum DeviceStatus
{
    Unknown = 0,
    Online = 1,
    Offline = 2,
    Idle = 3,
    Moving = 4,
    Parked = 5,
    LowBattery = 6,
    SignalLoss = 7
}

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

/// <summary>
/// Response message types from devices.
/// </summary>
public enum MessageType
{
    Unknown = 0,
    Ack = 1,
    Error = 2,
    LocationUpdate = 3,
    Status = 4,
    Alarm = 5,
    Heartbeat = 6
}

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

/// <summary>
/// Geofence alert types.
/// </summary>
public enum GeofenceAlertType
{
    Enter = 1,
    Exit = 2,
    DwellTime = 3
}

/// <summary>
/// Alarm types reported by devices.
/// </summary>
public enum AlarmType
{
    Sos = 1,
    Overspeed = 2,
    HarshBraking = 3,
    Towing = 4,
    FatigueDriving = 5,
    Collision = 6,
    PowerCutOff = 7,
    LowBattery = 8,
    GpsSignalLoss = 9,
    GeofenceViolation = 10
}
