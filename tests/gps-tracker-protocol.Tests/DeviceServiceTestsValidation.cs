using System;
using System.Collections.Generic;
using System.Linq;

namespace gps_tracker_protocol.Tests
{
    /// <summary>
    /// Validation helpers for DeviceServiceTests class.
    /// </summary>
    public static class DeviceServiceTestsValidation
    {
        /// <summary>
        /// Validates the DeviceServiceTests instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The DeviceServiceTests instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(this DeviceServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate private fields (these are set in constructor)
            // _deviceRepository is mocked via NSubstitute, so we can't validate it deeply
            // _sut is the DeviceService instance under test

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the DeviceServiceTests instance is valid.
        /// </summary>
        /// <param name="value">The DeviceServiceTests instance to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValid(this DeviceServiceTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the DeviceServiceTests instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The DeviceServiceTests instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
        public static void EnsureValid(this DeviceServiceTests value)
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"DeviceServiceTests validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }
    }
}
