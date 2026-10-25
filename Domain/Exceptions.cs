#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain;

/// <summary>
/// Base exception for GPS tracker protocol operations.
/// Thrown directly for general failures that have no more specific type, such as
/// a failed weather API request in <c>WeatherApiClient</c> (wrapping the original exception).
/// Catch this type to handle any library error in one place; the derived types below
/// are more specific.
/// </summary>
public class GpsTrackerException : Exception
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">Description of the error.</param>
    public GpsTrackerException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with an error message and the exception that caused it.
    /// </summary>
    /// <param name="message">Description of the error.</param>
    /// <param name="innerException">The original exception that caused this error.</param>
    public GpsTrackerException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when protocol parsing fails. This happens when a frame fails validation
/// (<c>ProtocolParserService</c>), when a protocol-specific parser (GT06, H02, TK103)
/// throws while decoding a frame, when the frame's protocol is unsupported, or when
/// an NMEA sentence is empty, has too few fields, is an unsupported type, or has an
/// invalid coordinate, hemisphere, or fix status. Also produced by
/// <c>ParseResult&lt;T&gt;</c> when it is converted to an exception.
/// </summary>
public class ParseException : GpsTrackerException
{
    /// <summary>
    /// Gets or sets the raw input (typically hex) that failed to parse, if available.
    /// </summary>
    public string? RawData { get; set; }

    /// <summary>
    /// Gets or sets the protocol being parsed when the failure occurred.
    /// <see cref="ProtocolType.Unknown"/> when the protocol could not be determined.
    /// </summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>
    /// Gets or sets an optional machine-readable error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Initializes a new instance with a message and an optional protocol.
    /// </summary>
    /// <param name="message">Description of the parse failure.</param>
    /// <param name="protocol">The protocol being parsed, or <see cref="ProtocolType.Unknown"/>.</param>
    public ParseException(string message, ProtocolType protocol = ProtocolType.Unknown)
        : base(message)
    {
        Protocol = protocol;
    }

    /// <summary>
    /// Initializes a new instance with a message, the raw data that failed to parse,
    /// and an optional protocol.
    /// </summary>
    /// <param name="message">Description of the parse failure.</param>
    /// <param name="rawData">The raw input that failed to parse.</param>
    /// <param name="protocol">The protocol being parsed, or <see cref="ProtocolType.Unknown"/>.</param>
    public ParseException(string message, string rawData, ProtocolType protocol = ProtocolType.Unknown)
        : base(message)
    {
        RawData = rawData;
        Protocol = protocol;
    }
}

/// <summary>
/// Thrown when frame checksum validation fails, i.e. the checksum computed from the
/// frame content does not match the checksum carried in the frame. Currently thrown
/// by <c>NmeaSentenceParser</c> when an NMEA sentence's checksum is invalid.
/// Derives from <see cref="ParseException"/>, so a <c>catch (ParseException)</c>
/// also handles it.
/// </summary>
public class ChecksumException : ParseException
{
    /// <summary>
    /// Gets or sets the checksum value that was expected.
    /// </summary>
    public string? ExpectedChecksum { get; set; }

    /// <summary>
    /// Gets or sets the checksum value that was actually found in the frame.
    /// </summary>
    public string? ActualChecksum { get; set; }

    /// <summary>
    /// Initializes a new instance describing a checksum mismatch.
    /// </summary>
    /// <param name="expectedChecksum">The checksum computed from the frame content.</param>
    /// <param name="actualChecksum">The checksum found in the frame.</param>
    /// <param name="protocol">The protocol being parsed, or <see cref="ProtocolType.Unknown"/>.</param>
    public ChecksumException(string expectedChecksum, string actualChecksum, ProtocolType protocol = ProtocolType.Unknown)
        : base($"Checksum mismatch: expected {expectedChecksum}, got {actualChecksum}", protocol)
    {
        ExpectedChecksum = expectedChecksum;
        ActualChecksum = actualChecksum;
    }
}

