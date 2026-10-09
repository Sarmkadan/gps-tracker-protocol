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
    /// <summary>
    /// Registers or replaces a circular geofence. Invalid coordinates are logged and ignored.
    /// </summary>
    /// <param name="id">Unique identifier of the geofence. An existing geofence with the same id is overwritten.</param>
    /// <param name="centerLat">Latitude of the zone center, in degrees.</param>
    /// <param name="centerLon">Longitude of the zone center, in degrees.</param>
    /// <param name="radiusKm">Zone radius in kilometers.</param>
    void AddGeofence(string id, double centerLat, double centerLon, double radiusKm);

    /// <summary>
    /// Checks whether a position lies inside the specified geofence.
    /// </summary>
    /// <param name="geofenceId">Identifier of the geofence to check.</param>
    /// <param name="latitude">Latitude of the position, in degrees.</param>
    /// <param name="longitude">Longitude of the position, in degrees.</param>
    /// <returns>
    /// <c>true</c> if the distance to the center is less than or equal to the radius;
    /// <c>false</c> if the point is outside or the geofence does not exist.
    /// </returns>
    bool IsInsideGeofence(string geofenceId, double latitude, double longitude);

    /// <summary>
    /// Returns the ids of geofences that could be near a position.
    /// </summary>
    /// <param name="latitude">Latitude of the position, in degrees.</param>
    /// <param name="longitude">Longitude of the position, in degrees.</param>
    /// <param name="searchRadiusKm">Search radius around the position, in kilometers.</param>
    /// <returns>
    /// Ids of geofences whose center is within <paramref name="searchRadiusKm"/> plus their own radius
    /// of the position. This is a candidate list: a match does not guarantee the position is inside the zone.
    /// </returns>
    IEnumerable<string> GetNearbyGeofences(double latitude, double longitude, double searchRadiusKm);
}

/// <summary>
/// In-memory implementation of <see cref="IGeofenceService"/>. Geofences are stored in a dictionary
/// keyed by id and are not thread-safe.
/// </summary>
public class GeofenceService : IGeofenceService
{
    private readonly Dictionary<string, Geofence> _geofences = new();
    private readonly ILogger<GeofenceService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeofenceService"/> class.
    /// </summary>
    /// <param name="logger">Logger used for geofence registration and validation messages.</param>
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

/// <summary>
/// Circular geographic zone monitored by <see cref="IGeofenceService"/>.
/// </summary>
public class Geofence
{
    /// <summary>Unique identifier of the geofence.</summary>
    public string Id { get; set; }

    /// <summary>Latitude of the zone center, in degrees.</summary>
    public double CenterLatitude { get; set; }

    /// <summary>Longitude of the zone center, in degrees.</summary>
    public double CenterLongitude { get; set; }

    /// <summary>Zone radius in kilometers.</summary>
    public double RadiusKm { get; set; }

    /// <summary>Optional human-readable name of the zone. Not set by <c>AddGeofence</c>.</summary>
    public string Name { get; set; }

    /// <summary>UTC timestamp of when the geofence was registered.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Arbitrary additional data attached to the geofence. Empty by default.</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
