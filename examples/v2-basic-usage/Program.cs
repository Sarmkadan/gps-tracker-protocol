#nullable enable
using System;
using System.Threading.Tasks;
using GpsTrackerProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace V2BasicUsage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create host builder with default configuration
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Add GPS Tracker Protocol services
                    services.AddGpsTrackerProtocol(options =>
                    {
                        // Configure basic options
                        options.DefaultProtocol = Domain.Enums.GpsProtocol.GT06;
                        options.MaxDevices = 1000;
                        options.CacheExpirationSeconds = 60;
                    });

                    // Add v2.0 features: Fleet Analytics Dashboard
                    services.AddFleetDashboard(options =>
                    {
                        // Enable route optimization
                        options.EnableRouteOptimization = true;

                        // Enable fuel tracking
                        options.EnableFuelTracking = true;

                        // Set update interval for analytics (in seconds)
                        options.AnalyticsUpdateInterval = 30;
                    });

                    // Add hosted service to run the GPS tracker
                    services.AddHostedService<GpsTrackerHostedService>();
                })
                .Build();

            // Run the host
            await host.RunAsync();
        }
    }

    // Hosted service that runs the GPS tracker protocol
    /// <summary>
    /// A hosted service that manages the lifecycle of GPS tracker protocol services and fleet analytics dashboard.
    /// This service coordinates the startup and shutdown of the GPS tracker protocol v2.0 system,
    /// including protocol processing and fleet analytics collection.
    /// </summary>
    /// <summary>
    /// Hosted service implementation for GPS tracker protocol v2.0 that manages protocol processing and fleet analytics.
    /// </summary>
    public class GpsTrackerHostedService : IHostedService
    {
        /// <summary>
        /// The GPS tracker protocol service instance.
        /// </summary>
        private readonly IGpsTrackerProtocolService _gpsTrackerService;

        /// <summary>
        /// The fleet dashboard analytics service instance.
        /// </summary>
        private readonly IFleetDashboardService _fleetDashboardService;

        /// <summary>
        /// The logger instance for recording service lifecycle events.
        /// </summary>
        private readonly ILogger<GpsTrackerHostedService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsTrackerHostedService"/> class.
        /// </summary>
        /// <param name="gpsTrackerService">The GPS tracker protocol service.</param>
        /// <param name="fleetDashboardService">The fleet dashboard analytics service.</param>
        /// <param name="logger">The logger instance.</param>
        public GpsTrackerHostedService(
            IGpsTrackerProtocolService gpsTrackerService,
            IFleetDashboardService fleetDashboardService,
            ILogger<GpsTrackerHostedService> logger)
        {
            _gpsTrackerService = gpsTrackerService;
            _fleetDashboardService = fleetDashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Starts the GPS tracker protocol v2.0 services asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting GPS Tracker Protocol v2.0");

            // Start the GPS tracker service
            _gpsTrackerService.Start();

            // Start the fleet dashboard analytics
            _fleetDashboardService.StartAnalytics();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the GPS tracker protocol v2.0 services asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping GPS Tracker Protocol v2.0");

            // Stop services gracefully
            _gpsTrackerService.Stop();
            _fleetDashboardService.StopAnalytics();

            return Task.CompletedTask;
        }
    }
}
