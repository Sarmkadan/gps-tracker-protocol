#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Replays the waypoints of a completed journey in chronological order, optionally
/// adjusting playback speed and rebasing timestamps.
/// </summary>
public interface IRouteReplayService
{
    /// <summary>
    /// Builds a <see cref="RouteReplayResult"/> for the specified journey.
    /// The result contains every <see cref="ReplayFrame"/> in sequence with
    /// replay-adjusted timestamps computed from <paramref name="options"/>.
    /// </summary>
    /// <param name="journeyId">ID of the completed journey to replay.</param>
    /// <param name="options">Replay configuration (speed, slice, rebase time).</param>
    /// <returns>A fully populated <see cref="RouteReplayResult"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the journey does not exist, is still ongoing, or has fewer than
    /// two waypoints.
    /// </exception>
    Task<RouteReplayResult> ReplayJourneyAsync(string journeyId, ReplayOptions? options = null);

    /// <summary>
    /// Returns a lightweight summary of a journey's waypoints without applying any
    /// timing adjustments — useful for previewing available replay data.
    /// </summary>
    /// <param name="journeyId">ID of the journey to summarise.</param>
    Task<RouteReplaySummary> GetReplaySummaryAsync(string journeyId);
}

/// <summary>
/// High-level statistics about what a replay will cover, returned before committing
/// to a full <see cref="RouteReplayResult"/> build.
/// </summary>
public record RouteReplaySummary(
    string JourneyId,
    string DeviceId,
    int WaypointCount,
    double TotalDistanceKm,
    TimeSpan OriginalDuration,
    DateTime StartTime,
    DateTime EndTime);

/// <summary>
/// Implementation of <see cref="IRouteReplayService"/>.
/// All business logic is stateless; each call is independent.
/// </summary>
public class RouteReplayService : IRouteReplayService
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly ILogger<RouteReplayService> _logger;

    /// <summary>Initialises the service with required dependencies.</summary>
    public RouteReplayService(IUnitOfWork unitOfWork, ILogger<RouteReplayService> logger)
    {
        _journeyRepository = unitOfWork.Journeys;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<RouteReplayResult> ReplayJourneyAsync(
        string journeyId,
        ReplayOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(journeyId);

        options ??= new ReplayOptions();

        if (options.SpeedMultiplier <= 0)
            throw new ArgumentException("SpeedMultiplier must be greater than zero.", nameof(options));

        var journey = await _journeyRepository.GetByIdAsync(journeyId).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Journey '{journeyId}' not found.");

        if (journey.Status == 0)
            throw new InvalidOperationException($"Journey '{journeyId}' is still ongoing and cannot be replayed.");

        if (journey.Waypoints.Count < 2)
            throw new InvalidOperationException($"Journey '{journeyId}' has fewer than 2 waypoints and cannot be replayed.");

        var ordered = journey.Waypoints
            .OrderBy(w => w.Timestamp)
            .ToList();

        var startIdx = Math.Max(0, options.StartIndex);
        var endIdx   = options.EndIndex < 0 ? ordered.Count - 1 : Math.Min(options.EndIndex, ordered.Count - 1);

        if (startIdx > endIdx)
            throw new ArgumentException("StartIndex cannot exceed EndIndex after clamping to the waypoint range.");

        var slice = ordered.Skip(startIdx).Take(endIdx - startIdx + 1).ToList();

        var rebaseOrigin = options.RebaseToUtc ?? slice[0].Timestamp;
        var originalOrigin = slice[0].Timestamp;

        var frames = new List<ReplayFrame>(slice.Count);
        double cumulativeKm = 0;

        for (var i = 0; i < slice.Count; i++)
        {
            var wp = slice[i];
            var originalElapsed = wp.Timestamp - originalOrigin;
            var replayElapsed   = TimeSpan.FromSeconds(originalElapsed.TotalSeconds / options.SpeedMultiplier);

            if (i > 0)
                cumulativeKm += slice[i - 1].DistanceTo(wp);

            frames.Add(new ReplayFrame
            {
                Index                = i,
                Location             = wp,
                ReplayTimestamp      = rebaseOrigin + replayElapsed,
                ElapsedReplay        = replayElapsed,
                CumulativeDistanceKm = cumulativeKm
            });
        }

        var originalDuration = slice[^1].Timestamp - slice[0].Timestamp;
        var replayDuration   = TimeSpan.FromSeconds(originalDuration.TotalSeconds / options.SpeedMultiplier);

        _logger.LogInformation(
            "Route replay built for journey {JourneyId}: {Count} frames, {Distance:F2} km, multiplier {Multiplier}x",
            journeyId, frames.Count, cumulativeKm, options.SpeedMultiplier);

        return new RouteReplayResult
        {
            JourneyId        = journeyId,
            DeviceId         = journey.DeviceId,
            Options          = options,
            Frames           = frames,
            TotalDistanceKm  = cumulativeKm,
            OriginalDuration = originalDuration,
            ReplayDuration   = replayDuration
        };
    }

    /// <inheritdoc/>
    public async Task<RouteReplaySummary> GetReplaySummaryAsync(string journeyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(journeyId);

        var journey = await _journeyRepository.GetByIdAsync(journeyId).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Journey '{journeyId}' not found.");

        var ordered = journey.Waypoints.OrderBy(w => w.Timestamp).ToList();
        var totalKm = journey.GetTotalDistance();

        var start = ordered.Count > 0 ? ordered[0].Timestamp  : journey.StartTime;
        var end   = ordered.Count > 0 ? ordered[^1].Timestamp : journey.EndTime ?? journey.StartTime;

        return new RouteReplaySummary(
            journeyId,
            journey.DeviceId,
            ordered.Count,
            totalKm,
            end - start,
            start,
            end);
    }
}
