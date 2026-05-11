#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Controls how a journey is replayed by <c>IRouteReplayService</c>.
/// </summary>
public class ReplayOptions
{
    /// <summary>
    /// Gets or sets the speed multiplier applied to the original timestamps.
    /// A value of <c>2</c> replays the route at twice real-time speed.
    /// Must be greater than zero.  Defaults to <c>1.0</c> (real-time).
    /// </summary>
    public double SpeedMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the zero-based index of the first waypoint to include.
    /// Defaults to <c>0</c> (start of journey).
    /// </summary>
    public int StartIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets the zero-based index of the last waypoint to include (inclusive).
    /// Negative values mean "include everything to the end".  Defaults to <c>-1</c>.
    /// </summary>
    public int EndIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the UTC time that serves as the logical start of the replay output.
    /// Waypoint timestamps are rebased from this point.  When <c>null</c> the original
    /// journey timestamps are preserved.
    /// </summary>
    public DateTime? RebaseToUtc { get; set; }
}

/// <summary>
/// A single waypoint emitted during a route replay, paired with its replay-adjusted timestamp.
/// </summary>
public class ReplayFrame
{
    /// <summary>Gets or sets the sequential position of this frame within the replay (zero-based).</summary>
    public int Index { get; set; }

    /// <summary>Gets or sets the location data for this waypoint.</summary>
    public LocationData Location { get; set; } = default!;

    /// <summary>
    /// Gets or sets the wall-clock time at which this frame should be delivered during playback.
    /// Takes <see cref="ReplayOptions.SpeedMultiplier"/> and <see cref="ReplayOptions.RebaseToUtc"/>
    /// into account.
    /// </summary>
    public DateTime ReplayTimestamp { get; set; }

    /// <summary>Gets or sets the elapsed replay time since the first frame.</summary>
    public TimeSpan ElapsedReplay { get; set; }

    /// <summary>Gets or sets the cumulative distance from the first frame to this frame in km.</summary>
    public double CumulativeDistanceKm { get; set; }
}

/// <summary>
/// The complete output of a replay operation.
/// </summary>
public class RouteReplayResult
{
    /// <summary>Gets or sets the journey that was replayed.</summary>
    public string JourneyId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device associated with the replayed journey.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the options that were used to produce this replay.</summary>
    public ReplayOptions Options { get; set; } = new();

    /// <summary>Gets or sets the ordered sequence of replay frames.</summary>
    public IReadOnlyList<ReplayFrame> Frames { get; set; } = [];

    /// <summary>Gets or sets the total distance covered by the replayed segment in km.</summary>
    public double TotalDistanceKm { get; set; }

    /// <summary>Gets or sets the real-world duration of the replayed segment.</summary>
    public TimeSpan OriginalDuration { get; set; }

    /// <summary>Gets or sets the duration at the configured speed multiplier.</summary>
    public TimeSpan ReplayDuration { get; set; }

    /// <summary>Gets or sets when this replay result was produced.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
