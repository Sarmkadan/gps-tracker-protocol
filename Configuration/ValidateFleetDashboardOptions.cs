using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using GpsTrackerProtocol.Configuration;

namespace GpsTrackerProtocol.Configuration;

/// <summary>
/// Validates <see cref="FleetDashboardOptions"/> ensuring that numeric and time‑span
/// values fall within sensible ranges. The validator is intended to be added to the
/// DI container via <c>services.AddOptions&lt;FleetDashboardOptions&gt;()
/// .ValidateOptions&lt;FleetDashboardOptionsValidator&gt;()</c>.
/// </summary>
public sealed class FleetDashboardOptionsValidator : IValidateOptions<FleetDashboardOptions>
{
    public ValidateOptionsResult Validate(string? name, FleetDashboardOptions? options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("FleetDashboardOptions instance is null.");
        }

        var failures = new List<string>();

        // DefaultAlgorithm – enum, always valid (no check needed)

        // DefaultFuelPricePerLiter – must be non‑negative
        if (options.DefaultFuelPricePerLiter < 0)
        {
            failures.Add($"{nameof(options.DefaultFuelPricePerLiter)} cannot be negative (actual: {options.DefaultFuelPricePerLiter}).");
        }

        // AverageRoadSpeedKmh – must be positive
        if (options.AverageRoadSpeedKmh <= 0)
        {
            failures.Add($"{nameof(options.AverageRoadSpeedKmh)} must be greater than 0 (actual: {options.AverageRoadSpeedKmh}).");
        }

        // MaxStopsPerRoute – must be positive
        if (options.MaxStopsPerRoute <= 0)
        {
            failures.Add($"{nameof(options.MaxStopsPerRoute)} must be greater than 0 (actual: {options.MaxStopsPerRoute}).");
        }

        // MaxFleetSize – non‑negative (0 means unlimited)
        if (options.MaxFleetSize < 0)
        {
            failures.Add($"{nameof(options.MaxFleetSize)} cannot be negative (actual: {options.MaxFleetSize}).");
        }

        // SnapshotCacheTtl – cannot be negative
        if (options.SnapshotCacheTtl < TimeSpan.Zero)
        {
            failures.Add($"{nameof(options.SnapshotCacheTtl)} cannot be negative (actual: {options.SnapshotCacheTtl}).");
        }

        // LowFuelThresholdLiters – non‑negative
        if (options.LowFuelThresholdLiters < 0)
        {
            failures.Add($"{nameof(options.LowFuelThresholdLiters)} cannot be negative (actual: {options.LowFuelThresholdLiters}).");
        }

        // EnableDistanceBasedFallback – bool, always valid

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
