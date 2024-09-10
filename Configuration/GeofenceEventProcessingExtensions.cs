#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

using Microsoft.Extensions.DependencyInjection;
using GpsTrackerProtocol.Services;

/// <summary>
/// Extension methods for registering geofence event processing with the DI container.
/// </summary>
public static class GeofenceEventProcessingExtensions
{
    /// <summary>
    /// Registers <see cref="IGeofenceEventProcessor"/> so that callers can inject it
    /// and call <c>ProcessLocationAsync</c> after each parsed GPS frame.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="DependencyInjection.AddGpsTrackerServices"/> to be called first,
    /// as this service depends on <c>IGeofenceService</c>, <c>IWebhookClient</c>,
    /// <c>IEventPublisher</c>, and <c>INotificationService</c>.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddGeofenceEventProcessing(this IServiceCollection services)
    {
        services.AddSingleton<IGeofenceEventProcessor, GeofenceEventProcessor>();
        return services;
    }
}
