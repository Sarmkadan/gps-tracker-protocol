#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Utilities;

/// <summary>
/// Analytics service for computing device and fleet statistics.
/// Aggregates metrics for reporting and monitoring.
/// </summary>
public interface IAnalyticsService
{
    Task<DeviceAnalytics> GetDeviceAnalyticsAsync(string deviceId);
    Task<FleetAnalytics> GetFleetAnalyticsAsync();
    Task<RouteAnalytics> GetRouteAnalyticsAsync(string journeyId);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly ILocationDataService _locationService;
    private readonly IJourneyService _journeyService;
    private readonly IDeviceService _deviceService;
    private readonly ILogger<AnalyticsService> _logger;

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
