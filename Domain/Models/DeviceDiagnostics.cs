#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

using GpsTrackerProtocol.Domain;

/// <summary>
/// Comprehensive health and status snapshot for a single GPS tracking device.
/// Aggregates connectivity, hardware telemetry, and data-activity metrics into
/// one response object suitable for an operator dashboard or diagnostics endpoint.
/// </summary>
public class DeviceDiagnosticsReport
{
    /// <summary>Gets or sets the device identifier this report covers.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable device name.</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the device IMEI.</summary>
    public string Imei { get; set; } = string.Empty;

    /// <summary>Gets or sets the protocol the device communicates with.</summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>Gets or sets the current operational status.</summary>
    public DeviceStatus Status { get; set; }

    // ── Connectivity ───────────────────────────────────────────────────────────

    /// <summary>Gets or sets whether the device is currently connected.</summary>
    public bool IsOnline { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the most recently received packet.</summary>
    public DateTime LastSeen { get; set; }

    /// <summary>Gets or sets how long the device has been silent since its last packet.</summary>
    public TimeSpan TimeSinceLastContact { get; set; }

    /// <summary>Gets or sets the total number of packets ever received from this device.</summary>
    public int TotalPacketsReceived { get; set; }

    /// <summary>Gets or sets the IP address of the last known connection.</summary>
    public string? IpAddress { get; set; }

    // ── Hardware telemetry ─────────────────────────────────────────────────────

    /// <summary>Gets or sets the battery level as a percentage (0-100), or -1 when unknown.</summary>
    public int BatteryLevel { get; set; }

    /// <summary>Gets or sets the signal strength in dBm (negative), or 0 when unknown.</summary>
    public int SignalStrength { get; set; }

    /// <summary>
    /// Gets or sets a qualitative signal label derived from <see cref="SignalStrength"/>.
    /// One of: <c>Excellent</c>, <c>Good</c>, <c>Fair</c>, <c>Poor</c>, <c>Unknown</c>.
    /// </summary>
    public string SignalQuality { get; set; } = "Unknown";

    // ── Location activity ──────────────────────────────────────────────────────

    /// <summary>Gets or sets the total number of location points recorded for this device.</summary>
    public int TotalLocationPoints { get; set; }

    /// <summary>Gets or sets the most recently recorded location, or <c>null</c> when none exists.</summary>
    public LocationData? LastLocation { get; set; }

    /// <summary>Gets or sets the cumulative distance driven across all completed journeys in km.</summary>
    public double TotalDistanceKm { get; set; }

    // ── Journey activity ───────────────────────────────────────────────────────

    /// <summary>Gets or sets the total number of journeys (completed and ongoing).</summary>
    public int TotalJourneys { get; set; }

    /// <summary>Gets or sets the number of currently active (ongoing) journeys.</summary>
    public int ActiveJourneys { get; set; }

    // ── Self-test ──────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the results of the most recent self-test run, if any.</summary>
    public DeviceSelfTestResult? SelfTest { get; set; }

    /// <summary>Gets or sets when this report was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a lightweight self-test that checks each diagnostic subsystem and
/// reports which checks passed or failed.
/// </summary>
public class DeviceSelfTestResult
{
    /// <summary>Gets or sets when the self-test was executed.</summary>
    public DateTime RunAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether the connectivity check passed.</summary>
    public bool ConnectivityOk { get; set; }

    /// <summary>Gets or sets whether the battery level is above the warning threshold.</summary>
    public bool BatteryOk { get; set; }

    /// <summary>Gets or sets whether the signal strength is above the warning threshold.</summary>
    public bool SignalOk { get; set; }

    /// <summary>Gets or sets whether at least one location point has been recorded.</summary>
    public bool LocationDataOk { get; set; }

    /// <summary>Gets or sets the list of human-readable warnings raised during the test.</summary>
    public IReadOnlyList<string> Warnings { get; set; } = [];

    /// <summary>Gets or sets whether all checks passed with no warnings.</summary>
    public bool AllOk => ConnectivityOk && BatteryOk && SignalOk && LocationDataOk && !Warnings.Any();
}

/// <summary>Fleet-wide aggregate produced by <c>IDeviceDiagnosticsService.GetFleetHealthReportAsync</c>.</summary>
public class FleetHealthReport
{
    /// <summary>Gets or sets the total number of registered devices.</summary>
    public int TotalDevices { get; set; }

    /// <summary>Gets or sets how many devices are currently online.</summary>
    public int OnlineDevices { get; set; }

    /// <summary>Gets or sets how many devices are currently offline.</summary>
    public int OfflineDevices { get; set; }

    /// <summary>Gets or sets how many devices have a battery level below the warning threshold.</summary>
    public int LowBatteryDevices { get; set; }

    /// <summary>Gets or sets how many devices have weak signal strength.</summary>
    public int WeakSignalDevices { get; set; }

    /// <summary>Gets or sets the per-device diagnostics included in this report.</summary>
    public IReadOnlyList<DeviceDiagnosticsReport> DeviceReports { get; set; } = [];

    /// <summary>Gets or sets when the report was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
