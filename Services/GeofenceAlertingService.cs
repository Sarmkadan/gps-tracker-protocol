#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;

/// <summary>
/// Manages geofence alert rules and the alerts they produce.
/// Subscribe to <see cref="GeofenceEnteredEvent"/> and <see cref="GeofenceExitedEvent"/>
/// via the event publisher, then query alert history or acknowledge active alerts.
/// </summary>
public interface IGeofenceAlertingService
{
    /// <summary>
    /// Creates a new alert rule that fires when <paramref name="deviceId"/> crosses the
    /// boundary of <paramref name="geofenceId"/> in the direction indicated by
    /// <paramref name="alertType"/>.
    /// </summary>
    /// <param name="deviceId">Device to monitor.</param>
    /// <param name="geofenceId">Geofence to watch.</param>
    /// <param name="alertType">Which crossing direction triggers the alert.</param>
    /// <param name="cooldown">Minimum gap between consecutive alerts for the same rule.</param>
    /// <param name="description">Human-readable description of the rule.</param>
    /// <returns>The newly created <see cref="GeofenceAlertRule"/>.</returns>
    GeofenceAlertRule CreateAlertRule(
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType,
        TimeSpan? cooldown = null,
        string description = "");

    /// <summary>Deletes an alert rule by ID.  No-op when the rule does not exist.</summary>
    void DeleteAlertRule(string ruleId);

    /// <summary>Returns all rules currently registered for <paramref name="deviceId"/>.</summary>
    IReadOnlyList<GeofenceAlertRule> GetRulesForDevice(string deviceId);

    /// <summary>Returns all active (unacknowledged) alerts for <paramref name="deviceId"/>.</summary>
    IReadOnlyList<GeofenceAlert> GetActiveAlerts(string deviceId);

    /// <summary>
    /// Returns the most recent alert records for <paramref name="deviceId"/>,
    /// ordered newest-first, up to <paramref name="limit"/> entries.
    /// </summary>
    IReadOnlyList<GeofenceAlert> GetAlertHistory(string deviceId, int limit = 50);

    /// <summary>
    /// Marks an alert as acknowledged and records optional operator notes.
    /// Returns <c>false</c> when the alert was not found.
    /// </summary>
    bool AcknowledgeAlert(string alertId, string notes = "");

    /// <summary>Evaluates all rules against a geofence-entered event and fires alerts where applicable.</summary>
    void ProcessGeofenceEntered(GeofenceEnteredEvent @event);

    /// <summary>Evaluates all rules against a geofence-exited event and fires alerts where applicable.</summary>
    void ProcessGeofenceExited(GeofenceExitedEvent @event);
}

/// <summary>
/// In-memory implementation of <see cref="IGeofenceAlertingService"/>.
/// All state is kept in sorted collections protected by a reader-writer lock so that
/// multiple threads can query alert history concurrently while writes remain exclusive.
/// </summary>
public class GeofenceAlertingService : IGeofenceAlertingService
{
    private readonly ILogger<GeofenceAlertingService> _logger;
    private readonly IEventPublisher _eventPublisher;

    private readonly Dictionary<string, GeofenceAlertRule> _rules = new();
    private readonly List<GeofenceAlert> _alerts = [];
    private readonly Dictionary<string, DateTime> _lastFiredAt = new();
    private readonly object _lock = new();

    /// <summary>Initialises the service and subscribes to geofence domain events.</summary>
    public GeofenceAlertingService(
        IEventPublisher eventPublisher,
        ILogger<GeofenceAlertingService> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;

        _eventPublisher.Subscribe<GeofenceEnteredEvent>(e =>
        {
            ProcessGeofenceEntered(e);
            return Task.CompletedTask;
        });

        _eventPublisher.Subscribe<GeofenceExitedEvent>(e =>
        {
            ProcessGeofenceExited(e);
            return Task.CompletedTask;
        });
    }

    /// <inheritdoc/>
    public GeofenceAlertRule CreateAlertRule(
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType,
        TimeSpan? cooldown = null,
        string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(geofenceId);

        var rule = new GeofenceAlertRule
        {
            DeviceId    = deviceId,
            GeofenceId  = geofenceId,
            AlertType   = alertType,
            Cooldown    = cooldown ?? TimeSpan.FromMinutes(5),
            Description = description
        };

        lock (_lock)
            _rules[rule.Id] = rule;

        _logger.LogInformation(
            "Alert rule created: {RuleId} device={DeviceId} geofence={GeofenceId} type={AlertType}",
            rule.Id, deviceId, geofenceId, alertType);

        return rule;
    }

