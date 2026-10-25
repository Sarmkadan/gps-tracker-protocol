#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Utilities;

/// <summary>
/// Simulation service for generating synthetic GPS data for testing and demo.
/// Creates realistic device movements with proper timing and coordinates.
/// </summary>
public interface ISimulationService
{
    Task<IEnumerable<LocationData>> SimulateRouteAsync(string deviceId,
        double startLat, double startLon, double endLat, double endLon, int points);
    Task<LocationData> GenerateRandomLocationAsync(string deviceId,
        double centerLat, double centerLon, double radiusKm);
}

/// <summary>
/// Default <see cref="ISimulationService"/> implementation. Generates synthetic GT06 locations
/// and persists each one through <see cref="ILocationDataService"/>.
/// </summary>
public class SimulationService : ISimulationService
{
    private readonly ILocationDataService _locationService;
    private readonly ILogger<SimulationService> _logger;
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationService"/> class.
    /// </summary>
    /// <param name="locationService">Service used to store the generated locations.</param>
    /// <param name="logger">Logger for simulation diagnostics.</param>
    public SimulationService(
        ILocationDataService locationService,
        ILogger<SimulationService> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Simulates a straight-line route between two coordinates at an assumed average speed of
    /// 50 km/h, with evenly spaced points and randomized speed, altitude, and accuracy.
    /// Each point is stored through the location service as it is generated.
    /// </summary>
    /// <param name="deviceId">Identifier of the device the locations belong to.</param>
    /// <param name="startLat">Latitude of the route start, in degrees.</param>
    /// <param name="startLon">Longitude of the route start, in degrees.</param>
    /// <param name="endLat">Latitude of the route end, in degrees.</param>
    /// <param name="endLon">Longitude of the route end, in degrees.</param>
    /// <param name="points">Number of points to generate, including both endpoints.
    /// Must be at least 2.</param>
    /// <returns>The generated locations, ordered from start to end.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is less than 2.</exception>
    public async Task<IEnumerable<LocationData>> SimulateRouteAsync(string deviceId,
        double startLat, double startLon, double endLat, double endLon, int points)
    {
        if (points < 2)
            throw new ArgumentException("Points must be at least 2");

        var locations = new List<LocationData>();
        var distance = GpsUtilities.CalculateDistanceKm(startLat, startLon, endLat, endLon);
        var bearing = GpsUtilities.CalculateBearing(startLat, startLon, endLat, endLon);

        var currentTime = DateTime.UtcNow;
        var estimatedDuration = TimeSpan.FromHours(distance / 50); // Assume 50 km/h average
        var timeBetweenPoints = estimatedDuration / (points - 1);

        for (int i = 0; i < points; i++)
        {
            var progress = (double)i / (points - 1);
            var latitude = startLat + (endLat - startLat) * progress;
            var longitude = startLon + (endLon - startLon) * progress;

            var location = new LocationData
            {
                DeviceId = deviceId,
                Latitude = latitude,
                Longitude = longitude,
                Speed = 50.0 + (_random.NextDouble() - 0.5) * 10, // 45-55 km/h with variation
                Bearing = bearing,
                Altitude = 100.0 + (_random.NextDouble() * 50),
                Timestamp = currentTime.Add(timeBetweenPoints * i),
                SatelliteCount = 12 + _random.Next(-2, 3),
                Accuracy = 5.0 + _random.NextDouble() * 5,
                Protocol = ProtocolType.GT06
            };

            locations.Add(location);
            await _locationService.StoreLocationAsync(location).ConfigureAwait(false);
        }

        _logger.LogInformation("Simulated route: {DeviceId}, {Points} points, {Distance:F2}km, {Bearing:F1}°",
            deviceId, points, distance, bearing);

        return locations;
    }

    /// <summary>
    /// Generates a single random location within a circle around a center point, with
    /// randomized speed, heading, altitude, and accuracy. The location is stored
    /// through the location service before it is returned.
    /// </summary>
    /// <param name="deviceId">Identifier of the device the location belongs to.</param>
    /// <param name="centerLat">Latitude of the circle center, in degrees.</param>
    /// <param name="centerLon">Longitude of the circle center, in degrees.</param>
    /// <param name="radiusKm">Radius of the circle, in kilometers.</param>
    /// <returns>The generated location.</returns>
    public async Task<LocationData> GenerateRandomLocationAsync(string deviceId,
        double centerLat, double centerLon, double radiusKm)
    {
        // Generate random point within circle using polar coordinates
        var angle = _random.NextDouble() * 2 * Math.PI;
        var r = _random.NextDouble() * radiusKm;

        var latOffset = (r / 111.2); // 1 degree latitude ≈ 111.2 km
        var lonOffset = (r / (111.2 * Math.Cos(centerLat * Math.PI / 180)));

        var latitude = centerLat + (Math.Cos(angle) * latOffset);
        var longitude = centerLon + (Math.Sin(angle) * lonOffset);

        var location = new LocationData
        {
            DeviceId = deviceId,
            Latitude = latitude,
            Longitude = longitude,
            Speed = _random.NextDouble() * 100,
            Bearing = _random.NextDouble() * 360,
            Altitude = 100.0 + (_random.NextDouble() * 200),
            Timestamp = DateTime.UtcNow,
            SatelliteCount = 8 + _random.Next(0, 16),
            Accuracy = 3.0 + (_random.NextDouble() * 10),
            Protocol = ProtocolType.GT06
        };

        await _locationService.StoreLocationAsync(location).ConfigureAwait(false);
        return location;
    }
}
