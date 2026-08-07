#nullable enable
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
    /// <summary>
    /// Gets or sets the unique identifier of the journey.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the identifier of the device that recorded the journey.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the journey started.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the journey ended, or <c>null</c> if the journey is still ongoing.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the list of location points recorded during the journey.
    /// </summary>
    public List<LocationData> Waypoints { get; set; } = [];

    /// <summary>
    /// Gets or sets the journey status. 0: ongoing, 1: completed, 2: abandoned.
    /// </summary>
    public int Status { get; set; } = 0; // 0: ongoing, 1: completed, 2: abandoned

    /// <summary>
    /// Gets or sets additional metadata associated with the journey.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// Adds a location point to the journey.
    /// </summary>
    /// <param name="location">The location point to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="location"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="location"/> contains invalid data.</exception>
    public void AddWaypoint(LocationData location)
    {
        if (location is null)
            throw new ArgumentNullException(nameof(location));

        if (!location.IsValid())
            throw new InvalidOperationException("Location data is invalid");

        Waypoints.Add(location);
        EndTime = location.Timestamp;
    }

    /// <summary>
    /// Calculates total distance traveled in the journey.
    /// </summary>
    /// <returns>The total distance traveled in kilometers, or 0 if the journey has fewer than two waypoints.</returns>
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
    /// <returns>The average speed in kilometers per hour, or 0 if the journey has fewer than two waypoints.</returns>
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
    /// <returns>The maximum speed in kilometers per hour, or 0 if the journey has no waypoints.</returns>
    public double GetMaxSpeed()
    {
        return Waypoints.Count == 0 ? 0 : Waypoints.Max(w => w.Speed);
    }

    /// <summary>
    /// Gets duration of the journey.
    /// </summary>
    /// <returns>The duration of the journey. For ongoing journeys, the elapsed time since <see cref="StartTime"/> is returned.</returns>
    public TimeSpan GetDuration()
    {
        if (EndTime is null)
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

    /// <inheritdoc />
    public override string ToString() =>
        $"Journey({Id}) - {DeviceId} - {Waypoints.Count} points - {GetDuration().TotalMinutes:F1}min";
}

/// <summary>
/// Represents a period of inactivity/idle time during a journey.
/// </summary>
public class IdlePeriod
{
    /// <summary>
    /// Gets or sets the timestamp when the idle period started.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the idle period ended.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the duration of the idle period.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the location where the idle period started.
    /// </summary>
    public LocationData StartLocation { get; set; } = null!;

    /// <summary>
    /// Gets or sets the location where the idle period ended.
    /// </summary>
    public LocationData EndLocation { get; set; } = null!;

    /// <summary>
    /// Gets or sets the maximum distance in meters the device moved during the idle period.
    /// </summary>
    public double MaxDistanceMeters { get; set; }

    /// <inheritdoc />
    public override string ToString() =>
        $"IdlePeriod [{StartTime:O} - {EndTime:O}] Duration: {Duration.TotalMinutes:F1}min, Distance: {MaxDistanceMeters}m";
}
