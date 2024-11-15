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
    /// <param name="tests">The test instance.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="geofenceId">The geofence identifier.</param>
    /// <param name="alertType">The alert type. Defaults to <see cref="GeofenceAlertType.Enter"/>.</param>
    /// <param name="cooldown">Optional cooldown period. Defaults to <see cref="TimeSpan.Zero"/>.</param>
    /// <param name="description">Optional description. Defaults to empty string.</param>
    /// <returns>The created <see cref="GeofenceAlertRule"/> for further assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/>, <paramref name="deviceId"/>, or <paramref name="geofenceId"/> is null.</exception>
    public static GeofenceAlertRule CreateAlertRuleAndVerify(
        this GeofenceAlertingServiceTests tests,
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType = GeofenceAlertType.Enter,
        TimeSpan? cooldown = null,
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(deviceId);
        ArgumentNullException.ThrowIfNull(geofenceId);

        var sut = tests.GetSut();
        var rule = sut.CreateAlertRule(deviceId, geofenceId, alertType, cooldown ?? TimeSpan.Zero, description ?? string.Empty);

        rule.Should().NotBeNull();
        rule.DeviceId.Should().Be(deviceId);
        rule.GeofenceId.Should().Be(geofenceId);
        rule.AlertType.Should().Be(alertType);
        rule.IsEnabled.Should().BeTrue();

        var rules = sut.GetRulesForDevice(deviceId);
        rules.Should().HaveCount(1);

        if (description is not null)
        {
            rule.Description.Should().Be(description);
        }

        return rule;
    }

    /// <summary>
    /// Processes a geofence entered event and verifies an alert is fired when a matching rule exists.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="geofenceId">The geofence identifier.</param>
    /// <param name="alertType">The alert type. Defaults to <see cref="GeofenceAlertType.Enter"/>.</param>
    /// <param name="cooldown">Optional cooldown period. Defaults to <see cref="TimeSpan.Zero"/>.</param>
    /// <param name="latitude">The latitude coordinate. Defaults to 51.5.</param>
    /// <param name="longitude">The longitude coordinate. Defaults to -0.1.</param>
    /// <param name="speed">The device speed. Defaults to 30.</param>
    /// <returns>List of active alerts for further assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/>, <paramref name="deviceId"/>, or <paramref name="geofenceId"/> is null.</exception>
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
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(deviceId);
        ArgumentNullException.ThrowIfNull(geofenceId);

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
    /// <param name="tests">The test instance.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="geofenceId">The geofence identifier.</param>
    /// <param name="alertType">The alert type. Defaults to <see cref="GeofenceAlertType.Exit"/>.</param>
    /// <param name="cooldown">Optional cooldown period. Defaults to <see cref="TimeSpan.Zero"/>.</param>
    /// <returns>List of active alerts for further assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/>, <paramref name="deviceId"/>, or <paramref name="geofenceId"/> is null.</exception>
    public static IReadOnlyList<GeofenceAlert> ProcessGeofenceExitedAndGetAlerts(
        this GeofenceAlertingServiceTests tests,
        string deviceId,
        string geofenceId,
        GeofenceAlertType alertType = GeofenceAlertType.Exit,
        TimeSpan? cooldown = null)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(deviceId);
        ArgumentNullException.ThrowIfNull(geofenceId);

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
    /// <param name="tests">The test instance containing the service under test.</param>
    /// <returns>The <see cref="GeofenceAlertingService"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the _sut field is not found or contains null.</exception>
    public static GeofenceAlertingService GetSut(this GeofenceAlertingServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        var field = typeof(GeofenceAlertingServiceTests)
            .GetField("_sut", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field?.GetValue(tests) as GeofenceAlertingService
               ?? throw new InvalidOperationException("The _sut field was not found or contains null.");
    }
}