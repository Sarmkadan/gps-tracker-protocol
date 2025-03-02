// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents a trip or journey containing multiple location data points.
/// </summary>
public class Journey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<LocationData> Waypoints { get; set; } = [];
    public int Status { get; set; } = 0; // 0: ongoing, 1: completed, 2: abandoned
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Adds a location point to the journey.
    /// </summary>
    public void AddWaypoint(LocationData location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));

        if (!location.IsValid())
            throw new InvalidOperationException("Location data is invalid");

        Waypoints.Add(location);
        EndTime = location.Timestamp;
    }

    /// <summary>
    /// Calculates total distance traveled in the journey.
    /// </summary>
    public double GetTotalDistance()
    {
        if (Waypoints.Count < 2)
            return 0;

        double distance = 0;
        for (int i = 0; i < Waypoints.Count - 1; i++)
        {
            distance += Waypoints[i].DistanceTo(Waypoints[i + 1]);
        }
        return distance;
    }

    /// <summary>
    /// Calculates average speed during the journey.
    /// </summary>
    public double GetAverageSpeed()
    {
        if (Waypoints.Count < 2)
            return 0;

        var totalSpeed = Waypoints.Sum(w => w.Speed);
        return totalSpeed / Waypoints.Count;
    }

    /// <summary>
    /// Gets maximum speed recorded during journey.
    /// </summary>
    public double GetMaxSpeed()
    {
        return Waypoints.Count == 0 ? 0 : Waypoints.Max(w => w.Speed);
    }

    /// <summary>
    /// Gets duration of the journey.
    /// </summary>
    public TimeSpan GetDuration()
    {
        if (EndTime == null)
            return DateTime.UtcNow - StartTime;
        return EndTime.Value - StartTime;
    }

    /// <summary>
    /// Completes the journey and calculates summary metrics.
    /// </summary>
    public void Complete()
    {
        Status = 1;
        EndTime = DateTime.UtcNow;
        Metadata["total_distance_km"] = GetTotalDistance();
        Metadata["average_speed_kmh"] = GetAverageSpeed();
        Metadata["max_speed_kmh"] = GetMaxSpeed();
        Metadata["duration_minutes"] = GetDuration().TotalMinutes;
    }

    public override string ToString() =>
        $"Journey({Id}) - {DeviceId} - {Waypoints.Count} points - {GetDuration().TotalMinutes:F1}min";
}
