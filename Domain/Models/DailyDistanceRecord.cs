#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents the total distance travelled by a device on a specific day.
/// </summary>
public class DailyDistanceRecord
{
    /// <summary>
    /// Identifier of the device.
    /// </summary>
    public string DeviceId { get; set; } = null!;

    /// <summary>
    /// The day for which the distance is calculated (date component only).
    /// </summary>
    public DateTime Day { get; set; }

    /// <summary>
    /// Total distance in kilometres travelled on that day.
    /// </summary>
    public double DistanceKm { get; set; }
}
