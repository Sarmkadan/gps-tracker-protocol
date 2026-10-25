# Domain Enums
This document summarizes the enums in `Domain/Enums/` of the gps-tracker-protocol library. All of them live in the `GpsTrackerProtocol.Domain.Enums` namespace. Values are listed with their underlying integer, and the meaning is taken from each enum's XML documentation.

## ProtocolType
Supported GPS tracker protocol types.

| Member | Value | Meaning |
|--------|-------|---------|
| `Unknown` | 0 | Unspecified or unrecognized protocol type. |
| `GT06` | 1 | GT06 protocol, commonly used by Chinese GPS trackers. |
| `H02` | 2 | H02 protocol, a text-based protocol used by various GPS trackers. |
| `TK103` | 3 | TK103 protocol, widely used by vehicle GPS trackers. |

## DeviceStatus
Device operational status.

| Member | Value | Meaning |
|--------|-------|---------|
| `Unknown` | 0 | The status is unknown or not yet determined. |
| `Online` | 1 | The device is online and communicating. |
| `Offline` | 2 | The device is offline or not communicating. |
| `Idle` | 3 | The device is powered on but not moving (stationary). |
| `Moving` | 4 | The device is in motion. |
| `Parked` | 5 | The device is stopped and parked. |
| `LowBattery` | 6 | The device battery is low. |
| `SignalLoss` | 7 | The device has lost signal (e.g., GPS or cellular). |

## CommandType
Command types that can be sent to devices.

| Member | Value | Meaning |
|--------|-------|---------|
| `Unknown` | 0 | Unknown command type. |
| `SetGpsInterval` | 1 | Set GPS interval command. |
| `SetReportingServer` | 2 | Set reporting server command. |
| `RequestLocation` | 3 | Request location command. |
| `PowerOff` | 4 | Power off device command. |
| `SetGeofence` | 5 | Set geofence command. |
| `ClearGeofence` | 6 | Clear geofence command. |
| `ResetDevice` | 7 | Reset device command. |
| `UpdateFirmware` | 8 | Update firmware command. |
| `RequestStatus` | 9 | Request status command. |

## CommandStatus
Command execution status.

| Member | Value | Meaning |
|--------|-------|---------|
| `Pending` | 0 | The command is pending execution. |
| `Sent` | 1 | The command has been sent. |
| `Executed` | 2 | The command has been executed. |
| `Failed` | 3 | The command failed. |
| `Cancelled` | 4 | The command was cancelled. |
| `TimedOut` | 5 | The command timed out. |

## AlarmType
Alarm types reported by devices.

| Member | Value | Meaning |
|--------|-------|---------|
| `Sos` | 1 | SOS alarm. |
| `Overspeed` | 2 | Overspeed alarm. |
| `HarshBraking` | 3 | Harsh braking alarm. |
| `Towing` | 4 | Towing alarm. |
| `FatigueDriving` | 5 | Fatigue driving alarm. |
| `Collision` | 6 | Collision alarm. |
| `PowerCutOff` | 7 | Power cut off alarm. |
| `LowBattery` | 8 | Low battery alarm. |
| `GpsSignalLoss` | 9 | GPS signal loss alarm. |
| `GeofenceViolation` | 10 | Geofence violation alarm. |

## GeofenceAlertType
Geofence alert types.

| Member | Value | Meaning |
|--------|-------|---------|
| `Enter` | 1 | Entering a geofence. |
| `Exit` | 2 | Exiting a geofence. |
| `DwellTime` | 3 | Dwelling within a geofence for a specified time. |

## MessageType
Response message types from devices.

| Member | Value | Meaning |
|--------|-------|---------|
| `Unknown` | 0 | Unknown or unrecognized message type. |
| `Ack` | 1 | Acknowledgment message. |
| `Error` | 2 | Error message. |
| `LocationUpdate` | 3 | Location update message. |
| `Status` | 4 | Status message. |
| `Alarm` | 5 | Alarm message. |
| `Heartbeat` | 6 | Heartbeat message. |

## TransmissionMode
Data transmission modes.

| Member | Value | Meaning |
|--------|-------|---------|
| `TCP` | 1 | Transmission Control Protocol (TCP) mode. |
| `UDP` | 2 | User Datagram Protocol (UDP) mode. |
| `GPRS` | 3 | General Packet Radio Service (GPRS) mode. |
| `LTE` | 4 | Long-Term Evolution (LTE) mode. |
