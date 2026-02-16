#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Infrastructure;
using GpsTrackerProtocol.Formatting;
using GpsTrackerProtocol.Integration;
using GpsTrackerProtocol.Caching;
using GpsTrackerProtocol.Events;
using GpsTrackerProtocol.BackgroundWorkers;
using GpsTrackerProtocol.CLI;
using GpsTrackerProtocol.Utilities;

/// <summary>
/// Extension methods for configuring dependency injection.
/// Registers all core services, infrastructure, integrations, and formatters.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all GPS tracker protocol services and repositories.
    /// </summary>
    public static IServiceCollection AddGpsTrackerServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

        // Register core services
        services.AddSingleton<IProtocolParserService, ProtocolParserService>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<ILocationDataService, LocationDataService>();
        services.AddSingleton<ICommandService, CommandService>();
        services.AddSingleton<IJourneyService, JourneyService>();

        // Register infrastructure
        services.AddSingleton<ILoggingPipeline, LoggingPipeline>();
        services.AddSingleton<IValidationPipeline>(sp => new ValidationPipeline(
            new FrameValidator(),
            new LocationValidator(),
            new DeviceValidator()
        ));
        services.AddSingleton<IRateLimiter>(sp => new RateLimitingService(
            new RateLimitConfig { MaxTokens = 100, RefillRate = 10 }
        ));

        // Register formatters
        services.AddSingleton<IJsonFormatter, JsonFormatter>();
        services.AddSingleton<ICsvFormatter, CsvFormatter>();
        services.AddSingleton<IGeoJsonFormatter, GeoJsonFormatter>();

        // Register caching
        services.AddSingleton<ICachingService, CachingService>();
        services.AddSingleton<IDistributedCache, InMemoryDistributedCache>();

        // Register events
        services.AddSingleton<IEventPublisher, EventPublisher>();

        // Register integrations
        services.AddSingleton<IHttpClientFactory, HttpClientFactoryService>();
        services.AddSingleton<IWebhookClient, WebhookClient>();
        services.AddSingleton<IGeocodingService, GeocodingService>();
        services.AddSingleton<IWeatherApiClient, WeatherApiClient>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Register services
        services.AddSingleton<IGeofenceService, GeofenceService>();
        services.AddSingleton<IAnalyticsService, AnalyticsService>();

        // Register background workers
        services.AddSingleton<IBackgroundProcessingService, BackgroundProcessingService>();
        services.AddSingleton<LocationAggregationWorker>();
        services.AddSingleton<JourneyAnalyticsWorker>();

        // Register CLI
        services.AddSingleton<ICommandLineInterface, CommandLineInterface>();

        // Register utilities
        services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
        services.AddSingleton<ISimulationService, SimulationService>();

        return services;
    }

    /// <summary>
    /// Registers logging configuration with console output.
    /// </summary>
    public static IServiceCollection AddGpsTrackerLogging(this IServiceCollection services)
    {
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }
}
