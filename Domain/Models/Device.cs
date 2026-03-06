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
    /// Updates device as seen with current timestamp.
    /// </summary>
    public void UpdateHeartbeat(string? ipAddress = null, int port = 0)
    {
        LastSeen = DateTime.UtcNow;
        Status = DeviceStatus.Online;
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
