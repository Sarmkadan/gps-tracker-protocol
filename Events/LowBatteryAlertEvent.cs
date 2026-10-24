#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Events;

/// <summary>
/// Raised once when a tracked device's battery drops below the low-battery threshold.
/// Re-armed only after the battery recovers above the threshold plus the recovery margin.
/// </summary>
public class LowBatteryAlertEvent : IDomainEvent
{
    /// <summary>Gets the unique event identifier.</summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets the UTC timestamp when the low battery condition was detected.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Gets the aggregate root identifier (device ID).</summary>
    public string AggregateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device whose battery is low.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable device name.</summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the battery level in percent at the moment of detection.</summary>
    public int BatteryLevel { get; set; }

    /// <summary>Gets or sets the threshold in percent that the battery fell below.</summary>
    public int ThresholdPercent { get; set; }
}
