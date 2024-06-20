// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.BackgroundWorkers;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Caching;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Utilities;
using GpsTrackerProtocol.Services;

/// <summary>
/// Background worker that analyzes completed journeys for patterns and insights.
/// Detects speeding, idle time, and efficiency metrics.
/// </summary>
public class JourneyAnalyticsWorker : RecurringBackgroundWorker
{
    private readonly IJourneyService _journeyService;
    private readonly IDeviceService _deviceService;
    private readonly ICachingService _cache;

    public override string WorkerName => "JourneyAnalytics";

    public JourneyAnalyticsWorker(
        IJourneyService journeyService,
        IDeviceService deviceService,
        ICachingService cache,
        ILogger<JourneyAnalyticsWorker> logger)
        : base(logger)
    {
        _journeyService = journeyService;
        _deviceService = deviceService;
        _cache = cache;
        _interval = TimeSpan.FromMinutes(15);
    }

    protected override async Task ExecuteAsync()
    {
        var devices = await _deviceService.GetAllDevicesAsync();

        foreach (var device in devices)
        {
            try
            {
                await AnalyzeDeviceJourneysAsync(device.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing journeys for device {DeviceId}", device.Id);
            }
        }
    }

    private async Task AnalyzeDeviceJourneysAsync(string deviceId)
    {
        var journeys = await _journeyService.GetJourneyHistoryAsync(deviceId);
        var completedJourneys = journeys.Where(j => j.Status == 1).ToList();

        if (!completedJourneys.Any())
            return;

        var analytics = new JourneyAnalytics
        {
            DeviceId = deviceId,
            AnalysisTime = DateTime.UtcNow,
            TotalJourneys = completedJourneys.Count,
            TotalDistanceKm = completedJourneys.Sum(j => j.GetTotalDistance()),
            TotalDurationHours = completedJourneys.Sum(j => j.GetDuration().TotalHours),
            AverageSpeedKmh = completedJourneys.Average(j =>
                j.Waypoints.Any() ? j.Waypoints.Average(w => w.Speed) : 0),
            SpeedingIncidents = DetectSpeedingIncidents(completedJourneys),
            IdleTimePercentage = CalculateIdleTimePercentage(completedJourneys)
        };

        var cacheKey = CacheKeyGenerator.GetDeviceKey(deviceId) + ":analytics";
        _cache.Set(cacheKey, analytics, TimeSpan.FromHours(2));

        _logger.LogInformation(
            "Journey analytics for {DeviceId}: {Journeys} journeys, {Distance:F2}km, {Speed:F1}km/h avg",
            deviceId, analytics.TotalJourneys, analytics.TotalDistanceKm, analytics.AverageSpeedKmh);
    }

    private int DetectSpeedingIncidents(List<Domain.Models.Journey> journeys)
    {
        const double speedLimit = 100; // km/h
        int incidents = 0;

        foreach (var journey in journeys.Where(j => j.Status == 1))
        {
            var speedingWaypoints = journey.Waypoints.Where(w => w.Speed > speedLimit).ToList();
            if (speedingWaypoints.Any())
                incidents += speedingWaypoints.Count;
        }

        return incidents;
    }

    private double CalculateIdleTimePercentage(List<Domain.Models.Journey> journeys)
    {
        const double idleSpeedThreshold = 5; // km/h
        double totalIdleSeconds = 0;
        double totalSeconds = 0;

        foreach (var journey in journeys.Where(j => j.Status == 1))
        {
            for (int i = 1; i < journey.Waypoints.Count; i++)
            {
                var current = journey.Waypoints[i];
                var previous = journey.Waypoints[i - 1];

                var timeDiff = (current.Timestamp - previous.Timestamp).TotalSeconds;
                totalSeconds += timeDiff;

                if (current.Speed < idleSpeedThreshold)
                    totalIdleSeconds += timeDiff;
            }
        }

        return totalSeconds > 0 ? (totalIdleSeconds / totalSeconds) * 100 : 0;
    }
}

public class JourneyAnalytics
{
    public string DeviceId { get; set; }
    public DateTime AnalysisTime { get; set; }
    public int TotalJourneys { get; set; }
    public double TotalDistanceKm { get; set; }
    public double TotalDurationHours { get; set; }
    public double AverageSpeedKmh { get; set; }
    public int SpeedingIncidents { get; set; }
    public double IdleTimePercentage { get; set; }
}
