#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

/// <summary>
/// Settings for low-battery alerting raised by the device diagnostics service.
/// An alert fires once when the battery drops below <see cref="ThresholdPercent"/>, and
/// does not fire again until the battery recovers to <see cref="ThresholdPercent"/> plus
/// <see cref="RecoveryMarginPercent"/>. The margin prevents alert flapping around the threshold.
/// </summary>
public class LowBatteryAlertOptions
{
    /// <summary>
    /// Gets or sets the battery level in percent below which an alert is raised.
    /// Range: 1 to 100.
    /// </summary>
    public int ThresholdPercent { get; set; } = 20;

    /// <summary>
    /// Gets or sets how far above <see cref="ThresholdPercent"/> the battery must recover
    /// before a new alert can be raised for the same device.
    /// Range: 0 to 100, and <see cref="ThresholdPercent"/> plus this value must not exceed 100.
    /// </summary>
    public int RecoveryMarginPercent { get; set; } = 5;
}
