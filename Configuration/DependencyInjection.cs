// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Services;

/// <summary>
/// Extension methods for configuring dependency injection.
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

        // Register services
        services.AddSingleton<IProtocolParserService, ProtocolParserService>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<ILocationDataService, LocationDataService>();
        services.AddSingleton<ICommandService, CommandService>();
        services.AddSingleton<IJourneyService, JourneyService>();

        return services;
    }

    /// <summary>
    /// Registers logging configuration.
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
