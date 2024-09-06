// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using NSubstitute;
using FluentAssertions;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace gps_tracker_protocol.Tests
{
    public class DeviceServiceTests
    {
        private readonly IRepository<Device> _deviceRepository;
        private readonly DeviceService _sut;

        public DeviceServiceTests()
        {
            _deviceRepository = Substitute.For<IRepository<Device>>();
            _sut = new DeviceService(_deviceRepository);
        }

        [Fact]
        public async Task RegisterDeviceAsync_ShouldAddDevice()
        {
            // Arrange
            var deviceId = "newDevice";
            _deviceRepository.GetByIdAsync(deviceId).Returns((Device)null);

            // Act
            var device = await _sut.RegisterDeviceAsync(deviceId);

            // Assert
            device.Should().NotBeNull();
            device.Id.Should().Be(deviceId);
            device.IsActive.Should().BeTrue();
            device.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            await _deviceRepository.Received(1).AddAsync(Arg.Is<Device>(d => d.Id == deviceId));
        }

        [Fact]
        public async Task RegisterDeviceAsync_ShouldReturnExistingDevice_IfAlreadyRegistered()
        {
            // Arrange
            var deviceId = "existingDevice";
            var existingDevice = new Device { Id = deviceId, IsActive = true };
            _deviceRepository.GetByIdAsync(deviceId).Returns(existingDevice);

            // Act
            var device = await _sut.RegisterDeviceAsync(deviceId);

            // Assert
            device.Should().Be(existingDevice);
            await _deviceRepository.DidNotReceive().AddAsync(Arg.Any<Device>());
        }

        [Fact]
        public async Task GetDeviceByIdAsync_ShouldReturnDevice()
        {
            // Arrange
            var deviceId = "device1";
            var expectedDevice = new Device { Id = deviceId, IsActive = true };
            _deviceRepository.GetByIdAsync(deviceId).Returns(expectedDevice);

            // Act
            var actualDevice = await _sut.GetDeviceByIdAsync(deviceId);

            // Assert
            actualDevice.Should().Be(expectedDevice);
        }

        [Fact]
        public async Task GetDeviceByIdAsync_ShouldReturnNull_WhenDeviceNotFound()
        {
            // Arrange
            var deviceId = "nonexistentDevice";
            _deviceRepository.GetByIdAsync(deviceId).Returns((Device)null);

            // Act
            var actualDevice = await _sut.GetDeviceByIdAsync(deviceId);

            // Assert
            actualDevice.Should().BeNull();
        }

        [Fact]
        public async Task UpdateDeviceStatusAsync_ShouldUpdateDevice()
        {
            // Arrange
            var deviceId = "device1";
            var device = new Device { Id = deviceId, IsActive = true };
            _deviceRepository.GetByIdAsync(deviceId).Returns(device);

            // Act
            await _sut.UpdateDeviceStatusAsync(deviceId, false);

            // Assert
            device.IsActive.Should().BeFalse();
            await _deviceRepository.Received(1).UpdateAsync(device);
        }

        [Fact]
        public async Task UpdateDeviceStatusAsync_ShouldDoNothing_WhenDeviceNotFound()
        {
            // Arrange
            var deviceId = "nonexistentDevice";
            _deviceRepository.GetByIdAsync(deviceId).Returns((Device)null);

            // Act
            await _sut.UpdateDeviceStatusAsync(deviceId, false);

            // Assert
            await _deviceRepository.DidNotReceive().UpdateAsync(Arg.Any<Device>());
        }

        [Fact]
        public async Task GetAllDevicesAsync_ShouldReturnAllDevices()
        {
            // Arrange
            var devices = new List<Device>
            {
                new Device { Id = "dev1" },
                new Device { Id = "dev2" }
            };
            _deviceRepository.GetAllAsync().Returns(devices);

            // Act
            var result = await _sut.GetAllDevicesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(d => d.Id == "dev1");
            result.Should().Contain(d => d.Id == "dev2");
        }
    }
}
