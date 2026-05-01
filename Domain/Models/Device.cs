#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents a GPS tracking device.
/// </summary>
public class Device
{
    public string Id { get; set; } = string.Empty;
    public string Imei { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public ProtocolType Protocol { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
    public DateTime LastSeen { get; set; }
    public string? IpAddress { get; set; }
    public int Port { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, string> Metadata { get; set; } = [];
    public int BatteryLevel { get; set; } = -1;
    public int SignalStrength { get; set; } = 0;
    /// <summary>Total number of packets/heartbeats received from the device.</summary>
    public int ConnectionCount { get; set; } = 0;

    /// <summary>
    /// Validates device configuration.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Imei) &&
               Imei.Length >= 14 && Imei.Length <= 16 &&
               Imei.All(char.IsDigit);
    }

    /// <summary>
    /// Updates device as seen with current timestamp and increments connection count.
    /// </summary>
    public void UpdateHeartbeat(string? ipAddress = null, int port = 0)
    {
        LastSeen = DateTime.UtcNow;
        Status = DeviceStatus.Online;
        ConnectionCount++;
        if (!string.IsNullOrWhiteSpace(ipAddress))
            IpAddress = ipAddress;
        if (port > 0)
            Port = port;
    }

    /// <summary>
    /// Checks if device is considered offline based on heartbeat timeout.
    /// </summary>
    public bool IsOffline(TimeSpan timeout)
    {
        return DateTime.UtcNow - LastSeen > timeout;
    }

    public override string ToString() =>
        $"Device({DeviceName}) - IMEI: {Imei} - {Protocol} - {Status}";
}

/// <summary>
/// Read-only snapshot of a device's connection status for fleet health monitoring.
/// Returned by the device status endpoint to expose <see cref="IsConnected"/>,
/// <see cref="LastSeen"/>, and <see cref="ConnectionCount"/> without leaking
/// internal device state.
/// </summary>
public record DeviceStatusDto
{
    public string DeviceId { get; init; } = string.Empty;
    public string Imei { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    /// <summary>True when <see cref="DeviceStatus"/> is <see cref="DeviceStatus.Online"/>.</summary>
    public bool IsConnected { get; init; }
    /// <summary>UTC timestamp of the last received packet from the device.</summary>
    public DateTime LastSeen { get; init; }
    /// <summary>Total number of packets received from the device.</summary>
    public int ConnectionCount { get; init; }
    public DeviceStatus Status { get; init; }
}
