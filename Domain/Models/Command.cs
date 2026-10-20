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
    /// <summary>Unique identifier for this command instance.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Identifier of the target tracking device.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Strongly-typed command type indicating the action to perform.</summary>
    public CommandType Type { get; set; }

    /// <summary>Key-value parameters required by the command type.</summary>
    public Dictionary<string, object> Parameters { get; set; } = [];

    /// <summary>UTC timestamp when the command was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the command was executed, or <c>null</c> if not yet executed.</summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>Current execution status of the command.</summary>
    public CommandStatus Status { get; set; } = CommandStatus.Pending;

    /// <summary>Execution priority; lower values indicate higher priority.</summary>
    public int Priority { get; set; } = 0;

    /// <summary>Number of retry attempts made so far.</summary>
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
    /// <summary>Maximum number of retry attempts before the command is marked as failed.</summary>
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
