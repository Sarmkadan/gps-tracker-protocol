#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

using System.Globalization;

/// <summary>
/// Provides validation helpers for <see cref="FleetDashboardOptions"/> configuration.
/// </summary>
public static class FleetDashboardOptionsValidation
{
    /// <summary>
    /// Validates the <see cref="FleetDashboardOptions"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this FleetDashboardOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate DefaultFuelPricePerLiter
        if (value.DefaultFuelPricePerLiter < 0)
        {
            errors.Add($"DefaultFuelPricePerLiter must be non-negative, but was {value.DefaultFuelPricePerLiter.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate AverageRoadSpeedKmh
        if (value.AverageRoadSpeedKmh <= 0)
        {
            errors.Add($"AverageRoadSpeedKmh must be positive, but was {value.AverageRoadSpeedKmh.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate MaxStopsPerRoute
        if (value.MaxStopsPerRoute <= 0)
        {
            errors.Add($"MaxStopsPerRoute must be positive, but was {value.MaxStopsPerRoute}.");
        }

        // Validate MaxFleetSize
        if (value.MaxFleetSize < 0)
        {
            errors.Add($"MaxFleetSize must be non-negative, but was {value.MaxFleetSize}.");
        }

        // Validate SnapshotCacheTtl
        if (value.SnapshotCacheTtl < TimeSpan.Zero)
        {
            errors.Add($"SnapshotCacheTtl must be non-negative, but was {value.SnapshotCacheTtl}.");
        }

        // Validate LowFuelThresholdLiters
        if (value.LowFuelThresholdLiters < 0)
        {
            errors.Add($"LowFuelThresholdLiters must be non-negative, but was {value.LowFuelThresholdLiters.ToString(CultureInfo.InvariantCulture)}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="FleetDashboardOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this FleetDashboardOptions value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="FleetDashboardOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the options are invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this FleetDashboardOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "FleetDashboardOptions validation failed. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }
}