/// <summary>
/// Thrown when a device is not found or is in an invalid state. This happens when
/// a device lookup by ID or IMEI fails (<c>DeviceService</c>, <c>LocationDataService</c>,
/// <c>JourneyService</c>), when registering a device whose IMEI already exists, or when
/// a command targets a device that is missing or not active (<c>CommandService</c>).
/// </summary>
public class DeviceException : GpsTrackerException
{
    /// <summary>
    /// Gets or sets the ID of the device involved in the error, if known.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">Description of the device error.</param>
    public DeviceException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with an error message and the affected device ID.
    /// </summary>
    /// <param name="message">Description of the device error.</param>
    /// <param name="deviceId">The ID of the affected device.</param>
    public DeviceException(string message, string deviceId)
        : base(message)
    {
        DeviceId = deviceId;
    }
}

/// <summary>
/// Thrown when command execution fails. This happens when a command cannot be found
/// by ID (<c>CommandService</c>). The <see cref="ErrorCode"/> property can carry a
/// numeric code describing the failure.
/// </summary>
public class CommandException : GpsTrackerException
{
    /// <summary>
    /// Gets or sets the ID of the command involved in the error, if known.
    /// </summary>
    public string? CommandId { get; set; }

    /// <summary>
    /// Gets or sets a numeric error code describing the failure. Defaults to <c>0</c>.
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">Description of the command error.</param>
    public CommandException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with an error message, the command ID, and an optional error code.
    /// </summary>
    /// <param name="message">Description of the command error.</param>
    /// <param name="commandId">The ID of the affected command.</param>
    /// <param name="errorCode">A numeric code describing the failure. Defaults to <c>0</c>.</param>
    public CommandException(string message, string commandId, int errorCode = 0)
        : base(message)
    {
        CommandId = commandId;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Thrown when data validation fails, such as a null or incomplete device, command,
/// or location record (<c>DeviceService</c>, <c>CommandService</c>, <c>LocationDataService</c>,
/// <c>JourneyService</c>, <c>ProtocolParserService</c>), or when a value is out of range,
/// for example an invalid latitude or longitude (<c>LocationDataService</c>) or a
/// fuel-tracking or fleet-dashboard input that fails its checks.
/// </summary>
public class ValidationException : GpsTrackerException
{
    /// <summary>
    /// Gets or sets the name of the field or parameter that failed validation, if known.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Gets or sets the value that failed validation, if known.
    /// </summary>
    public object? FieldValue { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">Description of the validation failure.</param>
    public ValidationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with an error message, the failing field name,
    /// and the offending value.
    /// </summary>
    /// <param name="message">Description of the validation failure.</param>
    /// <param name="fieldName">The name of the field or parameter that failed validation.</param>
    /// <param name="fieldValue">The value that failed validation, or <c>null</c>.</param>
    public ValidationException(string message, string fieldName, object? fieldValue = null)
        : base(message)
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
    }
}

/// <summary>
/// Thrown when a repository operation fails. Reserved for persistence failures in
/// the <c>Data/</c> layer. The library's in-memory repositories do not currently throw
/// it, so it appears only when a custom <c>IRepository</c> implementation raises it.
/// </summary>
public class RepositoryException : GpsTrackerException
{
    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">Description of the repository failure.</param>
    public RepositoryException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with an error message and the exception that caused it.
    /// </summary>
    /// <param name="message">Description of the repository failure.</param>
    /// <param name="innerException">The original exception that caused this failure.</param>
    public RepositoryException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when communication with a device times out. Reserved for timeouts on
/// device I/O. The library does not currently throw it, so it appears only when
/// calling code raises it. It is not the same as <see cref="System.TimeoutException"/>.
/// </summary>
public class TimeoutException : GpsTrackerException
{
    /// <summary>
    /// Gets or sets the ID of the device that timed out, if known.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Gets or sets how long the operation waited before timing out.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Initializes a new instance with the specified error message.
    /// </summary>
    /// <param name="message">Description of the timeout.</param>
    public TimeoutException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with an error message, the device ID, and the elapsed duration.
    /// </summary>
    /// <param name="message">Description of the timeout.</param>
    /// <param name="deviceId">The ID of the device that timed out.</param>
    /// <param name="duration">How long the operation waited before timing out.</param>
    public TimeoutException(string message, string deviceId, TimeSpan duration)
        : base(message)
    {
        DeviceId = deviceId;
        Duration = duration;
    }
}
