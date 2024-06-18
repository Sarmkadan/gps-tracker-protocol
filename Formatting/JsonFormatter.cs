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
/// JSON serialization for GPS frames and location data.
/// Provides compact and pretty-printed output formats.
/// </summary>
public interface IJsonFormatter
{
    string Format(LocationData location, bool prettyPrint = false);
    string Format(GpsFrame frame, bool prettyPrint = false);
    string Format(Device device, bool prettyPrint = false);
    string Format(Journey journey, bool prettyPrint = false);
    T Deserialize<T>(string json);
}

public class JsonFormatter : IJsonFormatter
{
    private readonly JsonSerializerOptions _compactOptions;
    private readonly JsonSerializerOptions _prettyOptions;

    public JsonFormatter()
    {
        _compactOptions = new JsonSerializerOptions { WriteIndented = false };
        _prettyOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public string Format(LocationData location, bool prettyPrint = false)
    {
        var dto = new LocationDataDto
        {
            DeviceId = location.DeviceId,
            Timestamp = location.Timestamp.ToString("o"),
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Speed = location.Speed,
            Bearing = location.Bearing,
            Altitude = location.Altitude,
            Accuracy = location.Accuracy,
            SatelliteCount = location.SatelliteCount,
            Protocol = location.Protocol.ToString()
        };

        return JsonSerializer.Serialize(dto, prettyPrint ? _prettyOptions : _compactOptions);
    }

    public string Format(GpsFrame frame, bool prettyPrint = false)
    {
        var dto = new GpsFrameDto
        {
            FrameId = frame.FrameId,
            Protocol = frame.Protocol.ToString(),
            RawDataHex = Convert.ToHexString(frame.RawData),
            ReceivedAt = frame.ReceivedAt.ToString("o"),
            SourceAddress = frame.SourceAddress,
            SourcePort = frame.SourcePort,
            IsValidChecksum = frame.IsValidChecksum
        };

        return JsonSerializer.Serialize(dto, prettyPrint ? _prettyOptions : _compactOptions);
    }

    public string Format(Device device, bool prettyPrint = false)
    {
        var dto = new DeviceDto
        {
            Id = device.Id,
            Imei = device.Imei,
            DeviceName = device.DeviceName,
            Protocol = device.Protocol.ToString(),
            IsActive = device.IsActive,
            Status = device.Status.ToString(),
            LastSeen = device.LastSeen.ToString("o")
        };

        return JsonSerializer.Serialize(dto, prettyPrint ? _prettyOptions : _compactOptions);
    }

    public string Format(Journey journey, bool prettyPrint = false)
    {
        var dto = new JourneyDto
        {
            Id = journey.Id,
            DeviceId = journey.DeviceId,
            StartTime = journey.StartTime.ToString("o"),
            EndTime = journey.EndTime?.ToString("o"),
            WaypointCount = journey.Waypoints.Count,
            TotalDistanceKm = journey.GetTotalDistance(),
            AverageSpeed = journey.Waypoints.Count > 0
                ? journey.Waypoints.Average(w => w.Speed)
                : 0,
            Status = journey.Status == 1 ? "Completed" : "In Progress"
        };

        return JsonSerializer.Serialize(dto, prettyPrint ? _prettyOptions : _compactOptions);
    }

    public T Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Deserialization returned null");
        }
        catch (JsonException ex)
        {
            throw new FormatException($"JSON deserialization failed: {ex.Message}", ex);
        }
    }
}

// DTO classes for serialization
public class LocationDataDto
{
    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("bearing")]
    public double Bearing { get; set; }

    [JsonPropertyName("altitude")]
    public double Altitude { get; set; }

    [JsonPropertyName("accuracy")]
    public double Accuracy { get; set; }

    [JsonPropertyName("satellite_count")]
    public int SatelliteCount { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }
}

public class GpsFrameDto
{
    [JsonPropertyName("frame_id")]
    public string FrameId { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("raw_data_hex")]
    public string RawDataHex { get; set; }

    [JsonPropertyName("received_at")]
    public string ReceivedAt { get; set; }

    [JsonPropertyName("source_address")]
    public string SourceAddress { get; set; }

    [JsonPropertyName("source_port")]
    public int SourcePort { get; set; }

    [JsonPropertyName("is_valid_checksum")]
    public bool IsValidChecksum { get; set; }
}

public class DeviceDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("imei")]
    public string Imei { get; set; }

    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("last_seen")]
    public string LastSeen { get; set; }
}

public class JourneyDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; }

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public string EndTime { get; set; }

    [JsonPropertyName("waypoint_count")]
    public int WaypointCount { get; set; }

    [JsonPropertyName("total_distance_km")]
    public double TotalDistanceKm { get; set; }

    [JsonPropertyName("average_speed")]
    public double AverageSpeed { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }
}
