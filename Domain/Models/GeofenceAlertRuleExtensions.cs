#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="GeofenceAlertRule"/> to simplify common operations
/// such as rule validation, status checks, and metadata management.
/// </summary>
public static class GeofenceAlertRuleExtensions
{
    /// <summary>
    /// Determines whether this rule is currently eligible to fire an alert based on its cooldown period.
    /// </summary>
    /// <param name="rule">The geofence alert rule to check.</param>
    /// <param name="lastFiredAt">The timestamp of the last alert firing for this rule.</param>
    /// <returns>
    /// True if the rule can fire (either no previous alert or cooldown has elapsed); otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    public static bool CanFireAlert(this GeofenceAlertRule rule, DateTime? lastFiredAt)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (!rule.IsEnabled)
        {
            return false;
        }

        if (lastFiredAt is null)
        {
            return true;
        }

        var timeSinceLastAlert = DateTime.UtcNow - lastFiredAt.Value;
        return timeSinceLastAlert >= rule.Cooldown;
    }

    /// <summary>
    /// Determines whether this rule should be considered "active" based on its enabled status and alert type.
    /// </summary>
    /// <param name="rule">The geofence alert rule to check.</param>
    /// <returns>True if the rule is active and should be monitored; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    public static bool IsActiveRule(this GeofenceAlertRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.IsEnabled && rule.AlertType != GeofenceAlertType.DwellTime;
    }

    /// <summary>
    /// Gets a display-friendly summary of this rule for UI purposes.
    /// </summary>
    /// <param name="rule">The geofence alert rule.</param>
    /// <returns>A formatted string representing the rule.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    public static string GetDisplaySummary(this GeofenceAlertRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        var status = rule.IsEnabled ? "Active" : "Disabled";
        var alertType = rule.AlertType.ToString();
        var cooldownMinutes = rule.Cooldown.TotalMinutes;

        return rule.Id is null
            ? $"Rule: {status} | {alertType} | Device: {rule.DeviceId} | Fence: {rule.GeofenceId} | Cooldown: {cooldownMinutes}m"
            : $"Rule {rule.Id[..8]}: {status} | {alertType} | Device: {rule.DeviceId} | Fence: {rule.GeofenceId} | Cooldown: {cooldownMinutes}m";
    }

    /// <summary>
    /// Determines whether this rule matches the specified device and geofence combination.
    /// </summary>
    /// <param name="rule">The geofence alert rule to check.</param>
    /// <param name="deviceId">The device identifier to match.</param>
    /// <param name="geofenceId">The geofence identifier to match.</param>
    /// <returns>True if the rule matches both device and geofence; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    public static bool MatchesDeviceAndGeofence(this GeofenceAlertRule rule, string deviceId, string geofenceId)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(geofenceId))
        {
            return false;
        }

        return string.Equals(rule.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(rule.GeofenceId, geofenceId, StringComparison.OrdinalIgnoreCase);
    }
}