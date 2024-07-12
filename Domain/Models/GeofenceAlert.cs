#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Defines a rule that triggers an alert when a device crosses a geofence boundary.
/// Rules are evaluated each time a location update arrives for the device.
/// </summary>
public class GeofenceAlertRule
{
    /// <summary>Gets or sets the unique rule identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the device this rule monitors.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the geofence this rule is bound to.</summary>
    public string GeofenceId { get; set; } = string.Empty;

    /// <summary>Gets or sets which boundary crossing fires the alert.</summary>
    public GeofenceAlertType AlertType { get; set; }

    /// <summary>
    /// Gets or sets the minimum interval between consecutive alerts for the same
    /// device/geofence/type combination.  Defaults to five minutes.
    /// </summary>
    public TimeSpan Cooldown { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Gets or sets whether the rule is currently active.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets a human-readable description of the rule.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets when the rule was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a single geofence alert that was fired because a device satisfied a
/// <see cref="GeofenceAlertRule"/>.  Alerts transition from <see cref="GeofenceAlertStatus.Active"/>
/// to <see cref="GeofenceAlertStatus.Acknowledged"/> once an operator reviews them.
/// </summary>
public class GeofenceAlert
{
    /// <summary>Gets or sets the unique alert identifier.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Gets or sets the rule that produced this alert.</summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device that triggered the alert.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the geofence involved in the transition.</summary>
    public string GeofenceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of boundary event that fired the alert.</summary>
    public GeofenceAlertType AlertType { get; set; }

    /// <summary>Gets or sets the latitude at the time of the event.</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude at the time of the event.</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets the device speed at the time of the event.</summary>
    public double Speed { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the alert was raised.</summary>
    public DateTime FiredAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp when the alert was acknowledged, if ever.</summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>Gets or sets the current status of this alert.</summary>
    public GeofenceAlertStatus Status { get; set; } = GeofenceAlertStatus.Active;

    /// <summary>Gets or sets optional notes added during acknowledgement.</summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>Status lifecycle for a <see cref="GeofenceAlert"/>.</summary>
public enum GeofenceAlertStatus
{
    /// <summary>Alert has been raised and is awaiting operator review.</summary>
    Active = 0,

    /// <summary>An operator has reviewed and dismissed the alert.</summary>
    Acknowledged = 1,

    /// <summary>The alert was suppressed by the cooldown period.</summary>
    Suppressed = 2
}
