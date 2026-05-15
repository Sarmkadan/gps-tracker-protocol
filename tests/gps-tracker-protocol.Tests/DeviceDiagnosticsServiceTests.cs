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
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Services;

namespace GpsTrackerProtocol.Tests;

public class DeviceDiagnosticsServiceTests
{
    private static (DeviceDiagnosticsService Sut, Device Device) BuildSut(
        int battery       = 80,
        int signal        = -65,
        DeviceStatus status = DeviceStatus.Online,
        int locationCount = 5,
        int journeyCount  = 2)
    {
        var device = new Device
        {
            Id              = "diag-device-1",
            Imei            = "123456789012345",
            DeviceName      = "Tracker X",
            Protocol        = ProtocolType.GT06,
            Status          = status,
            LastSeen        = DateTime.UtcNow.AddMinutes(-2),
            BatteryLevel    = battery,
            SignalStrength  = signal,
            ConnectionCount = 42
        };

        var locations = Enumerable.Range(0, locationCount).Select(i => new LocationData
        {
            DeviceId  = device.Id,
            Latitude  = 51.5 + i * 0.001,
            Longitude = -0.1,
            Speed     = 40,
            Bearing   = 0,
            Timestamp = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();

        var journeys = Enumerable.Range(0, journeyCount).Select(i => new Journey
        {
            Id       = $"j-{i}",
            DeviceId = device.Id,
            Status   = 1
        }).ToList();

        var deviceRepo = Substitute.For<IDeviceRepository>();
        deviceRepo.GetByIdAsync(device.Id).Returns(device);

        var locationRepo = Substitute.For<ILocationDataRepository>();
        locationRepo.GetByDeviceIdAsync(device.Id).Returns(locations);
        locationRepo.GetLatestByDeviceIdAsync(device.Id).Returns(locations.First());

        var journeyRepo = Substitute.For<IJourneyRepository>();
        journeyRepo.GetByDeviceIdAsync(device.Id).Returns(journeys);

        var uow = Substitute.For<IUnitOfWork>();
        uow.Devices.Returns(deviceRepo);
        uow.LocationData.Returns(locationRepo);
        uow.Journeys.Returns(journeyRepo);

        var sut = new DeviceDiagnosticsService(uow, Substitute.For<ILogger<DeviceDiagnosticsService>>());
        return (sut, device);
    }

    [Fact]
    public async Task GetDiagnosticsAsync_ShouldReturnCorrectConnectivityInfo()
    {
        var (sut, device) = BuildSut();

        var report = await sut.GetDiagnosticsAsync(device.Id);

        report.Should().NotBeNull();
        report!.IsOnline.Should().BeTrue();
        report.TotalPacketsReceived.Should().Be(42);
        report.SignalQuality.Should().Be("Excellent");
    }

    [Fact]
    public async Task GetDiagnosticsAsync_ShouldReturnNull_WhenDeviceNotFound()
    {
        var deviceRepo = Substitute.For<IDeviceRepository>();
        deviceRepo.GetByIdAsync(Arg.Any<string>()).Returns((Device?)null);

        var uow = Substitute.For<IUnitOfWork>();
        uow.Devices.Returns(deviceRepo);
        uow.LocationData.Returns(Substitute.For<ILocationDataRepository>());
        uow.Journeys.Returns(Substitute.For<IJourneyRepository>());

        var sut    = new DeviceDiagnosticsService(uow, Substitute.For<ILogger<DeviceDiagnosticsService>>());
        var result = await sut.GetDiagnosticsAsync("non-existent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RunSelfTestAsync_ShouldPassForHealthyDevice()
    {
        var (sut, device) = BuildSut(battery: 80, signal: -65, status: DeviceStatus.Online);

        var result = await sut.RunSelfTestAsync(device.Id);

        result.Should().NotBeNull();
        result!.ConnectivityOk.Should().BeTrue();
        result.BatteryOk.Should().BeTrue();
        result.SignalOk.Should().BeTrue();
        result.LocationDataOk.Should().BeTrue();
        result.AllOk.Should().BeTrue();
    }

    [Fact]
    public async Task RunSelfTestAsync_ShouldWarnAboutLowBattery()
    {
        var (sut, device) = BuildSut(battery: 10);

        var result = await sut.RunSelfTestAsync(device.Id);

        result.Should().NotBeNull();
        result!.BatteryOk.Should().BeFalse();
        result.Warnings.Should().Contain(w => w.Contains("Battery low"));
    }

    [Fact]
    public async Task RunSelfTestAsync_ShouldWarnAboutWeakSignal()
    {
        var (sut, device) = BuildSut(signal: -100);

        var result = await sut.RunSelfTestAsync(device.Id);

        result.Should().NotBeNull();
        result!.SignalOk.Should().BeFalse();
        result.Warnings.Should().Contain(w => w.Contains("Weak signal"));
    }

    [Fact]
    public async Task GetDiagnosticsAsync_ShouldIncludeCorrectLocationCount()
    {
        var (sut, device) = BuildSut(locationCount: 7);

        var report = await sut.GetDiagnosticsAsync(device.Id);

        report!.TotalLocationPoints.Should().Be(7);
    }

    [Fact]
    public async Task GetDiagnosticsAsync_ShouldClassifySignalCorrectly()
    {
        var (sut, device) = BuildSut(signal: -95);

        var report = await sut.GetDiagnosticsAsync(device.Id);

        report!.SignalQuality.Should().Be("Poor");
    }
}
