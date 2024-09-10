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
            CommandType.SetGpsInterval => Parameters.ContainsKey("interval"),
            CommandType.SetReportingServer => Parameters.ContainsKey("server_ip") && Parameters.ContainsKey("port"),
            CommandType.RequestLocation => true,
            CommandType.PowerOff => true,
            CommandType.SetGeofence => Parameters.ContainsKey("latitude") && Parameters.ContainsKey("longitude") && Parameters.ContainsKey("radius"),
            CommandType.ClearGeofence => true,
            CommandType.ResetDevice => true,
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
            CommandType.SetGpsInterval =>
                $"*HQ,{DeviceId},GPRS,{Parameters["interval"]}#",
            CommandType.SetReportingServer =>
                $"*HQ,{DeviceId},SERVER,{Parameters["server_ip"]},{Parameters["port"]}#",
            CommandType.RequestLocation =>
                $"*HQ,{DeviceId},GPS#",
            CommandType.PowerOff =>
                $"*HQ,{DeviceId},POWEROFF#",
            CommandType.ResetDevice =>
                $"*HQ,{DeviceId},RESET#",
            _ => string.Empty
        };
    }

    public override string ToString() =>
        $"Command({Type}) - Device: {DeviceId} - Status: {Status}";
}
