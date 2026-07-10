#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Data;

namespace GpsTrackerProtocol.Tests;

public static class DeviceDiagnosticsServiceTestsExtensions
{
    /// <summary>
    /// Creates a simplified test case for GetDiagnosticsAsync that verifies basic connectivity
    /// without requiring full device setup. Useful for quick validation of connectivity scenarios.
    /// </summary>
    /// <param name="deviceId">The device identifier</param>
    /// <param name="isOnline">Whether device should be reported as online</param>
    /// <param name="expectedSignalQuality">Expected signal quality classification</param>
    /// <param name="expectedPackets">Expected total packets received count</param>
    /// <returns>Tuple containing the test assertion and expected values</returns>
    public static async Task<(DeviceDiagnosticsService Sut, DeviceDiagnosticsReport Report)>
        CreateDiagnosticsTestAsync(
            this DeviceDiagnosticsServiceTests _,
            string deviceId,
            bool isOnline,
            string expectedSignalQuality,
            int expectedPackets)
    {
        // Arrange
        var deviceRepo = Substitute.For<IDeviceRepository>();
        deviceRepo.GetByIdAsync(deviceId).Returns(new Device
        {
            Id = deviceId,
            Imei = "test-imei",
            DeviceName = "Test Device",
            Status = isOnline ? DeviceStatus.Online : DeviceStatus.Offline,
            BatteryLevel = 75,
            SignalStrength = isOnline ? -60 : -120,
            ConnectionCount = expectedPackets,
            LastSeen = DateTime.UtcNow.AddMinutes(-1)
        });

        var uow = Substitute.For<IUnitOfWork>();
        uow.Devices.Returns(deviceRepo);
        uow.LocationData.Returns(Substitute.For<ILocationDataRepository>());
        uow.Journeys.Returns(Substitute.For<IJourneyRepository>());

        var sut = new DeviceDiagnosticsService(uow, Substitute.For<ILogger<DeviceDiagnosticsService>>());

        // Act
        var report = await sut.GetDiagnosticsAsync(deviceId);

        // Assert
        report.Should().NotBeNull();
        report!.IsOnline.Should().Be(isOnline);
        report.SignalQuality.Should().Be(expectedSignalQuality);
        report.TotalPacketsReceived.Should().Be(expectedPackets);

        return (sut, report);
    }

    /// <summary>
    /// Creates a test case for RunSelfTestAsync that validates multiple scenarios
    /// in a single call. Useful for testing combinations of device states.
    /// </summary>
    /// <param name="deviceId">The device identifier</param>
    /// <param name="batteryLevel">Battery level percentage</param>
    /// <param name="signalStrength">Signal strength in dBm</param>
    /// <param name="expectedConnectivityOk">Expected connectivity result</param>
    /// <param name="expectedBatteryOk">Expected battery result</param>
    /// <param name="expectedSignalOk">Expected signal result</param>
    /// <returns>Tuple containing the test assertion and expected values</returns>
    public static async Task<(DeviceDiagnosticsService Sut, DeviceSelfTestResult Result)>
        CreateSelfTestAsync(
            this DeviceDiagnosticsServiceTests _,
            string deviceId,
            int batteryLevel,
            int signalStrength,
            bool expectedConnectivityOk,
            bool expectedBatteryOk,
            bool expectedSignalOk)
    {
        // Arrange
        var deviceRepo = Substitute.For<IDeviceRepository>();
        deviceRepo.GetByIdAsync(deviceId).Returns(new Device
        {
            Id = deviceId,
            Imei = "test-imei",
            DeviceName = "Test Device",
            Status = DeviceStatus.Online,
            BatteryLevel = batteryLevel,
            SignalStrength = signalStrength,
            ConnectionCount = 100,
            LastSeen = DateTime.UtcNow.AddMinutes(-1)
        });

        var locationRepo = Substitute.For<ILocationDataRepository>();
        locationRepo.GetByDeviceIdAsync(deviceId).Returns(new List<LocationData>
        {
            new LocationData
            {
                DeviceId = deviceId,
                Latitude = 51.5,
                Longitude = -0.1,
                Speed = 40,
                Bearing = 0,
                Timestamp = DateTime.UtcNow
            }
        });

        var uow = Substitute.For<IUnitOfWork>();
        uow.Devices.Returns(deviceRepo);
        uow.LocationData.Returns(locationRepo);
        uow.Journeys.Returns(Substitute.For<IJourneyRepository>());

        var sut = new DeviceDiagnosticsService(uow, Substitute.For<ILogger<DeviceDiagnosticsService>>());

        // Act
        var result = await sut.RunSelfTestAsync(deviceId);

        // Assert
        result.Should().NotBeNull();
        result!.ConnectivityOk.Should().Be(expectedConnectivityOk);
        result.BatteryOk.Should().Be(expectedBatteryOk);
        result.SignalOk.Should().Be(expectedSignalOk);
        result.LocationDataOk.Should().BeTrue();
        result.AllOk.Should().Be(expectedConnectivityOk && expectedBatteryOk && expectedSignalOk);

        return (sut, result);
    }

