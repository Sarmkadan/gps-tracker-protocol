#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol;

/// <summary>
/// Protocol-specific frame constants and sizes.
/// </summary>
public static class ProtocolConstants
{
    // GT06 Protocol
    public const int GT06_MIN_FRAME_SIZE = 15;
    public const int GT06_MAX_FRAME_SIZE = 256;
    public const byte GT06_START_MARKER = 0x78;
    public const byte GT06_EXTENDED_START_MARKER = 0x79;
    public const byte GT06_END_MARKER = 0x0D;

    // H02 Protocol
    public const int H02_MIN_FRAME_SIZE = 32;
    public const int H02_MAX_FRAME_SIZE = 512;
    public const string H02_START_MARKER = "$GPRMC";
    public const string H02_HQ_START_MARKER = "*HQ";
    public const string H02_END_MARKER = "\r\n";

    // TK103 Protocol
    public const int TK103_MIN_FRAME_SIZE = 30;
    public const int TK103_MAX_FRAME_SIZE = 256;
    public const byte TK103_START_MARKER = 0x28;
    public const byte TK103_END_MARKER = 0x29;

    // Common
    public const int IMEI_LENGTH = 15;
    public const int PHONE_NUMBER_LENGTH = 11;
    public const int DEVICE_ID_MAX_LENGTH = 50;
}

/// <summary>
/// Configuration defaults and limits.
/// </summary>
public static class ConfigConstants
{
    public const int DEFAULT_GPS_INTERVAL_SECONDS = 60;
    public const int MIN_GPS_INTERVAL_SECONDS = 5;
    public const int MAX_GPS_INTERVAL_SECONDS = 3600;

    public const int DEFAULT_HEARTBEAT_INTERVAL_SECONDS = 300;
    public const int DEVICE_OFFLINE_TIMEOUT_SECONDS = 900; // 15 minutes
    public const int COMMAND_EXECUTION_TIMEOUT_SECONDS = 120;

    public const int MAX_LOCATION_HISTORY = 10000;
    public const int MAX_JOURNEY_WAYPOINTS = 50000;
    public const int BATCH_PROCESS_SIZE = 100;
}

/// <summary>
/// Coordinate and measurement bounds.
/// </summary>
public static class MeasurementBounds
{
    public const double MIN_LATITUDE = -90.0;
    public const double MAX_LATITUDE = 90.0;
    public const double MIN_LONGITUDE = -180.0;
    public const double MAX_LONGITUDE = 180.0;

    public const double MAX_SPEED_KMH = 300.0;
    public const double MAX_ALTITUDE_METERS = 9000.0;
    public const double MIN_ALTITUDE_METERS = -500.0;

    public const int MAX_SATELLITES = 32;
    public const int MAX_SIGNAL_STRENGTH = 31;
    public const int MAX_BATTERY_PERCENT = 100;
}

/// <summary>
/// Error codes for protocol operations.
/// </summary>
public static class ErrorCodes
{
    public const int OK = 0;
    public const int INVALID_FRAME = 100;
    public const int CHECKSUM_FAILED = 101;
    public const int UNKNOWN_PROTOCOL = 102;
    public const int INVALID_DATA = 103;
    public const int MISSING_FIELD = 104;

    public const int DEVICE_NOT_FOUND = 200;
    public const int DEVICE_OFFLINE = 201;
    public const int DEVICE_INVALID = 202;
    public const int DEVICE_DUPLICATE = 203;

    public const int COMMAND_FAILED = 300;
    public const int COMMAND_TIMEOUT = 301;
    public const int COMMAND_INVALID = 302;
    public const int COMMAND_REJECTED = 303;

    public const int REPOSITORY_ERROR = 400;
    public const int DATABASE_ERROR = 401;
    public const int FILE_NOT_FOUND = 402;
    public const int STORAGE_FULL = 403;
}

/// <summary>
/// Regular expression patterns for parsing.
/// </summary>
public static class RegexPatterns
{
    public const string IMEI_PATTERN = @"^\d{14,16}$";
    public const string PHONE_PATTERN = @"^[\d]{10,13}$";
    public const string IP_ADDRESS_PATTERN = @"^(\d{1,3}\.){3}\d{1,3}$";
    public const string COORDINATES_PATTERN = @"^-?\d{1,3}\.\d+$";
    public const string HEX_PATTERN = @"^[0-9A-Fa-f]+$";
}

/// <summary>
/// Alarm threshold values.
/// </summary>
public static class AlarmThresholds
{
    public const int OVERSPEED_THRESHOLD_KMH = 100;
    public const int HARSH_BRAKING_G = 8;
    public const int LOW_BATTERY_PERCENT = 20;
    public const int FATIGUE_DRIVING_HOURS = 4;
    public const int GEOFENCE_RADIUS_METERS = 100;
    public const int DWELL_TIME_MINUTES = 30;
}
