// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Events;

/// <summary>
/// Raised when a tracked device crosses into a geofence zone.
/// </summary>
public class GeofenceEnteredEvent : IDomainEvent
{
    /// <summary>Gets the unique event identifier.</summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets the UTC timestamp when the crossing was detected.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Gets the aggregate root identifier (device ID).</summary>
    public string AggregateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device that entered the zone.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the geofence identifier that was entered.</summary>
    public string GeofenceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the latitude at the moment of entry.</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude at the moment of entry.</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets the device speed in km/h at the moment of entry.</summary>
    public double Speed { get; set; }
}

/// <summary>
/// Raised when a tracked device crosses out of a geofence zone.
/// </summary>
public class GeofenceExitedEvent : IDomainEvent
{
    /// <summary>Gets the unique event identifier.</summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets the UTC timestamp when the crossing was detected.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Gets the aggregate root identifier (device ID).</summary>
    public string AggregateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device that exited the zone.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the geofence identifier that was exited.</summary>
    public string GeofenceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the latitude at the moment of exit.</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude at the moment of exit.</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets how long the device was inside the zone before exiting.</summary>
    public TimeSpan DwellDuration { get; set; }
}

/// <summary>
/// Raised when a device has remained inside a geofence beyond the configured dwell threshold.
/// </summary>
public class GeofenceDwellEvent : IDomainEvent
{
    /// <summary>Gets the unique event identifier.</summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets the UTC timestamp when the dwell threshold was exceeded.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Gets the aggregate root identifier (device ID).</summary>
    public string AggregateId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device dwelling inside the zone.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the geofence where extended dwell was detected.</summary>
    public string GeofenceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the total time the device has been inside the zone.</summary>
    public TimeSpan DwellDuration { get; set; }

    /// <summary>Gets or sets the configured threshold that was exceeded.</summary>
    public TimeSpan DwellThreshold { get; set; }
}

/// <summary>
/// HTTP payload structure delivered to registered webhook endpoints on geofence transitions.
/// Mirrors <c>WebhookPayload</c> but carries geofence-specific fields.
/// </summary>
public class GeofenceWebhookPayload
{
    /// <summary>
    /// Gets or sets the event type discriminator.
    /// One of: <c>geofence_entered</c>, <c>geofence_exited</c>, <c>geofence_dwell</c>.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Gets or sets the device identifier that triggered the event.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the geofence identifier involved in the transition.</summary>
    public string GeofenceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the latitude at the time of the event.</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude at the time of the event.</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets the device speed in km/h at the time of the event.</summary>
    public double Speed { get; set; }

    /// <summary>
    /// Gets or sets the total seconds the device was or has been inside the zone.
    /// Zero for entry events.
    /// </summary>
    public long DwellSeconds { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the event in ISO 8601 format.</summary>
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
}
