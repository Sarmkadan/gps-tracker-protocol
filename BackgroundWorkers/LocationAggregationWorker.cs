#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.BackgroundWorkers;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Caching;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

/// <summary>
/// Background worker that aggregates location data for analytics.
/// Computes distance traveled, average speed, and other metrics periodically.
/// </summary>
public class LocationAggregationWorker : RecurringBackgroundWorker
{
    private readonly ILocationDataService _locationService;
    private readonly IDeviceService _deviceService;
    private readonly ICachingService _cache;

    public override string WorkerName => "LocationAggregation";

    public LocationAggregationWorker(
        ILocationDataService locationService,
        IDeviceService deviceService,
        ICachingService cache,
        ILogger<LocationAggregationWorker> logger)
        : base(logger)
    {
        _locationService = locationService;
        _deviceService = deviceService;
        _cache = cache;
        _interval = TimeSpan.FromMinutes(10);
    }

    protected override async Task ExecuteAsync()
    {
        var devices = await _deviceService.GetAllDevicesAsync();

        foreach (var device in devices)
        {
            try
            {
                await AggregateDeviceLocationAsync(device.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aggregating locations for device {DeviceId}", device.Id);
            }
        }
    }

    private async Task AggregateDeviceLocationAsync(string deviceId)
    {
        var locations = await _locationService.GetLocationHistoryAsync(deviceId, 1000);

        if (!locations.Any())
            return;

        var aggregation = new LocationAggregation
        {
            DeviceId = deviceId,
            AggregationTime = DateTime.UtcNow,
            LocationCount = locations.Count(),
            TimeSpan = DateTime.UtcNow - locations.First().Timestamp,
            MaxSpeed = locations.Max(l => l.Speed),
            MinSpeed = locations.Min(l => l.Speed),
            AverageSpeed = locations.Average(l => l.Speed),
            TotalDistance = CalculateTotalDistance(locations)
        };

        var cacheKey = CacheKeyGenerator.GetDeviceKey(deviceId) + ":aggregation";
        _cache.Set(cacheKey, aggregation, TimeSpan.FromHours(1));

        _logger.LogInformation(
            "Location aggregation updated for {DeviceId}: {Count} locations, {Distance:F2}km, {Speed:F1}km/h avg",
            deviceId, aggregation.LocationCount, aggregation.TotalDistance, aggregation.AverageSpeed);
    }

    private double CalculateTotalDistance(IEnumerable<LocationData> locations)
    {
        var locationList = locations.OrderBy(l => l.Timestamp).ToList();
        double totalDistance = 0;

        for (int i = 1; i < locationList.Count; i++)
        {
            var distance = Utilities.GpsUtilities.CalculateDistanceKm(
                locationList[i - 1].Latitude, locationList[i - 1].Longitude,
                locationList[i].Latitude, locationList[i].Longitude);

            totalDistance += distance;
        }

        return totalDistance;
    }
}

public class LocationAggregation
{
    public string DeviceId { get; set; }
    public DateTime AggregationTime { get; set; }
    public int LocationCount { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public double MaxSpeed { get; set; }
    public double MinSpeed { get; set; }
    public double AverageSpeed { get; set; }
    public double TotalDistance { get; set; }
}
