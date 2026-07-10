#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;

namespace GpsTrackerProtocol.Events;

/// <summary>
/// Provides extension methods for <see cref="GeofenceEnteredEvent"/> to enhance functionality
/// with common operations and conversions.
/// </summary>
public static class GeofenceEnteredEventExtensions
{
    /// <summary>
    /// Creates a webhook payload from the geofence entered event.
    /// </summary>
    /// <param name="event">The geofence entered event.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/>.DeviceId is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/>.GeofenceId is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="event"/>.Timestamp is <see cref="DateTime.MinValue"/>.</exception>
    /// <returns>A webhook payload ready for HTTP delivery.</returns>
    public static GeofenceWebhookPayload ToWebhookPayload(this GeofenceEnteredEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(@event.DeviceId);
        ArgumentNullException.ThrowIfNull(@event.GeofenceId);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(@event.Timestamp, DateTime.MinValue);

        return new GeofenceWebhookPayload
        {
            EventType = "geofence_entered",
            DeviceId = @event.DeviceId,
            GeofenceId = @event.GeofenceId,
            Latitude = @event.Latitude,
            Longitude = @event.Longitude,
            Speed = @event.Speed,
            DwellSeconds = 0,
            Timestamp = @event.Timestamp.ToString("o", CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Calculates the distance in meters from the entry point to a specified coordinate.
    /// Uses Haversine formula for accurate great-circle distance calculation.
    /// </summary>
    /// <param name="event">The geofence entered event.</param>
    /// <param name="latitude">Target latitude coordinate.</param>
    /// <param name="longitude">Target longitude coordinate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is <see langword="null"/>.</exception>
    /// <returns>Distance in meters between entry point and target coordinate.</returns>
    public static double DistanceTo(this GeofenceEnteredEvent @event, double latitude, double longitude)
    {
        ArgumentNullException.ThrowIfNull(@event);

        const double EarthRadiusMeters = 6_371_000;

        var lat1 = @event.Latitude * Math.PI / 180.0;
        var lon1 = @event.Longitude * Math.PI / 180.0;
        var lat2 = latitude * Math.PI / 180.0;
        var lon2 = longitude * Math.PI / 180.0;

        var dLat = lat2 - lat1;
        var dLon = lon2 - lon1;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// Determines if the entry occurred within a specified rectangular bounding box.
    /// </summary>
    /// <param name="event">The geofence entered event.</param>
    /// <param name="minLatitude">Minimum latitude of bounding box.</param>
    /// <param name="maxLatitude">Maximum latitude of bounding box.</param>
    /// <param name="minLongitude">Minimum longitude of bounding box.</param>
    /// <param name="maxLongitude">Maximum longitude of bounding box.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is <see langword="null"/>.</exception>
    /// <returns>True if entry point is within the bounding box; otherwise false.</returns>
    public static bool IsWithinBoundingBox(
        this GeofenceEnteredEvent @event,
        double minLatitude,
        double maxLatitude,
        double minLongitude,
        double maxLongitude)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return @event.Latitude >= minLatitude &&
               @event.Latitude <= maxLatitude &&
               @event.Longitude >= minLongitude &&
               @event.Longitude <= maxLongitude;
    }

    /// <summary>
    /// Creates a human-readable summary of the geofence entry event.
    /// </summary>
    /// <param name="event">The geofence entered event.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is <see langword="null"/>.</exception>
    /// <returns>A formatted string containing key event details.</returns>
    public static string ToSummaryString(this GeofenceEnteredEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return $"Geofence Entered: Device {@event.DeviceId} entered zone {@event.GeofenceId} " +
               $"at {@event.Latitude:F6}, {@event.Longitude:F6} " +
               $"at {@event.Timestamp:yyyy-MM-dd HH:mm:ss} UTC " +
               $"(Speed: {@event.Speed:F1} km/h)";
    }
}
