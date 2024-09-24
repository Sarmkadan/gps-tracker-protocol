#nullable enable
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
    /// <summary>
    /// Tests for the DeviceService class.
    /// </summary>
    public class DeviceServiceTests
    {
        private readonly IRepository<Device> _deviceRepository;
        private readonly DeviceService _sut;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceServiceTests"/> class.
        /// </summary>
        public DeviceServiceTests()
        {
            _deviceRepository = Substitute.For<IRepository<Device>>();
            _sut = new DeviceService(_deviceRepository);
        }

        [Fact]
        public async Task RegisterDeviceAsync_ShouldAddDevice()
        {
            /// <summary>
            /// Tests that the RegisterDeviceAsync method adds a new device to the repository.
            /// </summary>
            /// <param name="deviceId">The ID of the device to register.</param>
            /// <returns>A task that represents the asynchronous operation.</returns>
            // Arrange
            var deviceId = "newDevice";
            _deviceRepository.GetByIdAsync(deviceId).Returns((Device)null);

            // Act
            var device = await _sut.RegisterDeviceAsync(deviceId).ConfigureAwait(false);

            // Assert
            device.Should().NotBeNull();
            device.Id.Should().Be(deviceId);
            device.IsActive.Should().BeTrue();
            device.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            await _deviceRepository.Received(1).AddAsync(Arg.Is<Device>(d => d.Id == deviceId)).ConfigureAwait(false);
        }

        [Fact]
        public async Task RegisterDeviceAsync_ShouldReturnExistingDevice_IfAlreadyRegistered()
        {
            /// <summary>
            /// Tests that the RegisterDeviceAsync method returns the existing device if it is already registered.
            /// </summary>
            /// <param name="deviceId">The ID of the device to register.</param>
            /// <returns>A task that represents the asynchronous operation.</returns>
            // Arrange
            var deviceId = "existingDevice";
            var existingDevice = new Device { Id = deviceId, IsActive = true };
            _deviceRepository.GetByIdAsync(deviceId).Returns(existingDevice);

            // Act
            var device = await _sut.RegisterDeviceAsync(deviceId).ConfigureAwait(false);

            // Assert
            device.Should().Be(existingDevice);
            await _deviceRepository.DidNotReceive().AddAsync(Arg.Any<Device>()).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetDeviceByIdAsync_ShouldReturnDevice()
        {
            /// <summary>
            /// Tests that the GetDeviceByIdAsync method returns the device with the specified ID.
            /// </summary>
            /// <param name="deviceId">The ID of the device to retrieve.</param>
            /// <returns>A task that represents the asynchronous operation.</returns>
            // Arrange
            var deviceId = "device1";
            var expectedDevice = new Device { Id = deviceId, IsActive = true };
            _deviceRepository.GetByIdAsync(deviceId).Returns(expectedDevice);

            // Act
            var actualDevice = await _sut.GetDeviceByIdAsync(deviceId).ConfigureAwait(false);

            // Assert
            actualDevice.Should().Be(expectedDevice);
        }

        [Fact]
        public async Task GetDeviceByIdAsync_ShouldReturnNull_WhenDeviceNotFound()
        {
            /// <summary>
            /// Tests that the GetDeviceByIdAsync method returns null when the device is not found.
            /// </summary>
            /// <param name="deviceId">The ID of the device to retrieve.</param>
            /// <returns>A task that represents the asynchronous operation.</returns>
            // Arrange
            var deviceId = "nonexistentDevice";
            _deviceRepository.GetByIdAsync(deviceId).Returns((Device)null);

            // Act
            var actualDevice = await _sut.GetDeviceByIdAsync(deviceId).ConfigureAwait(false);

            // Assert
            actualDevice.Should().BeNull();
        }

        [Fact]
        public async Task UpdateDeviceStatusAsync_ShouldUpdateDevice()
        {
            /// <summary>
            /// Tests that the UpdateDeviceStatusAsync method updates the device status.
            /// </summary>
            /// <param name="deviceId">The ID of the device to update.</param>
            /// <param name="isActive">The new status of the device.</param>
            /// <returns>A task that represents the asynchronous operation.</returns>
            // Arrange
            var deviceId = "device1";
            var device = new Device { Id = deviceId, IsActive = true };
            _deviceRepository.GetByIdAsync(deviceId).Returns(device);

            // Act
            await _sut.UpdateDeviceStatusAsync(deviceId, false).ConfigureAwait(false);

            // Assert
            device.IsActive.Should().BeFalse();
            await _deviceRepository.Received(1).UpdateAsync(device).ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateDeviceStatusAsync_ShouldDoNothing_WhenDeviceNotFound()
        {
            /// <summary>
            /// Tests that the UpdateDeviceStatusAsync method does nothing when the device is not found.
            /// </summary>
            /// <param name="deviceId">The ID of the device to update.</param>
            /// <param name="isActive">The new status of the device.</param>
            /// <returns>A task that represents the asynchronous operation.</returns>
            // Arrange
            var deviceId = "nonexistentDevice";
            _deviceRepository.GetByIdAsync(deviceId).Returns((Device)null);

            // Act
            await _sut.UpdateDeviceStatusAsync(deviceId, false).ConfigureAwait(false);

            // Assert
            await _deviceRepository.DidNotReceive().UpdateAsync(Arg.Any<Device>()).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetAllDevicesAsync_ShouldReturnAllDevices()
        {
            /// <summary>
            /// Tests that the GetAllDevicesAsync method returns all devices.
            /// </summary>
            /// <returns>A task that represents the asynchronous operation.</returns>
            // Arrange
            var devices = new List<Device>
            {
                new Device { Id = "dev1" },
                new Device { Id = "dev2" }
            };
            _deviceRepository.GetAllAsync().Returns(devices);

            // Act
            var result = await _sut.GetAllDevicesAsync().ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(d => d.Id == "dev1");
            result.Should().Contain(d => d.Id == "dev2");
        }
    }
}
