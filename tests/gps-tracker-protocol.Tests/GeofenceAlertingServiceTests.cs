#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;
using GpsTrackerProtocol.Services;

namespace GpsTrackerProtocol.Tests;

public class GeofenceAlertingServiceTests
{
    private readonly IEventPublisher _publisher;
    private readonly GeofenceAlertingService _sut;

    public GeofenceAlertingServiceTests()
    {
        _publisher = Substitute.For<IEventPublisher>();
        _publisher
            .Subscribe(Arg.Any<Func<GeofenceEnteredEvent, Task>>())
            .Returns(Substitute.For<IDisposable>());
        _publisher
            .Subscribe(Arg.Any<Func<GeofenceExitedEvent, Task>>())
            .Returns(Substitute.For<IDisposable>());

        _sut = new GeofenceAlertingService(_publisher, Substitute.For<ILogger<GeofenceAlertingService>>());
    }

    [Fact]
    public void CreateAlertRule_ShouldAddRuleForDevice()
    {
        var rule = _sut.CreateAlertRule("device-1", "zone-hq", GeofenceAlertType.Enter);

        rule.Should().NotBeNull();
        rule.DeviceId.Should().Be("device-1");
        rule.GeofenceId.Should().Be("zone-hq");
        rule.AlertType.Should().Be(GeofenceAlertType.Enter);
        rule.IsEnabled.Should().BeTrue();

        var rules = _sut.GetRulesForDevice("device-1");
        rules.Should().HaveCount(1);
    }

    [Fact]
    public void ProcessGeofenceEntered_ShouldFireAlertWhenMatchingRuleExists()
    {
        _sut.CreateAlertRule("device-2", "zone-a", GeofenceAlertType.Enter, cooldown: TimeSpan.Zero);

        _sut.ProcessGeofenceEntered(new GeofenceEnteredEvent
        {
            DeviceId   = "device-2",
            GeofenceId = "zone-a",
            Latitude   = 51.5,
            Longitude  = -0.1,
            Speed      = 30
        });

        var alerts = _sut.GetActiveAlerts("device-2");
        alerts.Should().HaveCount(1);
        alerts[0].AlertType.Should().Be(GeofenceAlertType.Enter);
        alerts[0].Status.Should().Be(GeofenceAlertStatus.Active);
    }

    [Fact]
    public void ProcessGeofenceEntered_ShouldSuppressAlertWithinCooldown()
    {
        _sut.CreateAlertRule("device-3", "zone-b", GeofenceAlertType.Enter, cooldown: TimeSpan.FromHours(1));

        var @event = new GeofenceEnteredEvent
        {
            DeviceId   = "device-3",
            GeofenceId = "zone-b",
            Latitude   = 51.5,
            Longitude  = -0.1
        };

        _sut.ProcessGeofenceEntered(@event);
        _sut.ProcessGeofenceEntered(@event);

        var history = _sut.GetAlertHistory("device-3");
        history.Should().HaveCount(2);
        history.Count(a => a.Status == GeofenceAlertStatus.Active).Should().Be(1);
        history.Count(a => a.Status == GeofenceAlertStatus.Suppressed).Should().Be(1);
    }

    [Fact]
    public void AcknowledgeAlert_ShouldMarkAlertAsAcknowledged()
    {
        _sut.CreateAlertRule("device-4", "zone-c", GeofenceAlertType.Exit, cooldown: TimeSpan.Zero);
        _sut.ProcessGeofenceExited(new GeofenceExitedEvent
        {
            DeviceId   = "device-4",
            GeofenceId = "zone-c"
        });

        var alert  = _sut.GetActiveAlerts("device-4").First();
        var result = _sut.AcknowledgeAlert(alert.Id, "reviewed by operator");

        result.Should().BeTrue();
        _sut.GetActiveAlerts("device-4").Should().BeEmpty();
    }

    [Fact]
    public void DeleteAlertRule_ShouldRemoveRule()
    {
        var rule = _sut.CreateAlertRule("device-5", "zone-d", GeofenceAlertType.Enter);
        _sut.DeleteAlertRule(rule.Id);

        _sut.GetRulesForDevice("device-5").Should().BeEmpty();
    }

    [Fact]
    public void ProcessGeofenceEntered_ShouldNotFireAlert_WhenNoMatchingRule()
    {
        _sut.ProcessGeofenceEntered(new GeofenceEnteredEvent
        {
            DeviceId   = "device-no-rule",
            GeofenceId = "zone-x"
        });

        _sut.GetActiveAlerts("device-no-rule").Should().BeEmpty();
    }
}
