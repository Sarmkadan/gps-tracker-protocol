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
    public class CommandServiceTests
    {
        private readonly IRepository<Command> _commandRepository;
        private readonly IRepository<Device> _deviceRepository;
        private readonly CommandService _sut;

        public CommandServiceTests()
        {
            _commandRepository = Substitute.For<IRepository<Command>>();
            _deviceRepository = Substitute.For<IRepository<Device>>();
            _sut = new CommandService(_commandRepository, _deviceRepository);
        }

        [Fact]
        public async Task SendCommandAsync_ShouldAddCommandAndMarkAsSent()
        {
            // Arrange
            var deviceId = "device1";
            var commandType = "TEST_COMMAND";
            var payload = "test_payload";

            _deviceRepository.GetByIdAsync(deviceId).Returns(new Device { Id = deviceId, IsActive = true });

            // Act
            var command = await _sut.SendCommandAsync(deviceId, commandType, payload).ConfigureAwait(false);

            // Assert
            command.Should().NotBeNull();
            command.DeviceId.Should().Be(deviceId);
            command.CommandType.Should().Be(commandType);
            command.Payload.Should().Be(payload);
            command.SentTime.Should().NotBeNull();
            command.IsSent.Should().BeTrue();
            await _commandRepository.Received(1).AddAsync(command).ConfigureAwait(false);
        }

        [Fact]
        public async Task SendCommandAsync_ShouldReturnNull_WhenDeviceNotFound()
        {
            // Arrange
            var deviceId = "nonexistentDevice";
            var commandType = "TEST_COMMAND";
            var payload = "test_payload";

            _deviceRepository.GetByIdAsync(deviceId).Returns((Device)null);

            // Act
            var command = await _sut.SendCommandAsync(deviceId, commandType, payload).ConfigureAwait(false);

            // Assert
            command.Should().BeNull();
            await _commandRepository.DidNotReceive().AddAsync(Arg.Any<Command>()).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetCommandsForDeviceAsync_ShouldReturnCommands()
        {
            // Arrange
            var deviceId = "device1";
            var commands = new List<Command>
            {
                new Command { Id = "cmd1", DeviceId = deviceId, CommandType = "TYPE1" },
                new Command { Id = "cmd2", DeviceId = deviceId, CommandType = "TYPE2" }
            };
            _commandRepository.FindManyAsync(Arg.Any<Func<Command, bool>>())
                              .Returns(ci => Task.FromResult(commands.FindAll(c => ci.Arg<Func<Command, bool>>().Invoke(c))));


            // Act
            var result = await _sut.GetCommandsForDeviceAsync(deviceId).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.CommandType == "TYPE1");
            result.Should().Contain(c => c.CommandType == "TYPE2");
        }

        [Fact]
        public async Task GetCommandsForDeviceAsync_ShouldReturnEmptyList_WhenNoCommandsForDevice()
        {
            // Arrange
            var deviceId = "device1";
            _commandRepository.FindManyAsync(Arg.Any<Func<Command, bool>>())
                              .Returns(new List<Command>());

            // Act
            var result = await _sut.GetCommandsForDeviceAsync(deviceId).ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AcknowledgeCommandAsync_ShouldMarkCommandAsAcknowledged()
        {
            // Arrange
            var commandId = "cmd1";
            var command = new Command { Id = commandId, IsSent = true, IsAcknowledged = false };
            _commandRepository.GetByIdAsync(commandId).Returns(command);

            // Act
            await _sut.AcknowledgeCommandAsync(commandId).ConfigureAwait(false);

            // Assert
            command.IsAcknowledged.Should().BeTrue();
            command.AcknowledgedTime.Should().NotBeNull();
            await _commandRepository.Received(1).UpdateAsync(command).ConfigureAwait(false);
        }

        [Fact]
        public async Task AcknowledgeCommandAsync_ShouldDoNothing_WhenCommandNotFound()
        {
            // Arrange
            var commandId = "nonexistentCmd";
            _commandRepository.GetByIdAsync(commandId).Returns((Command)null);

            // Act
            await _sut.AcknowledgeCommandAsync(commandId).ConfigureAwait(false);

            // Assert
            await _commandRepository.DidNotReceive().UpdateAsync(Arg.Any<Command>()).ConfigureAwait(false);
        }
    }
}
