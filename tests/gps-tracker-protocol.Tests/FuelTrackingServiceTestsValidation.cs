using System;
using System.Collections.Generic;
using System.Linq;

namespace GpsTrackerProtocol.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="FuelTrackingServiceTests"/>.
    /// </summary>
    public static class FuelTrackingServiceTestsValidation
    {
        /// <summary>
        /// Validates the supplied <see cref="FuelTrackingServiceTests"/> instance and returns a list of human‑readable problems.
        /// </summary>
        /// <param name="value">The test class instance to validate.</param>
        /// <returns>A read‑only list of validation error messages. The list is empty when the instance is considered valid.</returns>
        public static IReadOnlyList<string> Validate(this FuelTrackingServiceTests? value)
        {
            var problems = new List<string>();

            // The instance itself must not be null.
            if (value is null)
            {
                problems.Add("FuelTrackingServiceTests instance is null.");
                // No further checks are possible if the instance is null.
                return problems;
            }

            // Since the public members of FuelTrackingServiceTests are only test methods,
            // there are no data properties to validate. However, we still perform a sanity
            // check that the type contains the expected test methods. This guards against
            // accidental renaming or removal of required tests.

            var expectedMethodNames = new[]
            {
                nameof(FuelTrackingServiceTests.RecordFuelEventAsync_ShouldStoreRecordSuccessfully),
                nameof(FuelTrackingServiceTests.RecordFuelEventAsync_ShouldThrowException_WhenFuelAmountIsZeroOrNegative),
                nameof(FuelTrackingServiceTests.GetRecordsAsync_ShouldReturnFilteredRecords),
                nameof(FuelTrackingServiceTests.DeleteRecordAsync_ShouldReturnTrue_WhenRecordExists),
                nameof(FuelTrackingServiceTests.GetReportAsync_ShouldCalculateCorrectTotals),
                nameof(FuelTrackingServiceTests.EstimateFuelLiters_ShouldReturnZero_WhenInputsAreInvalid)
            };

            var actualMethods = value.GetType()
                                     .GetMethods(System.Reflection.BindingFlags.Instance |
                                                 System.Reflection.BindingFlags.Public |
                                                 System.Reflection.BindingFlags.DeclaredOnly)
                                     .Select(m => m.Name)
                                     .ToHashSet(StringComparer.Ordinal);

            foreach (var expected in expectedMethodNames)
            {
                if (!actualMethods.Contains(expected))
                {
                    problems.Add($"Expected test method '{expected}' is missing.");
                }
            }

            return problems;
        }

        /// <summary>
        /// Determines whether the supplied <see cref="FuelTrackingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test class instance to check.</param>
        /// <returns><c>true</c> if no validation problems were found; otherwise, <c>false</c>.</returns>
        public static bool IsValid(this FuelTrackingServiceTests? value)
        {
            return !(value?.Validate().Any() ?? true);
        }

        /// <summary>
        /// Ensures that the supplied <see cref="FuelTrackingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test class instance to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation problems are found. The exception message contains the list of problems.</exception>
        public static void EnsureValid(this FuelTrackingServiceTests? value)
        {
            var problems = value.Validate();
            if (problems.Count > 0)
            {
                var message = $"FuelTrackingServiceTests validation failed:{Environment.NewLine}" +
                              string.Join(Environment.NewLine, problems.Select(p => $"- {p}"));
                throw new ArgumentException(message, nameof(value));
            }
        }
    }
}
