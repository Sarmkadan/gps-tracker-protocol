#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents parsed GPS location data from a tracking device.
/// </summary>
public class LocationData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public double Speed { get; set; }
    public double Bearing { get; set; }
    public DateTime Timestamp { get; set; }
    public double Accuracy { get; set; }
    public int SatelliteCount { get; set; }
    public ProtocolType Protocol { get; set; }
    public Dictionary<string, object> ExtendedData { get; set; } = [];

    /// <summary>
    /// Validates location data bounds and consistency.
    /// </summary>
    public bool IsValid()
    {
        return Latitude >= -90 && Latitude <= 90 &&
               Longitude >= -180 && Longitude <= 180 &&
               Speed >= 0 &&
               Bearing >= 0 && Bearing <= 360 &&
               SatelliteCount >= 0 &&
               !string.IsNullOrWhiteSpace(DeviceId);
    }

    /// <summary>
    /// Calculates distance to another location using Haversine formula.
    /// </summary>
    public double DistanceTo(LocationData other)
    {
        const double R = 6371; // Earth radius in kilometers
        var dLat = ToRad(other.Latitude - Latitude);
        var dLon = ToRad(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(Latitude)) * Math.Cos(ToRad(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Calculates bearing between two locations in degrees.
    /// </summary>
    public double BearingTo(LocationData other)
    {
        var dLon = ToRad(other.Longitude - Longitude);
        var y = Math.Sin(dLon) * Math.Cos(ToRad(other.Latitude));
        var x = Math.Cos(ToRad(Latitude)) * Math.Sin(ToRad(other.Latitude)) -
                Math.Sin(ToRad(Latitude)) * Math.Cos(ToRad(other.Latitude)) * Math.Cos(dLon);
        var bearing = Math.Atan2(y, x);
        return (ToDeg(bearing) + 360) % 360;
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180;
    private static double ToDeg(double radians) => radians * 180 / Math.PI;

    public override string ToString() =>
        $"Location({Latitude:F6}, {Longitude:F6}) - Speed: {Speed:F2}km/h - {Timestamp:O}";
}
