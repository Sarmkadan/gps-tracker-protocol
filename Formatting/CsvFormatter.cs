#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Formatting;

using System.Globalization;
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
    string FormatDailyDistanceReport(IEnumerable<DailyDistanceRecord> records);
}

public class CsvFormatter : ICsvFormatter
{
    private const string LocationHeader = "DeviceId,Timestamp,Latitude,Longitude,Speed(km/h),Bearing(°),Altitude(m),Accuracy(m),SatelliteCount,Protocol";
    private const string JourneyHeader = "WaypointIndex,Timestamp,Latitude,Longitude,Speed(km/h),Bearing(°),Altitude(m),DistanceFromPrevious(km)";
    private const string DeviceHeader = "Id,Imei,DeviceName,Protocol,IsActive,Status,LastSeen";
    private const string DailyDistanceHeader = "DeviceId,Date,DistanceKm";

    public string FormatLocationHistory(IEnumerable<LocationData> locations)
    {
        if (locations is null || !locations.Any())
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
        if (journey is null || !journey.Waypoints.Any())
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
        if (devices is null || !devices.Any())
            return DeviceHeader;

        var sb = new StringBuilder();
        sb.AppendLine(DeviceHeader);

        foreach (var device in devices)
        {
            sb.AppendLine(FormatDeviceRow(device));
        }

        return sb.ToString();
    }

    public string FormatDailyDistanceReport(IEnumerable<DailyDistanceRecord> records)
    {
        if (records is null || !records.Any())
            return DailyDistanceHeader;

        var sb = new StringBuilder();
        sb.AppendLine(DailyDistanceHeader);

        foreach (var rec in records)
        {
            sb.AppendLine($"{EscapeCsvField(rec.DeviceId)},{rec.Day:yyyy-MM-dd},{rec.DistanceKm.ToString("F2", CultureInfo.InvariantCulture)}");
        }

        return sb.ToString();
    }

    private string FormatLocationRow(LocationData location)
    {
        return EscapeCsvField(new[]
        {
            location.DeviceId,
            location.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            location.Latitude.ToString("F8", CultureInfo.InvariantCulture),
            location.Longitude.ToString("F8", CultureInfo.InvariantCulture),
            location.Speed.ToString("F2", CultureInfo.InvariantCulture),
            location.Bearing.ToString("F2", CultureInfo.InvariantCulture),
            location.Altitude.ToString("F2", CultureInfo.InvariantCulture),
            location.Accuracy.ToString("F2", CultureInfo.InvariantCulture),
            location.SatelliteCount.ToString(CultureInfo.InvariantCulture),
            location.Protocol.ToString()
        });
    }

    private string FormatJourneyRow(int index, LocationData waypoint, double distanceFromPrevious)
    {
        return EscapeCsvField(new[]
        {
            index.ToString(CultureInfo.InvariantCulture),
            waypoint.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            waypoint.Latitude.ToString("F8", CultureInfo.InvariantCulture),
            waypoint.Longitude.ToString("F8", CultureInfo.InvariantCulture),
            waypoint.Speed.ToString("F2", CultureInfo.InvariantCulture),
            waypoint.Bearing.ToString("F2", CultureInfo.InvariantCulture),
            waypoint.Altitude.ToString("F2", CultureInfo.InvariantCulture),
            distanceFromPrevious.ToString("F4", CultureInfo.InvariantCulture)
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
            device.LastSeen.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        });
    }

    private string EscapeCsvField(string[] fields)
    {
        return string.Join(",", fields.Select(field =>
        {
            if (field is null)
                return string.Empty;

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }));
    }

    private string EscapeCsvField(string field)
    {
        if (field is null)
            return string.Empty;

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}
