#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using FluentAssertions;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;
using GpsTrackerProtocol.Services;

namespace GpsTrackerProtocol.Tests;

public static class GeofenceAlertingServiceTestsExtensions
{
    /// <summary>
    /// Creates a test alert rule using the service under test and verifies it was added successfully.
    /// </summary>
    /// <returns>The created GeofenceAlertRule for further assertions.</returns>
    public static GeofenceAlertRule CreateAlertRuleAndVerify(
        this GeofenceAlertingServiceTests tests,
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType = GeofenceAlertType.Enter,
        TimeSpan? cooldown = null,
        string? description = null)
    {
        var sut = tests.GetSut();
        var rule = sut.CreateAlertRule(deviceId, geofenceId, alertType, cooldown, description ?? "");

        rule.Should().NotBeNull();
        rule.DeviceId.Should().Be(deviceId);
        rule.GeofenceId.Should().Be(geofenceId);
        rule.AlertType.Should().Be(alertType);
        rule.IsEnabled.Should().BeTrue();

        var rules = sut.GetRulesForDevice(deviceId);
        rules.Should().HaveCount(1);

        if (description != null)
        {
            rule.Description.Should().Be(description);
        }

        return rule;
    }

    /// <summary>
    /// Processes a geofence entered event and verifies an alert is fired when a matching rule exists.
    /// </summary>
    /// <returns>List of active alerts for further assertions.</returns>
    public static IReadOnlyList<GeofenceAlert> ProcessGeofenceEnteredAndGetAlerts(
        this GeofenceAlertingServiceTests tests,
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType = GeofenceAlertType.Enter,
        TimeSpan? cooldown = null,
        double latitude = 51.5,
        double longitude = -0.1,
        double speed = 30)
    {
        var sut = tests.GetSut();
        sut.CreateAlertRule(deviceId, geofenceId, alertType, cooldown ?? TimeSpan.Zero);

        var @event = new GeofenceEnteredEvent
        {
            DeviceId = deviceId,
            GeofenceId = geofenceId,
            Latitude = latitude,
            Longitude = longitude,
            Speed = speed
        };

        sut.ProcessGeofenceEntered(@event);

        var alerts = sut.GetActiveAlerts(deviceId);
        alerts.Should().HaveCount(1);
        alerts[0].AlertType.Should().Be(alertType);
        alerts[0].Status.Should().Be(GeofenceAlertStatus.Active);

        return alerts;
    }

    /// <summary>
    /// Processes a geofence exited event and verifies an alert is fired when a matching rule exists.
    /// </summary>
    /// <returns>List of active alerts for further assertions.</returns>
    public static IReadOnlyList<GeofenceAlert> ProcessGeofenceExitedAndGetAlerts(
        this GeofenceAlertingServiceTests tests,
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType = GeofenceAlertType.Exit,
        TimeSpan? cooldown = null)
    {
        var sut = tests.GetSut();
        sut.CreateAlertRule(deviceId, geofenceId, alertType, cooldown ?? TimeSpan.Zero);

        var @event = new GeofenceExitedEvent
        {
            DeviceId = deviceId,
            GeofenceId = geofenceId
        };

        sut.ProcessGeofenceExited(@event);

        var alerts = sut.GetActiveAlerts(deviceId);
        alerts.Should().HaveCount(1);
        alerts[0].AlertType.Should().Be(alertType);
        alerts[0].Status.Should().Be(GeofenceAlertStatus.Active);

        return alerts;
    }

    /// <summary>
    /// Gets the GeofenceAlertingService instance under test for direct access in extensions.
    /// </summary>
    public static GeofenceAlertingService GetSut(this GeofenceAlertingServiceTests tests)
    {
        var field = typeof(GeofenceAlertingServiceTests)
            .GetField("_sut", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (GeofenceAlertingService)field!.GetValue(tests)!;
    }
}