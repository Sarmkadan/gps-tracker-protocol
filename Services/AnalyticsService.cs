#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Utilities;
using GpsTrackerProtocol.Formatting;

/// <summary>
/// Analytics service for computing device and fleet statistics.
/// Aggregates metrics for reporting and monitoring.
/// </summary>
public interface IAnalyticsService
{
    Task<DeviceAnalytics> GetDeviceAnalyticsAsync(string deviceId);
    Task<FleetAnalytics> GetFleetAnalyticsAsync();
    Task<RouteAnalytics> GetRouteAnalyticsAsync(string journeyId);
    Task<JourneyWithIdlePeriods> GetJourneyWithIdlePeriodsAsync(string journeyId);
    /// <summary>
    /// Generates a CSV report of per‑device daily travelled distance for the given date range.
    /// </summary>
    Task<string> GetDailyDistanceReportCsvAsync(DateTime start, DateTime end);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly ILocationDataService _locationService = null!;
    private readonly IJourneyService _journeyService = null!;
    private readonly IDeviceService _deviceService = null!;
    private readonly ILogger<AnalyticsService> _logger = null!;
    private readonly IRepository<Journey>? _legacyJourneyRepository;
    private readonly IRepository<LocationData>? _legacyLocationDataRepository;

    public AnalyticsService(
        ILocationDataService locationService,
        IJourneyService journeyService,
        IDeviceService deviceService,
        ILogger<AnalyticsService> logger)
    {
        _locationService = locationService;
        _journeyService = journeyService;
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>
    /// Constructs the service directly from generic journey and location data repositories.
    /// </summary>
    public AnalyticsService(IRepository<Journey> journeyRepository, IRepository<LocationData> locationDataRepository)
    {
        _legacyJourneyRepository = journeyRepository;
        _legacyLocationDataRepository = locationDataRepository;
    }

    /// <summary>
    /// Gets the total number of journeys using the generic journey repository.
    /// </summary>
    public async Task<int> GetTotalJourneysAsync()
    {
        if (_legacyJourneyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        var journeys = await _legacyJourneyRepository.GetAllAsync().ConfigureAwait(false);
        return journeys.Count();
    }

    /// <summary>
    /// Gets the average journey duration using the generic journey repository.
    /// </summary>
    public async Task<TimeSpan> GetAverageJourneyDurationAsync()
    {
        if (_legacyJourneyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        var journeys = (await _legacyJourneyRepository.GetAllAsync().ConfigureAwait(false)).ToList();
        if (journeys.Count == 0)
            return TimeSpan.Zero;

        var totalTicks = journeys.Sum(j => (j.EndTime ?? DateTime.UtcNow).Subtract(j.StartTime).Ticks);
        return TimeSpan.FromTicks(totalTicks / journeys.Count);
    }

    /// <summary>
    /// Gets the device ID with the most journeys using the generic journey repository.
    /// </summary>
    public async Task<string?> GetMostActiveDeviceAsync()
    {
        if (_legacyJourneyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        var journeys = await _legacyJourneyRepository.GetAllAsync().ConfigureAwait(false);
        return journeys
            .GroupBy(j => j.DeviceId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
    }

    public async Task<DeviceAnalytics> GetDeviceAnalyticsAsync(string deviceId)
    {
        var locations = await _locationService.GetLocationHistoryAsync(deviceId, 1000).ConfigureAwait(false);
        var journeys = await _journeyService.GetJourneyHistoryAsync(deviceId).ConfigureAwait(false);
        var completedJourneys = journeys.Where(j => j.Status == 1).ToList();

        var analytics = new DeviceAnalytics
        {
            DeviceId = deviceId,
            ComputedAt = DateTime.UtcNow,
            TotalLocationPoints = locations.Count(),
            AverageSpeed = locations.Any() ? locations.Average(l => l.Speed) : 0,
            MaxSpeed = locations.Any() ? locations.Max(l => l.Speed) : 0,
            MinSpeed = locations.Any() ? locations.Min(l => l.Speed) : 0,
            TotalJourneys = completedJourneys.Count,
            TotalDistance = completedJourneys.Sum(j => j.GetTotalDistance()),
            TotalDurationHours = completedJourneys.Sum(j => j.GetDuration().TotalHours),
            AverageSatellites = locations.Any() ? locations.Average(l => l.SatelliteCount) : 0
        };

        return analytics;
    }

    public async Task<FleetAnalytics> GetFleetAnalyticsAsync()
    {
        var devices = await _deviceService.GetAllDevicesAsync().ConfigureAwait(false);
        var deviceAnalytics = new List<DeviceAnalytics>();

        foreach (var device in devices)
        {
            var analytics = await GetDeviceAnalyticsAsync(device.Id).ConfigureAwait(false);
            deviceAnalytics.Add(analytics);
        }

        return new FleetAnalytics
        {
            ComputedAt = DateTime.UtcNow,
            TotalDevices = devices.Count(),
            ActiveDevices = devices.Count(d => d.IsActive),
            TotalDistance = deviceAnalytics.Sum(a => a.TotalDistance),
            TotalLocations = deviceAnalytics.Sum(a => a.TotalLocationPoints),
            AverageFleetSpeed = deviceAnalytics.Average(a => a.AverageSpeed),
            DeviceAnalytics = deviceAnalytics
        };
    }

    public async Task<RouteAnalytics> GetRouteAnalyticsAsync(string journeyId)
    {
        var journeys = await _journeyService.GetJourneyHistoryAsync("").ConfigureAwait(false);
        var journey = journeys.FirstOrDefault(j => j.Id == journeyId);

        if (journey is null)
            throw new KeyNotFoundException($"Journey {journeyId} not found");

        var routeAnalytics = new RouteAnalytics
        {
            JourneyId = journeyId,
            DeviceId = journey.DeviceId,
            StartTime = journey.StartTime,
            EndTime = journey.EndTime,
            Duration = journey.GetDuration(),
            TotalDistance = journey.GetTotalDistance(),
            WaypointCount = journey.Waypoints.Count,
            AverageSpeed = journey.Waypoints.Any() ? journey.Waypoints.Average(w => w.Speed) : 0,
            MaxSpeed = journey.Waypoints.Any() ? journey.Waypoints.Max(w => w.Speed) : 0,
            MinSpeed = journey.Waypoints.Any() ? journey.Waypoints.Min(w => w.Speed) : 0,
            MaxAltitude = journey.Waypoints.Any() ? journey.Waypoints.Max(w => w.Altitude) : 0,
            MinAltitude = journey.Waypoints.Any() ? journey.Waypoints.Min(w => w.Altitude) : 0
        };

        if (journey.Waypoints.Count > 1)
        {
            var bounds = CalculateBoundingBox(journey.Waypoints);
            routeAnalytics.BoundingBox = bounds;
            routeAnalytics.ZoomLevel = GpsUtilities.CalculateZoomLevel(
                bounds.MinLat, bounds.MaxLat, bounds.MinLon, bounds.MaxLon);
        }

        return routeAnalytics;
    }

    private BoundingBox CalculateBoundingBox(IEnumerable<LocationData> waypoints)
    {
        var points = waypoints.ToList();
        return new BoundingBox
        {
            MinLat = points.Min(w => w.Latitude),
            MaxLat = points.Max(w => w.Latitude),
            MinLon = points.Min(w => w.Longitude),
            MaxLon = points.Max(w => w.Longitude)
        };
    }

    /// <summary>
    /// Gets a journey with detected idle periods.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <returns>A journey with idle periods.</returns>
    public async Task<JourneyWithIdlePeriods> GetJourneyWithIdlePeriodsAsync(string journeyId)
    {
        if (string.IsNullOrWhiteSpace(journeyId))
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));

        var journey = await _journeyService.GetJourneyAsync(journeyId).ConfigureAwait(false);
        if (journey is null)
            throw new KeyNotFoundException($"Journey {journeyId} not found");

        var idlePeriods = await _journeyService.GetIdlePeriodsAsync(journeyId).ConfigureAwait(false);
        var totalIdleMinutes = idlePeriods.Sum(ip => ip.Duration.TotalMinutes);
        var totalDurationMinutes = journey.GetDuration().TotalMinutes;
        var idlePercentage = totalDurationMinutes > 0 ? (totalIdleMinutes / totalDurationMinutes) * 100 : 0;

        return new JourneyWithIdlePeriods
        {
            Journey = journey,
            IdlePeriods = idlePeriods,
            TotalIdleTimeMinutes = totalIdleMinutes,
            IdlePercentage = idlePercentage
        };
    }

    /// <summary>
    /// Generates a CSV report of per‑device daily travelled distance for the supplied date range.
    /// </summary>
    public async Task<string> GetDailyDistanceReportCsvAsync(DateTime start, DateTime end)
    {
        var devices = await _deviceService.GetAllDevicesAsync().ConfigureAwait(false);
        var records = new List<DailyDistanceRecord>();

        foreach (var device in devices)
        {
            // Retrieve all location points for the device; the service currently limits by count,
            // so we request a very large number to effectively get the full history.
            var locations = await _locationService.GetLocationHistoryAsync(device.Id, int.MaxValue).ConfigureAwait(false);

            var filtered = locations
                .Where(l => l.Timestamp >= start && l.Timestamp <= end)
                .OrderBy(l => l.Timestamp)
                .ToList();

            var dayGroups = filtered.GroupBy(l => l.Timestamp.Date);
            foreach (var dayGroup in dayGroups)
            {
                var dayLocations = dayGroup.OrderBy(l => l.Timestamp).ToList();
                double distance = 0;
                for (int i = 1; i < dayLocations.Count; i++)
                {
                    var prev = dayLocations[i - 1];
                    var curr = dayLocations[i];
                    distance += GpsUtilities.CalculateDistanceKm(
                        prev.Latitude, prev.Longitude,
                        curr.Latitude, curr.Longitude);
                }

                records.Add(new DailyDistanceRecord
                {
                    DeviceId = device.Id,
                    Day = dayGroup.Key,
                    DistanceKm = Math.Round(distance, 2)
                });
            }
        }

        var formatter = new CsvFormatter();
        return formatter.FormatDailyDistanceReport(records);
    }
}

public class DeviceAnalytics
{
    public string DeviceId { get; set; }
    public DateTime ComputedAt { get; set; }
    public int TotalLocationPoints { get; set; }
    public double AverageSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double MinSpeed { get; set; }
    public int TotalJourneys { get; set; }
    public double TotalDistance { get; set; }
    public double TotalDurationHours { get; set; }
    public double AverageSatellites { get; set; }
}

public class FleetAnalytics
{
    public DateTime ComputedAt { get; set; }
    public int TotalDevices { get; set; }
    public int ActiveDevices { get; set; }
    public double TotalDistance { get; set; }
    public int TotalLocations { get; set; }
    public double AverageFleetSpeed { get; set; }
    public List<DeviceAnalytics> DeviceAnalytics { get; set; }
}

public class RouteAnalytics
{
    public string JourneyId { get; set; }
    public string DeviceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public double TotalDistance { get; set; }
    public int WaypointCount { get; set; }
    public double AverageSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double MinSpeed { get; set; }
    public double MaxAltitude { get; set; }
    public double MinAltitude { get; set; }
    public BoundingBox BoundingBox { get; set; }
    public int ZoomLevel { get; set; }
}

public class BoundingBox
{
    public double MinLat { get; set; }
    public double MaxLat { get; set; }
    public double MinLon { get; set; }
    public double MaxLon { get; set; }
}

/// <summary>
/// Represents a journey with detected idle periods.
/// </summary>
public class JourneyWithIdlePeriods
{
    public Journey Journey { get; set; } = null!;
    public IEnumerable<IdlePeriod> IdlePeriods { get; set; } = Enumerable.Empty<IdlePeriod>();
    public double TotalIdleTimeMinutes { get; set; }
    public double IdlePercentage { get; set; }
}