    /// <summary>
    /// Validates that a device with marginal signal strength is properly classified.
    /// Useful for testing edge cases in signal quality thresholds.
    /// </summary>
    /// <param name="deviceId">The device identifier</param>
    /// <param name="signalStrength">Signal strength in dBm (marginal range: -90 to -100)</param>
    /// <param name="expectedQuality">Expected signal quality classification</param>
    public static async Task ValidateMarginalSignalQualityAsync(
        this DeviceDiagnosticsServiceTests _,
        string deviceId,
        int signalStrength,
        string expectedQuality)
    {
        // Arrange
        var (sut, _) = BuildSut(deviceId, signal: signalStrength);

        // Act
        var report = await sut.GetDiagnosticsAsync(deviceId);

        // Assert
        report.Should().NotBeNull();
        report!.SignalQuality.Should().Be(expectedQuality);
    }

    /// <summary>
    /// Creates a pre-configured device diagnostics service with specific device properties.
    /// Useful for testing various device configurations without repeating setup code.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="batteryLevel">Battery level percentage</param>
    /// <param name="signalStrength">Signal strength in dBm</param>
    /// <param name="status">Device status</param>
    /// <returns>Configured service instance</returns>
    public static DeviceDiagnosticsService CreateDiagnosticsService(
        this DeviceDiagnosticsServiceTests _,
        string deviceId = "test-device",
        int batteryLevel = 80,
        int signalStrength = -65,
        DeviceStatus status = DeviceStatus.Online)
    {
        var device = new Device
        {
            Id = deviceId,
            Imei = "test-imei-12345",
            DeviceName = "Test Device",
            Status = status,
            BatteryLevel = batteryLevel,
            SignalStrength = signalStrength,
            ConnectionCount = 50,
            LastSeen = DateTime.UtcNow.AddMinutes(-2)
        };

        var deviceRepo = Substitute.For<IDeviceRepository>();
        deviceRepo.GetByIdAsync(deviceId).Returns(device);

        var locationRepo = Substitute.For<ILocationDataRepository>();
        locationRepo.GetByDeviceIdAsync(deviceId).Returns(new List<LocationData>
        {
            new LocationData
            {
                DeviceId = deviceId,
                Latitude = 51.5,
                Longitude = -0.1,
                Speed = 45,
                Bearing = 90,
                Timestamp = DateTime.UtcNow.AddMinutes(-1)
            }
        });

        var uow = Substitute.For<IUnitOfWork>();
        uow.Devices.Returns(deviceRepo);
        uow.LocationData.Returns(locationRepo);
        uow.Journeys.Returns(Substitute.For<IJourneyRepository>());

        return new DeviceDiagnosticsService(uow, Substitute.For<ILogger<DeviceDiagnosticsService>>());
    }

    private static (DeviceDiagnosticsService Sut, Device Device) BuildSut(
        string deviceId = "test-device",
        int battery = 80,
        int signal = -65,
        DeviceStatus status = DeviceStatus.Online,
        int locationCount = 5,
        int journeyCount = 2)
    {
        var device = new Device
        {
            Id = deviceId,
            Imei = "123456789012345",
            DeviceName = "Tracker X",
            Protocol = ProtocolType.GT06,
            Status = status,
            LastSeen = DateTime.UtcNow.AddMinutes(-2),
            BatteryLevel = battery,
            SignalStrength = signal,
            ConnectionCount = 42
        };

        var locations = Enumerable.Range(0, locationCount).Select(i => new LocationData
        {
            DeviceId = device.Id,
            Latitude = 51.5 + i * 0.001,
            Longitude = -0.1,
            Speed = 40,
            Bearing = 0,
            Timestamp = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();

        var journeys = Enumerable.Range(0, journeyCount).Select(i => new Journey
        {
            Id = $"j-{i}",
            DeviceId = device.Id,
            Status = 1
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
}
