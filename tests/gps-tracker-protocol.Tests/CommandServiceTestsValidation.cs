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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this CommandServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate private fields via reflection since they're private
            var fields = value.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.Name == "_commandRepository")
                {
                    if (field.GetValue(value) == null)
                    {
                        problems.Add("Command repository (_commandRepository) is null");
                    }
                }
                else if (field.Name == "_deviceRepository")
                {
                    if (field.GetValue(value) == null)
                    {
                        problems.Add("Device repository (_deviceRepository) is null");
                    }
                }
                else if (field.Name == "_sut")
                {
                    var sut = field.GetValue(value);
                    if (sut == null)
                    {
                        problems.Add("System under test (_sut) is null");
                    }
                }
            }

            return problems;
        }

        /// <summary>
        /// Determines whether the CommandServiceTests instance is valid.
        /// </summary>
        /// <param name="value">The CommandServiceTests instance to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValid(this CommandServiceTests value) => Validate(value).Count == 0;

        /// <summary>
        /// Ensures that the CommandServiceTests instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The CommandServiceTests instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
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