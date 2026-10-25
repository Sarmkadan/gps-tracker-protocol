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
    /// <summary>Gets or sets the unique identifier of the device.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the International Mobile Equipment Identity (IMEI) number of the device.</summary>
    public string Imei { get; set; } = string.Empty;

    /// <summary>Gets or sets a human-readable name assigned to the device.</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the communication protocol used by the device.</summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>Gets or sets the current connection status of the device.</summary>
    public DeviceStatus Status { get; set; } = DeviceStatus.Offline;

    /// <summary>Gets or sets the UTC timestamp of the last received communication from the device.</summary>
    public DateTime LastSeen { get; set; }

    /// <summary>Gets or sets the IP address from which the device last connected, or null if unknown.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Gets or sets the port number from which the device last connected.</summary>
    public int Port { get; set; }

    /// <summary>Gets or sets a value indicating whether the device is active and should be tracked.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets arbitrary key-value metadata associated with the device.</summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>Gets or sets the battery level as a percentage (0-100), or -1 if unknown.</summary>
    public int BatteryLevel { get; set; } = -1;

    /// <summary>Gets or sets the signal strength indicator, or 0 if unknown.</summary>
    public int SignalStrength { get; set; } = 0;
    /// <summary>Total number of packets/heartbeats received from the device.</summary>
    public int ConnectionCount { get; set; } = 0;

    /// <summary>UTC timestamp when this device was registered in the system.</summary>
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

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
        $"Device({Id}) {DeviceName} - IMEI: {Imei} - {Protocol} - {Status}";
}

/// <summary>
/// Read-only snapshot of a device's connection status for fleet health monitoring.
/// Returned by the device status endpoint to expose <see cref="IsConnected"/>,
/// <see cref="LastSeen"/>, and <see cref="ConnectionCount"/> without leaking
/// internal device state.
/// </summary>
public record DeviceStatusDto
{
    /// <summary>Gets the unique identifier of the device.</summary>
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>Gets the IMEI number of the device.</summary>
    public string Imei { get; init; } = string.Empty;

    /// <summary>Gets the human-readable name of the device.</summary>
    public string DeviceName { get; init; } = string.Empty;
    /// <summary>True when <see cref="DeviceStatus"/> is <see cref="DeviceStatus.Online"/>.</summary>
    public bool IsConnected { get; init; }
    /// <summary>UTC timestamp of the last received packet from the device.</summary>
    public DateTime LastSeen { get; init; }
    /// <summary>Total number of packets received from the device.</summary>
    public int ConnectionCount { get; init; }
    /// <summary>Gets the current connection status of the device.</summary>
    public DeviceStatus Status { get; init; }
}
