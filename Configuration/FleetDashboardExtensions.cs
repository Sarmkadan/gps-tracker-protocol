#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

using Microsoft.Extensions.DependencyInjection;
using GpsTrackerProtocol.Services;

/// <summary>
/// Extension methods for registering the fleet analytics dashboard feature with the DI container.
/// </summary>
public static class FleetDashboardExtensions
{
    /// <summary>
    /// Registers the fleet analytics dashboard together with the route optimisation engine
    /// and fuel tracking service.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this method <em>after</em> <see cref="DependencyInjection.AddGpsTrackerServices"/>;
    /// the fleet dashboard depends on <c>IDeviceService</c> and <c>ILocationDataService</c>
    /// that are registered by that method.
    /// </para>
    /// <para>Example setup:</para>
    /// <code>
    /// services
    ///     .AddGpsTrackerServices()
    ///     .AddFleetAnalyticsDashboard(opts =>
    ///     {
    ///         opts.DefaultFuelPricePerLiter = 1.79;
    ///         opts.MaxStopsPerRoute = 100;
    ///         opts.DefaultAlgorithm = RouteOptimizationAlgorithm.TwoOpt;
    ///     });
    /// </code>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services into.</param>
    /// <param name="configure">
    /// Optional delegate for customising <see cref="FleetDashboardOptions"/>.
    /// When <see langword="null"/>, default option values apply.
    /// </param>
    /// <returns><paramref name="services"/> for fluent method chaining.</returns>
    public static IServiceCollection AddFleetAnalyticsDashboard(
        this IServiceCollection services,
        Action<FleetDashboardOptions>? configure = null)
    {
        var options = new FleetDashboardOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IFuelTrackingService, FuelTrackingService>();
        services.AddSingleton<IRouteOptimizationEngine, RouteOptimizationEngine>();
        services.AddSingleton<IFleetDashboardService, FleetDashboardService>();

        return services;
    }
}
