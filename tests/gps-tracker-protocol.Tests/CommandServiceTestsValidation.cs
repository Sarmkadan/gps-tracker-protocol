#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace gps_tracker_protocol.Tests
{
    /// <summary>
    /// Validation helpers for CommandServiceTests class.
    /// </summary>
    public static class CommandServiceTestsValidation
    {
        /// <summary>
        /// Validates the CommandServiceTests instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The CommandServiceTests instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(this CommandServiceTests value)
        {
            var problems = new List<string>();

            if (value == null)
            {
                problems.Add("CommandServiceTests instance is null");
                return problems;
            }

            // Validate private fields (these are set in constructor)
            // _commandRepository and _deviceRepository are mocked via NSubstitute, so we can't validate them deeply
            // _sut is the CommandService instance under test

            return problems;
        }

        /// <summary>
        /// Determines whether the CommandServiceTests instance is valid.
        /// </summary>
        /// <param name="value">The CommandServiceTests instance to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValid(this CommandServiceTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the CommandServiceTests instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The CommandServiceTests instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
        public static void EnsureValid(this CommandServiceTests value)
        {
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"CommandServiceTests validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
            }
        }
    }
}