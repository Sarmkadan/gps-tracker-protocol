#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain;

/// <summary>
/// Base exception for GPS tracker protocol operations.
/// </summary>
public class GpsTrackerException : Exception
{
    public GpsTrackerException(string message) : base(message) { }
    public GpsTrackerException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when protocol parsing fails.
/// </summary>
public class ParseException : GpsTrackerException
{
    public string? RawData { get; set; }
    public ProtocolType Protocol { get; set; }

    public ParseException(string message, ProtocolType protocol = ProtocolType.Unknown)
        : base(message)
    {
        Protocol = protocol;
    }

    public ParseException(string message, string rawData, ProtocolType protocol = ProtocolType.Unknown)
        : base(message)
    {
        RawData = rawData;
        Protocol = protocol;
    }
}

/// <summary>
/// Thrown when frame checksum validation fails.
/// </summary>
public class ChecksumException : ParseException
{
    public string? ExpectedChecksum { get; set; }
    public string? ActualChecksum { get; set; }

    public ChecksumException(string expectedChecksum, string actualChecksum, ProtocolType protocol = ProtocolType.Unknown)
        : base($"Checksum mismatch: expected {expectedChecksum}, got {actualChecksum}", protocol)
    {
        ExpectedChecksum = expectedChecksum;
        ActualChecksum = actualChecksum;
    }
}

/// <summary>
/// Thrown when device is not found or invalid.
/// </summary>
public class DeviceException : GpsTrackerException
{
    public string? DeviceId { get; set; }

    public DeviceException(string message) : base(message) { }

    public DeviceException(string message, string deviceId)
        : base(message)
    {
        DeviceId = deviceId;
    }
}

/// <summary>
/// Thrown when command execution fails.
/// </summary>
public class CommandException : GpsTrackerException
{
    public string? CommandId { get; set; }
    public int ErrorCode { get; set; }

    public CommandException(string message) : base(message) { }

    public CommandException(string message, string commandId, int errorCode = 0)
        : base(message)
    {
        CommandId = commandId;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Thrown when data validation fails.
/// </summary>
public class ValidationException : GpsTrackerException
{
    public string? FieldName { get; set; }
    public object? FieldValue { get; set; }

    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, string fieldName, object? fieldValue = null)
        : base(message)
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
    }
}

/// <summary>
/// Thrown when repository operation fails.
/// </summary>
public class RepositoryException : GpsTrackerException
{
    public RepositoryException(string message) : base(message) { }
    public RepositoryException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when communication with device times out.
/// </summary>
public class TimeoutException : GpsTrackerException
{
    public string? DeviceId { get; set; }
    public TimeSpan Duration { get; set; }

    public TimeoutException(string message) : base(message) { }

    public TimeoutException(string message, string deviceId, TimeSpan duration)
        : base(message)
    {
        DeviceId = deviceId;
        Duration = duration;
    }
}
