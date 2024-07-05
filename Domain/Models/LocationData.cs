#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents parsed GPS location data from a tracking device.
/// Contains coordinates, motion data, and GPS fix quality metrics.
/// Includes Haversine distance calculation and bearing computation methods.
/// </summary>
public class LocationData
{
    /// <summary>Unique record identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>IMEI or unique identifier of the tracker device.</summary>
    public string DeviceId { get; set; } = string.Empty;
    /// <summary>Latitude in decimal degrees (-90 to 90).</summary>
    public double Latitude { get; set; }
    /// <summary>Longitude in decimal degrees (-180 to 180).</summary>
    public double Longitude { get; set; }
    /// <summary>Altitude above sea level in meters.</summary>
    public double Altitude { get; set; }
    /// <summary>Speed in km/h at the time of the GPS fix.</summary>
    public double Speed { get; set; }
    /// <summary>Heading/bearing in degrees (0-360, where 0 = North).</summary>
    public double Bearing { get; set; }
    /// <summary>UTC timestamp of the GPS fix from the device.</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>Horizontal accuracy of the GPS fix in meters.</summary>
    public double Accuracy { get; set; }
    /// <summary>Number of satellites used for the fix. Higher values indicate better accuracy.</summary>
    public int SatelliteCount { get; set; }
    /// <summary>Protocol used to receive this data (GT06, H02, TK103).</summary>
    public ProtocolType Protocol { get; set; }
    /// <summary>Protocol-specific extended data fields (e.g., GSM signal, battery level).</summary>
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
