// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents a response message from a tracking device.
/// </summary>
public class ResponseMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; } = string.Empty;
    public string? CommandId { get; set; }
    public MessageType Type { get; set; }
    public bool IsSuccess { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> ParsedData { get; set; } = [];
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public int ErrorCode { get; set; } = 0;
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

    private void ParseAck(string[] parts)
    {
        IsSuccess = true;
        if (parts.Length > 1)
            ParsedData["sequence"] = parts[1];
    }

    private void ParseError(string[] parts)
    {
        IsSuccess = false;
        if (parts.Length > 1 && int.TryParse(parts[1], out var code))
        {
            ErrorCode = code;
            ErrorMessage = GetErrorMessage(code);
        }
    }

    private void ParseLocationUpdate(string[] parts)
    {
        try
        {
            if (parts.Length >= 6)
            {
                ParsedData["latitude"] = double.Parse(parts[1]);
                ParsedData["longitude"] = double.Parse(parts[2]);
                ParsedData["speed"] = double.Parse(parts[3]);
                ParsedData["bearing"] = double.Parse(parts[4]);
                ParsedData["altitude"] = double.Parse(parts[5]);
                IsSuccess = true;
            }
        }
        catch
        {
            IsSuccess = false;
            ErrorMessage = "Failed to parse location data";
        }
    }

    private void ParseStatus(string[] parts)
    {
        try
        {
            if (parts.Length >= 4)
            {
                ParsedData["battery"] = int.Parse(parts[1]);
                ParsedData["signal"] = int.Parse(parts[2]);
                ParsedData["satellites"] = int.Parse(parts[3]);
                IsSuccess = true;
            }
        }
        catch
        {
            IsSuccess = false;
            ErrorMessage = "Failed to parse status data";
        }
    }

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

    public override string ToString() =>
        $"Response({Type}) - Device: {DeviceId} - Success: {IsSuccess}";
}
