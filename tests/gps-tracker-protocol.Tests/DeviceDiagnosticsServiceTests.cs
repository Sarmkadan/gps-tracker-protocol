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

/// <summary>
/// Tests for the DeviceDiagnosticsService class.
/// </summary>
public class DeviceDiagnosticsServiceTests
{
    /// <summary>
    /// Builds a DeviceDiagnosticsService instance with a Device instance.
    /// </summary>
    /// <param name="battery">The battery level of the device.</param>
    /// <param name="signal">The signal strength of the device.</param>
    /// <param name="status">The status of the device.</param>
    /// <param name="locationCount">The number of location points for the device.</param>
    /// <param name="journeyCount">The number of journeys for the device.</param>
    /// <returns>A tuple containing the DeviceDiagnosticsService instance and the Device instance.</returns>
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

    /// <summary>
    /// Tests that GetDiagnosticsAsync returns the correct connectivity information.
    /// </summary>
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

    /// <summary>
    /// Tests that GetDiagnosticsAsync returns null when the device is not found.
    /// </summary>
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

    /// <summary>
    /// Tests that RunSelfTestAsync passes for a healthy device.
    /// </summary>
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

    /// <summary>
    /// Tests that RunSelfTestAsync warns about a low battery.
    /// </summary>
    [Fact]
    public async Task RunSelfTestAsync_ShouldWarnAboutLowBattery()
    {
        var (sut, device) = BuildSut(battery: 10);

        var result = await sut.RunSelfTestAsync(device.Id);

        result.Should().NotBeNull();
        result!.BatteryOk.Should().BeFalse();
        result.Warnings.Should().Contain(w => w.Contains("Battery low"));
    }

    /// <summary>
    /// Tests that RunSelfTestAsync warns about a weak signal.
    /// </summary>
    [Fact]
    public async Task RunSelfTestAsync_ShouldWarnAboutWeakSignal()
    {
        var (sut, device) = BuildSut(signal: -100);

        var result = await sut.RunSelfTestAsync(device.Id);

        result.Should().NotBeNull();
        result!.SignalOk.Should().BeFalse();
        result.Warnings.Should().Contain(w => w.Contains("Weak signal"));
    }

    /// <summary>
    /// Tests that GetDiagnosticsAsync includes the correct location count.
    /// </summary>
    [Fact]
    public async Task GetDiagnosticsAsync_ShouldIncludeCorrectLocationCount()
    {
        var (sut, device) = BuildSut(locationCount: 7);

        var report = await sut.GetDiagnosticsAsync(device.Id);

        report!.TotalLocationPoints.Should().Be(7);
    }

    /// <summary>
    /// Tests that GetDiagnosticsAsync classifies the signal correctly.
    /// </summary>
    [Fact]
    public async Task GetDiagnosticsAsync_ShouldClassifySignalCorrectly()
    {
        var (sut, device) = BuildSut(signal: -95);

        var report = await sut.GetDiagnosticsAsync(device.Id);

        report!.SignalQuality.Should().Be("Poor");
    }
}
