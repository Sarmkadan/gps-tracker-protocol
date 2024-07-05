#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Formatting;

using System.Text.Json;
using System.Text.Json.Serialization;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// GeoJSON formatter for mapping integration (Leaflet, Mapbox, etc).
/// Converts location data and journeys to RFC 7946 compliant GeoJSON.
/// </summary>
public interface IGeoJsonFormatter
{
    string FormatLocation(LocationData location);
    string FormatTrack(Journey journey);
    string FormatLocationCollection(IEnumerable<LocationData> locations);
}

public class GeoJsonFormatter : IGeoJsonFormatter
{
    public string FormatLocation(LocationData location)
    {
        var feature = new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new GeoJsonGeometry
            {
                Type = "Point",
                Coordinates = new[] { location.Longitude, location.Latitude }
            },
            Properties = new Dictionary<string, object>
            {
                { "device_id", location.DeviceId },
                { "timestamp", location.Timestamp.ToString("o") },
                { "speed", location.Speed },
                { "bearing", location.Bearing },
                { "altitude", location.Altitude },
                { "accuracy", location.Accuracy },
                { "satellites", location.SatelliteCount },
                { "protocol", location.Protocol.ToString() }
            }
        };

        return JsonSerializer.Serialize(feature, new JsonSerializerOptions { WriteIndented = true });
    }

    public string FormatTrack(Journey journey)
    {
        var coordinates = journey.Waypoints
            .Select(w => new[] { w.Longitude, w.Latitude })
            .Cast<object>()
            .ToList();

        var feature = new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new GeoJsonGeometry
            {
                Type = "LineString",
                Coordinates = coordinates
            },
            Properties = new Dictionary<string, object>
            {
                { "journey_id", journey.Id },
                { "device_id", journey.DeviceId },
                { "start_time", journey.StartTime.ToString("o") },
                { "end_time", journey.EndTime?.ToString("o") ?? "in_progress" },
                { "distance_km", journey.GetTotalDistance() },
                { "duration_minutes", journey.GetDuration().TotalMinutes },
                { "waypoint_count", journey.Waypoints.Count },
                { "average_speed", journey.Waypoints.Count > 0 ? journey.Waypoints.Average(w => w.Speed) : 0 }
            }
        };

        return JsonSerializer.Serialize(feature, new JsonSerializerOptions { WriteIndented = true });
    }

    public string FormatLocationCollection(IEnumerable<LocationData> locations)
    {
        var features = locations.Select(location => new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new GeoJsonGeometry
            {
                Type = "Point",
                Coordinates = new[] { location.Longitude, location.Latitude }
            },
            Properties = new Dictionary<string, object>
            {
                { "device_id", location.DeviceId },
                { "timestamp", location.Timestamp.ToString("o") },
                { "speed", location.Speed },
                { "bearing", location.Bearing }
            }
        }).ToList();

        var featureCollection = new GeoJsonFeatureCollection
        {
            Type = "FeatureCollection",
            Features = features
        };

        return JsonSerializer.Serialize(featureCollection, new JsonSerializerOptions { WriteIndented = true });
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public class GeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry Geometry { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; }
}

public class GeoJsonGeometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("coordinates")]
    public object Coordinates { get; set; }
}

public class GeoJsonFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("features")]
    public List<GeoJsonFeature> Features { get; set; }
}
