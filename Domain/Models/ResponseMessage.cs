#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents a response message from a tracking device.
/// </summary>
public class ResponseMessage
{
    /// <summary>
    /// Unique identifier for the response message.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Identifier of the device that sent the response.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;
    /// <summary>
    /// Identifier of the command that this response is for, if any.
    /// </summary>
    public string? CommandId { get; set; }
    /// <summary>
    /// Type of the response message.
    /// </summary>
    public MessageType Type { get; set; }
    /// <summary>
    /// Indicates whether the response indicates success.
    /// </summary>
    public bool IsSuccess { get; set; }
    /// <summary>
    /// Raw content of the response message.
    /// </summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>
    /// Parsed data from the response message, keyed by string.
    /// </summary>
    public Dictionary<string, object> ParsedData { get; set; } = [];
    /// <summary>
    /// Timestamp when the response was received (in UTC).
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Numeric error code if the response indicates an error, otherwise 0.
    /// </summary>
    public int ErrorCode { get; set; } = 0;
    /// <summary>
    /// Descriptive error message if the response indicates an error, otherwise null.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Validates response message structure.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(DeviceId) &&
               !string.IsNullOrWhiteSpace(Content);
    }

    /// <summary>
    /// Parses response content based on message type.
    /// </summary>
    public void Parse()
    {
        if (!IsValid())
            throw new InvalidOperationException("Response message is invalid");

        var parts = Content.Split(',');

        switch (Type)
        {
            case MessageType.Ack:
                ParseAck(parts);
                break;
            case MessageType.Error:
                ParseError(parts);
                break;
            case MessageType.LocationUpdate:
                ParseLocationUpdate(parts);
                break;
            case MessageType.Status:
                ParseStatus(parts);
                break;
        }
    }

    /// <summary>
    /// Parses an acknowledgment response.
    /// </summary>
    private void ParseAck(string[] parts)
    {
        IsSuccess = true;
        if (parts.Length > 1)
            ParsedData["sequence"] = parts[1];
    }

    /// <summary>
    /// Parses an error response.
    /// </summary>
    private void ParseError(string[] parts)
    {
        IsSuccess = false;
        if (parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
        {
            ErrorCode = code;
            ErrorMessage = GetErrorMessage(code);
        }
    }

    /// <summary>
    /// Parses a location update response.
    /// </summary>
    private void ParseLocationUpdate(string[] parts)
    {
        try
        {
            if (parts.Length >= 6)
            {
                ParsedData["latitude"] = double.Parse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                ParsedData["longitude"] = double.Parse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                ParsedData["speed"] = double.Parse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture);
                ParsedData["bearing"] = double.Parse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture);
                ParsedData["altitude"] = double.Parse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture);
                IsSuccess = true;
            }
        }
        catch
        {
            IsSuccess = false;
            ErrorMessage = "Failed to parse location data";
        }
    }

    /// <summary>
    /// Parses a status response.
    /// </summary>
    private void ParseStatus(string[] parts)
    {
        try
        {
            if (parts.Length >= 4)
            {
                ParsedData["battery"] = int.Parse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
                ParsedData["signal"] = int.Parse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
                ParsedData["satellites"] = int.Parse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture);
                IsSuccess = true;
            }
        }
        catch
        {
            IsSuccess = false;
            ErrorMessage = "Failed to parse status data";
        }
    }

    /// <summary>
    /// Gets the error message string for a given error code.
    /// </summary>
    private string GetErrorMessage(int code)
    {
        return code switch
        {
            1 => "Device not found",
            2 => "Invalid command format",
            3 => "Command not supported",
            4 => "Device offline",
            5 => "Authentication failed",
            _ => "Unknown error"
        };
    }

    /// <summary>
    /// Returns a string representation of the response message.
    /// </summary>
    public override string ToString() =>
        $"Response({Type}) - Device: {DeviceId} - Success: {IsSuccess}";
}