#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Configuration options for the fleet analytics dashboard feature.
/// Mutate via the builder overload in <see cref="FleetDashboardExtensions.AddFleetAnalyticsDashboard"/>:
/// <code>
/// services.AddFleetAnalyticsDashboard(opts =>
/// {
///     opts.DefaultAlgorithm = RouteOptimizationAlgorithm.TwoOpt;
///     opts.DefaultFuelPricePerLiter = 1.79;
/// });
/// </code>
/// </summary>
public sealed class FleetDashboardOptions
{
    /// <summary>
    /// <c>IConfiguration</c> section key used when binding from appsettings.
    /// </summary>
    public const string SectionName = "FleetDashboard";

    /// <summary>
    /// Default route optimisation algorithm applied when no per-request override is specified.
    /// <para>Default: <see cref="RouteOptimizationAlgorithm.TwoOpt"/>.</para>
    /// </summary>
    public RouteOptimizationAlgorithm DefaultAlgorithm { get; set; } = RouteOptimizationAlgorithm.TwoOpt;

    /// <summary>
    /// Default fuel unit price in local currency.
    /// Used to estimate route cost when vehicles do not carry explicit per-litre prices in their fuel records.
    /// <para>Default: 1.65.</para>
    /// </summary>
    public double DefaultFuelPricePerLiter { get; set; } = 1.65;

    /// <summary>
    /// Assumed average road speed in km/h used to convert inter-stop distances to travel-time estimates.
    /// Calibrate for your region's typical mixed-road conditions.
    /// <para>Default: 50 km/h (urban mixed driving).</para>
    /// </summary>
    public double AverageRoadSpeedKmh { get; set; } = 50.0;

    /// <summary>
    /// Maximum number of stops accepted in a single route optimisation request.
    /// Requests that exceed this limit are rejected immediately to protect response-time guarantees.
    /// <para>Default: 200.</para>
    /// </summary>
    public int MaxStopsPerRoute { get; set; } = 200;

    /// <summary>
    /// Upper bound on the number of vehicles that may be registered in the fleet.
    /// A value of <c>0</c> means unlimited.
    /// <para>Default: 0 (unlimited).</para>
    /// </summary>
    public int MaxFleetSize { get; set; }

    /// <summary>
    /// Minimum idle interval between full dashboard snapshot rebuilds.
    /// Snapshots requested within this window may return a recently cached result.
    /// Set to <see cref="TimeSpan.Zero"/> to disable caching.
    /// <para>Default: 30 seconds.</para>
    /// </summary>
    public TimeSpan SnapshotCacheTtl { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// When <see langword="true"/>, fuel consumption for a reporting window is estimated from
    /// driven distance and the vehicle's <see cref="FleetVehicle.BaseConsumptionLper100km"/>
    /// whenever no explicit <see cref="FuelEventType.Consumption"/> records exist for that window.
    /// Disable this flag when strict fuel-record-only reporting is required.
    /// <para>Default: <see langword="true"/>.</para>
    /// </summary>
    public bool EnableDistanceBasedFallback { get; set; } = true;

    /// <summary>
    /// Minimum remaining fuel level in litres below which a vehicle is considered at low-fuel risk.
    /// Used when computing dashboard status flags and KPI alerts.
    /// <para>Default: 10 L.</para>
    /// </summary>
    public double LowFuelThresholdLiters { get; set; } = 10.0;
}
