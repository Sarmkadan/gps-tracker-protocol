#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Configuration;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Events;
using GpsTrackerProtocol.Services;

namespace GpsTrackerProtocol.Tests;

/// <summary>
/// Tests for low-battery alerting in <see cref="DeviceDiagnosticsService"/>.
/// </summary>
public class DeviceDiagnosticsServiceLowBatteryTests
{
    private const string DeviceId = "lowbat-device-1";

    /// <summary>
    /// Builds a service backed by a single device whose battery level the test can change,
    /// along with the publisher mock that receives low-battery alerts.
    /// </summary>
    private static (DeviceDiagnosticsService Sut, Device Device, IEventPublisher Publisher) BuildSut(
        int battery = 80,
        LowBatteryAlertOptions? options = null)
    {
        var device = new Device
        {
            Id           = DeviceId,
            Imei         = "123456789012345",
            DeviceName   = "Tracker Low",
            Protocol     = ProtocolType.GT06,
            Status       = DeviceStatus.Online,
            LastSeen     = DateTime.UtcNow,
            BatteryLevel = battery
        };

        var deviceRepo = Substitute.For<IDeviceRepository>();
        deviceRepo.GetByIdAsync(DeviceId).Returns(device);

        var locationRepo = Substitute.For<ILocationDataRepository>();
        locationRepo.GetByDeviceIdAsync(DeviceId).Returns(Array.Empty<LocationData>());

        var journeyRepo = Substitute.For<IJourneyRepository>();
        journeyRepo.GetByDeviceIdAsync(DeviceId).Returns(Array.Empty<Journey>());

        var uow = Substitute.For<IUnitOfWork>();
        uow.Devices.Returns(deviceRepo);
        uow.LocationData.Returns(locationRepo);
        uow.Journeys.Returns(journeyRepo);

        var publisher = Substitute.For<IEventPublisher>();
        var sut = new DeviceDiagnosticsService(
            uow,
            Substitute.For<ILogger<DeviceDiagnosticsService>>(),
            publisher,
            options);

        return (sut, device, publisher);
    }

    /// <summary>
    /// Tests that a single low-battery alert is published when the battery drops below the threshold.
    /// </summary>
    [Fact]
    public async Task GetDiagnosticsAsync_ShouldPublishLowBatteryAlert_WhenBatteryBelowThreshold()
    {
        var (sut, _, publisher) = BuildSut(battery: 15);

        await sut.GetDiagnosticsAsync(DeviceId);

        await publisher.Received(1).PublishAsync(Arg.Is<LowBatteryAlertEvent>(e =>
            e.DeviceId == DeviceId &&
            e.BatteryLevel == 15 &&
            e.ThresholdPercent == 20));
    }

    /// <summary>
    /// Tests that repeated polls while the battery stays low do not publish duplicate alerts.
    /// </summary>
    [Fact]
    public async Task GetDiagnosticsAsync_ShouldNotRepeatAlert_WhileBatteryStaysLow()
    {
        var (sut, device, publisher) = BuildSut(battery: 15);

        await sut.GetDiagnosticsAsync(DeviceId);
        device.BatteryLevel = 10;
        await sut.GetDiagnosticsAsync(DeviceId);
        await sut.GetDiagnosticsAsync(DeviceId);

        await publisher.Received(1).PublishAsync(Arg.Any<LowBatteryAlertEvent>());
    }

    /// <summary>
    /// Tests that the alert stays muted while the battery is recovering but below threshold + margin.
    /// </summary>
    [Fact]
    public async Task GetDiagnosticsAsync_ShouldNotRealert_WhenBatteryRecoversWithinMargin()
    {
        var (sut, device, publisher) = BuildSut(battery: 15);

        await sut.GetDiagnosticsAsync(DeviceId);
        device.BatteryLevel = 24; // above threshold 20, below re-arm level 25
        await sut.GetDiagnosticsAsync(DeviceId);
        device.BatteryLevel = 15; // drops again without fully recovering
        await sut.GetDiagnosticsAsync(DeviceId);

        await publisher.Received(1).PublishAsync(Arg.Any<LowBatteryAlertEvent>());
    }

    /// <summary>
    /// Tests that a new alert is published after the battery recovers to threshold + margin and drops again.
    /// </summary>
    [Fact]
    public async Task GetDiagnosticsAsync_ShouldRealert_AfterBatteryRecoversAboveMargin()
    {
        var (sut, device, publisher) = BuildSut(battery: 15);

        await sut.GetDiagnosticsAsync(DeviceId);
        device.BatteryLevel = 25; // exactly threshold + margin re-arms the alert
        await sut.GetDiagnosticsAsync(DeviceId);
        device.BatteryLevel = 12;
        await sut.GetDiagnosticsAsync(DeviceId);

        await publisher.Received(2).PublishAsync(Arg.Any<LowBatteryAlertEvent>());
    }

    /// <summary>
    /// Tests that no alert is published when the battery level is unknown (-1).
    /// </summary>
    [Fact]
    public async Task GetDiagnosticsAsync_ShouldNotAlert_WhenBatteryUnknown()
    {
        var (sut, _, publisher) = BuildSut(battery: -1);

        await sut.GetDiagnosticsAsync(DeviceId);

        await publisher.DidNotReceive().PublishAsync(Arg.Any<LowBatteryAlertEvent>());
    }

    /// <summary>
    /// Tests that the constructor rejects a threshold and margin whose sum exceeds 100.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrow_WhenThresholdPlusMarginExceeds100()
    {
        var act = () => BuildSut(options: new LowBatteryAlertOptions
        {
            ThresholdPercent     = 98,
            RecoveryMarginPercent = 5
        });

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