    /// <inheritdoc/>
    public void DeleteAlertRule(string ruleId)
    {
        lock (_lock)
        {
            if (_rules.Remove(ruleId))
                _logger.LogInformation("Alert rule deleted: {RuleId}", ruleId);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<GeofenceAlertRule> GetRulesForDevice(string deviceId)
    {
        lock (_lock)
            return _rules.Values.Where(r => r.DeviceId == deviceId).ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<GeofenceAlert> GetActiveAlerts(string deviceId)
    {
        lock (_lock)
            return _alerts
                .Where(a => a.DeviceId == deviceId && a.Status == GeofenceAlertStatus.Active)
                .OrderByDescending(a => a.FiredAt)
                .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<GeofenceAlert> GetAlertHistory(string deviceId, int limit = 50)
    {
        if (limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be positive.");

        lock (_lock)
            return _alerts
                .Where(a => a.DeviceId == deviceId)
                .OrderByDescending(a => a.FiredAt)
                .Take(limit)
                .ToList();
    }

    /// <inheritdoc/>
    public bool AcknowledgeAlert(string alertId, string notes = "")
    {
        lock (_lock)
        {
            var alert = _alerts.FirstOrDefault(a => a.Id == alertId);
            if (alert is null)
                return false;

            alert.Status         = GeofenceAlertStatus.Acknowledged;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.Notes          = notes;
        }

        _logger.LogInformation("Alert acknowledged: {AlertId}", alertId);
        return true;
    }

    /// <inheritdoc/>
    public void ProcessGeofenceEntered(GeofenceEnteredEvent @event)
        => EvaluateRules(@event.DeviceId, @event.GeofenceId, GeofenceAlertType.Enter,
            @event.Latitude, @event.Longitude, @event.Speed);

    /// <inheritdoc/>
    public void ProcessGeofenceExited(GeofenceExitedEvent @event)
        => EvaluateRules(@event.DeviceId, @event.GeofenceId, GeofenceAlertType.Exit,
            @event.Latitude, @event.Longitude, speed: 0);

    // ── private helpers ────────────────────────────────────────────────────────

    private void EvaluateRules(
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType,
        double latitude,
        double longitude,
        double speed)
    {
        lock (_lock)
        {
            var matchingRules = _rules.Values
                .Where(r => r.IsEnabled
                         && r.DeviceId   == deviceId
                         && r.GeofenceId == geofenceId
                         && r.AlertType  == alertType)
                .ToList();

            foreach (var rule in matchingRules)
            {
                var cooldownKey = $"{rule.Id}";
                if (_lastFiredAt.TryGetValue(cooldownKey, out var lastFired)
                    && DateTime.UtcNow - lastFired < rule.Cooldown)
                {
                    _logger.LogDebug(
                        "Alert rule {RuleId} suppressed by cooldown for device {DeviceId}",
                        rule.Id, deviceId);

                    _alerts.Add(new GeofenceAlert
                    {
                        RuleId     = rule.Id,
                        DeviceId   = deviceId,
                        GeofenceId = geofenceId,
                        AlertType  = alertType,
                        Latitude   = latitude,
                        Longitude  = longitude,
                        Speed      = speed,
                        Status     = GeofenceAlertStatus.Suppressed
                    });
                    continue;
                }

                _lastFiredAt[cooldownKey] = DateTime.UtcNow;

                var alert = new GeofenceAlert
                {
                    RuleId     = rule.Id,
                    DeviceId   = deviceId,
                    GeofenceId = geofenceId,
                    AlertType  = alertType,
                    Latitude   = latitude,
                    Longitude  = longitude,
                    Speed      = speed,
                    Status     = GeofenceAlertStatus.Active
                };

                _alerts.Add(alert);

                _logger.LogWarning(
                    "Geofence alert fired: rule={RuleId} device={DeviceId} geofence={GeofenceId} type={AlertType}",
                    rule.Id, deviceId, geofenceId, alertType);
            }
        }
    }
}
