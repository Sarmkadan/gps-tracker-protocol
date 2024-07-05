#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Utilities;

/// <summary>
/// Geofencing service for monitoring device locations against defined zones.
/// Detects when devices enter/exit geographic boundaries.
/// </summary>
public interface IGeofenceService
{
    void AddGeofence(string id, double centerLat, double centerLon, double radiusKm);
    bool IsInsideGeofence(string geofenceId, double latitude, double longitude);
    IEnumerable<string> GetNearbyGeofences(double latitude, double longitude, double searchRadiusKm);
}

public class GeofenceService : IGeofenceService
{
    private readonly Dictionary<string, Geofence> _geofences = new();
    private readonly ILogger<GeofenceService> _logger;

    public GeofenceService(ILogger<GeofenceService> logger)
    {
        _logger = logger;
    }

    public void AddGeofence(string id, double centerLat, double centerLon, double radiusKm)
    {
        if (!GpsUtilities.IsValidCoordinate(centerLat, centerLon))
        {
            _logger.LogWarning("Invalid geofence coordinates: {Lat}, {Lon}", centerLat, centerLon);
            return;
        }

        _geofences[id] = new Geofence
        {
            Id = id,
            CenterLatitude = centerLat,
            CenterLongitude = centerLon,
            RadiusKm = radiusKm,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Geofence added: {Id} at {Lat:F6},{Lon:F6} radius {Radius}km",
            id, centerLat, centerLon, radiusKm);
    }

    public bool IsInsideGeofence(string geofenceId, double latitude, double longitude)
    {
        if (!_geofences.TryGetValue(geofenceId, out var geofence))
            return false;

        var distance = GpsUtilities.CalculateDistanceKm(
            geofence.CenterLatitude, geofence.CenterLongitude,
            latitude, longitude);

        return distance <= geofence.RadiusKm;
    }

    public IEnumerable<string> GetNearbyGeofences(double latitude, double longitude, double searchRadiusKm)
    {
        var nearby = new List<string>();

        foreach (var geofence in _geofences.Values)
        {
            var distance = GpsUtilities.CalculateDistanceKm(
                geofence.CenterLatitude, geofence.CenterLongitude,
                latitude, longitude);

            if (distance <= searchRadiusKm + geofence.RadiusKm)
                nearby.Add(geofence.Id);
        }

        return nearby;
    }
}

public class Geofence
{
    public string Id { get; set; }
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double RadiusKm { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
