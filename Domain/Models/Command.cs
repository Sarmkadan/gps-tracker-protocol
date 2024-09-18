#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents a command to be sent to a tracking device.
/// </summary>
public class Command
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; } = string.Empty;
    public CommandType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAt { get; set; }
    public CommandStatus Status { get; set; } = CommandStatus.Pending;
    public int Priority { get; set; } = 0;
    public int RetryCount { get; set; } = 0;

    /// <summary>Free-form command type label, distinct from the strongly-typed <see cref="Type"/> enum.</summary>
    public string CommandType { get; set; } = string.Empty;

    /// <summary>Raw payload sent to the device for this command.</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the command was transmitted to the device.</summary>
    public DateTime? SentTime { get; set; }

    /// <summary>Whether the command has been transmitted to the device.</summary>
    public bool IsSent { get; set; }

    /// <summary>Whether the device has acknowledged receipt of the command.</summary>
    public bool IsAcknowledged { get; set; }

    /// <summary>UTC timestamp when the device acknowledged the command.</summary>
    public DateTime? AcknowledgedTime { get; set; }
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Validates command structure.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(DeviceId))
            return false;

        return Type switch
        {
            GpsTrackerProtocol.Domain.CommandType.SetGpsInterval => Parameters.ContainsKey("interval"),
            GpsTrackerProtocol.Domain.CommandType.SetReportingServer => Parameters.ContainsKey("server_ip") && Parameters.ContainsKey("port"),
            GpsTrackerProtocol.Domain.CommandType.RequestLocation => true,
            GpsTrackerProtocol.Domain.CommandType.PowerOff => true,
            GpsTrackerProtocol.Domain.CommandType.SetGeofence => Parameters.ContainsKey("latitude") && Parameters.ContainsKey("longitude") && Parameters.ContainsKey("radius"),
            GpsTrackerProtocol.Domain.CommandType.ClearGeofence => true,
            GpsTrackerProtocol.Domain.CommandType.ResetDevice => true,
            _ => false
        };
    }

    /// <summary>
    /// Marks command as executed.
    /// </summary>
    public void Execute()
    {
        ExecutedAt = DateTime.UtcNow;
        Status = CommandStatus.Executed;
    }

    /// <summary>
    /// Increments retry counter and checks if max retries exceeded.
    /// </summary>
    public bool CanRetry()
    {
        if (RetryCount >= MaxRetries)
        {
            Status = CommandStatus.Failed;
            return false;
        }
        RetryCount++;
        return true;
    }

    /// <summary>
    /// Gets command as formatted string for transmission.
    /// </summary>
    public string ToFormattedCommand()
    {
        return Type switch
        {
            GpsTrackerProtocol.Domain.CommandType.SetGpsInterval =>
                $"*HQ,{DeviceId},GPRS,{Parameters["interval"]}#",
            GpsTrackerProtocol.Domain.CommandType.SetReportingServer =>
                $"*HQ,{DeviceId},SERVER,{Parameters["server_ip"]},{Parameters["port"]}#",
            GpsTrackerProtocol.Domain.CommandType.RequestLocation =>
                $"*HQ,{DeviceId},GPS#",
            GpsTrackerProtocol.Domain.CommandType.PowerOff =>
                $"*HQ,{DeviceId},POWEROFF#",
            GpsTrackerProtocol.Domain.CommandType.ResetDevice =>
                $"*HQ,{DeviceId},RESET#",
            _ => string.Empty
        };
    }

    public override string ToString() =>
        $"Command({Type}) - Device: {DeviceId} - Status: {Status}";
}
