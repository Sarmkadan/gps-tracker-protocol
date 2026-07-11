#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

namespace gps_tracker_protocol.Tests
{
    /// <summary>
    /// Extension methods for <see cref="CommandServiceTests"/> to provide additional test utilities.
    /// </summary>
    public static class CommandServiceTestsExtensions
    {
        /// <summary>
        /// Creates a command with the specified parameters and marks it as sent.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="payload">The command payload.</param>
        /// <returns>The created command.</returns>
        /// <exception cref="ArgumentNullException">Thrown when deviceId or commandType is null or empty.</exception>
        public static Command CreateSentCommand(
            this CommandServiceTests tests,
            string deviceId,
            string commandType,
            string? payload = null)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentException.ThrowIfNullOrEmpty(commandType);

            var command = new Command
            {
                Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                DeviceId = deviceId,
                CommandType = commandType,
                Payload = payload ?? "test_payload",
                SentTime = DateTime.UtcNow,
                IsSent = true
            };

            return command;
        }

        /// <summary>
        /// Creates a device with the specified ID and active status.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="imei">The device IMEI.</param>
        /// <param name="deviceName">The device name.</param>
        /// <param name="isActive">Whether the device is active.</param>
        /// <returns>The created device.</returns>
        /// <exception cref="ArgumentNullException">Thrown when deviceId is null or empty.</exception>
        public static Device CreateDevice(
            this CommandServiceTests tests,
            string deviceId,
            string? imei = null,
            string? deviceName = null,
            bool isActive = true)
        {
            ArgumentNullException.ThrowIfNull(deviceId);

            return new Device
            {
                Id = deviceId,
                Imei = imei ?? deviceId,
                DeviceName = deviceName ?? $"Device_{deviceId}",
                IsActive = isActive,
                Status = DeviceStatus.Online,
                LastSeen = DateTime.UtcNow,
                RegistrationDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a command with the specified parameters.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="payload">The command payload.</param>
        /// <returns>The created command.</returns>
        /// <exception cref="ArgumentNullException">Thrown when deviceId or commandType is null or empty.</exception>
        public static Command CreateCommand(
            this CommandServiceTests tests,
            string deviceId,
            string commandType,
            string? payload = null)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentException.ThrowIfNullOrEmpty(commandType);

            return new Command
            {
                Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                DeviceId = deviceId,
                CommandType = commandType,
                Payload = payload ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                Status = CommandStatus.Pending
            };
        }

        /// <summary>
        /// Creates a list of commands for testing batch operations.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="count">Number of commands to create.</param>
        /// <param name="commandType">The command type (optional prefix).</param>
        /// <returns>List of created commands.</returns>
        public static IReadOnlyList<Command> CreateCommandList(
            this CommandServiceTests tests,
            string deviceId,
            int count,
            string? commandType = null)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

            var commands = new List<Command>();
            for (int i = 0; i < count; i++)
            {
                commands.Add(new Command
                {
                    Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                    DeviceId = deviceId,
                    CommandType = commandType ?? "TEST_COMMAND",
                    Payload = $"payload_{i}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                    Status = CommandStatus.Pending
                });
            }

            return commands.AsReadOnly();
        }
    }
}