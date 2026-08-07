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
    /// <summary>
    /// Gets or sets the unique identifier of the location record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the identifier of the device that reported this location.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latitude in decimal degrees. Valid range is -90 to 90.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude in decimal degrees. Valid range is -180 to 180.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets the altitude in meters above sea level.
    /// </summary>
    public double Altitude { get; set; }

    /// <summary>
    /// Gets or sets the speed in kilometers per hour.
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// Gets or sets the bearing (course over ground) in degrees. Valid range is 0 to 360.
    /// </summary>
    public double Bearing { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the location was recorded by the device.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the estimated accuracy of the location fix in meters.
    /// </summary>
    public double Accuracy { get; set; }

    /// <summary>
    /// Gets or sets the number of satellites used for the location fix.
    /// </summary>
    public int SatelliteCount { get; set; }

    /// <summary>
    /// Gets or sets the protocol type used by the device that reported this location.
    /// </summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>
    /// Gets or sets additional protocol-specific data associated with this location.
    /// </summary>
    public Dictionary<string, object> ExtendedData { get; set; } = [];

    /// <summary>
    /// Validates location data bounds and consistency.
    /// </summary>
    /// <returns><c>true</c> if the location data is within valid bounds and consistent; otherwise, <c>false</c>.</returns>
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
    /// <param name="other">The other location to calculate the distance to.</param>
    /// <returns>The distance to the other location in kilometers.</returns>
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
    /// <param name="other">The other location to calculate the bearing to.</param>
    /// <returns>The initial bearing to the other location in degrees, normalized to the range 0 to 360.</returns>
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

    /// <inheritdoc />
    public override string ToString() =>
        $"Location({Latitude:F6}, {Longitude:F6}) - Speed: {Speed:F2}km/h - {Timestamp:O}";
}
