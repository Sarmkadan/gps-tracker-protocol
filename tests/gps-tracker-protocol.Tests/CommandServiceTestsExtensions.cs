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
    /// <remarks>
    /// This static class contains factory methods for creating test entities like <see cref="Command"/>,
    /// <see cref="Device"/>, and collections of commands to simplify test setup and improve readability.
    /// </remarks>
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commandType"/> is null or empty.</exception>
        public static Command CreateSentCommand(
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
                Payload = payload ?? "test_payload",
                SentTime = DateTime.UtcNow,
                IsSent = true
            };
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="commandType"/> is null or empty.</exception>
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
        /// <param name="count">Number of commands to create. Must be greater than zero.</param>
        /// <param name="commandType">The command type (optional prefix).</param>
        /// <returns>List of created commands.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
        public static IReadOnlyList<Command> CreateCommandList(
            this CommandServiceTests tests,
            string deviceId,
            int count,
            string? commandType = null)
        {
            ArgumentNullException.ThrowIfNull(deviceId);
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

            return Enumerable.Range(0, count)
                .Select(i => new Command
                {
                    Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                    DeviceId = deviceId,
                    CommandType = commandType ?? "TEST_COMMAND",
                    Payload = $"payload_{i}",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                    Status = CommandStatus.Pending
                })
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Creates a response for testing command execution results.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="commandId">The command ID.</param>
        /// <param name="success">Whether the command execution succeeded.</param>
        /// <param name="responseData">Optional response data.</param>
        /// <returns>The created command response.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandId"/> is null.</exception>
        public static CommandResponse CreateCommandResponse(
            this CommandServiceTests tests,
            string commandId,
            bool success,
            string? responseData = null)
        {
            ArgumentNullException.ThrowIfNull(commandId);

            return new CommandResponse
            {
                Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                CommandId = commandId,
                Success = success,
                ResponseData = responseData ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Mock response for testing command execution.
    /// </summary>
    public class CommandResponse
    {
        public string Id { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ResponseData { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}