// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Formatting;

using System.Text;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// CSV export formatter for location data and journeys.
/// Suitable for Excel/spreadsheet import and data analysis.
/// </summary>
public interface ICsvFormatter
{
    string FormatLocationHistory(IEnumerable<LocationData> locations);
    string FormatJourney(Journey journey);
    string FormatDevices(IEnumerable<Device> devices);
}

public class CsvFormatter : ICsvFormatter
{
    private const string LocationHeader = "DeviceId,Timestamp,Latitude,Longitude,Speed(km/h),Bearing(°),Altitude(m),Accuracy(m),SatelliteCount,Protocol";
    private const string JourneyHeader = "WaypointIndex,Timestamp,Latitude,Longitude,Speed(km/h),Bearing(°),Altitude(m),DistanceFromPrevious(km)";
    private const string DeviceHeader = "Id,Imei,DeviceName,Protocol,IsActive,Status,LastSeen";

    public string FormatLocationHistory(IEnumerable<LocationData> locations)
    {
        if (locations == null || !locations.Any())
            return LocationHeader;

        var sb = new StringBuilder();
        sb.AppendLine(LocationHeader);

        foreach (var location in locations)
        {
            sb.AppendLine(FormatLocationRow(location));
        }

        return sb.ToString();
    }

    public string FormatJourney(Journey journey)
    {
        if (journey == null || !journey.Waypoints.Any())
            return JourneyHeader;

        var sb = new StringBuilder();
        sb.AppendLine(JourneyHeader);

        double previousLat = 0, previousLon = 0;
        double cumulativeDistance = 0;

        for (int i = 0; i < journey.Waypoints.Count; i++)
        {
            var waypoint = journey.Waypoints[i];
            double distanceFromPrevious = 0;

            if (i > 0)
            {
                distanceFromPrevious = Utilities.GpsUtilities.CalculateDistanceKm(
                    previousLat, previousLon, waypoint.Latitude, waypoint.Longitude);
                cumulativeDistance += distanceFromPrevious;
            }

            sb.AppendLine(FormatJourneyRow(i + 1, waypoint, distanceFromPrevious));
            previousLat = waypoint.Latitude;
            previousLon = waypoint.Longitude;
        }

        return sb.ToString();
    }

    public string FormatDevices(IEnumerable<Device> devices)
    {
        if (devices == null || !devices.Any())
            return DeviceHeader;

        var sb = new StringBuilder();
        sb.AppendLine(DeviceHeader);

        foreach (var device in devices)
        {
            sb.AppendLine(FormatDeviceRow(device));
        }

        return sb.ToString();
    }

    private string FormatLocationRow(LocationData location)
    {
        return EscapeCsvField(new[]
        {
            location.DeviceId,
            location.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            location.Latitude.ToString("F8"),
            location.Longitude.ToString("F8"),
            location.Speed.ToString("F2"),
            location.Bearing.ToString("F2"),
            location.Altitude.ToString("F2"),
            location.Accuracy.ToString("F2"),
            location.SatelliteCount.ToString(),
            location.Protocol.ToString()
        });
    }

    private string FormatJourneyRow(int index, LocationData waypoint, double distanceFromPrevious)
    {
        return EscapeCsvField(new[]
        {
            index.ToString(),
            waypoint.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            waypoint.Latitude.ToString("F8"),
            waypoint.Longitude.ToString("F8"),
            waypoint.Speed.ToString("F2"),
            waypoint.Bearing.ToString("F2"),
            waypoint.Altitude.ToString("F2"),
            distanceFromPrevious.ToString("F4")
        });
    }

    private string FormatDeviceRow(Device device)
    {
        return EscapeCsvField(new[]
        {
            device.Id,
            device.Imei,
            device.DeviceName,
            device.Protocol.ToString(),
            device.IsActive.ToString(),
            device.Status.ToString(),
            device.LastSeen.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    private string EscapeCsvField(string[] fields)
    {
        return string.Join(",", fields.Select(field =>
        {
            if (field == null)
                return string.Empty;

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }));
    }
}
