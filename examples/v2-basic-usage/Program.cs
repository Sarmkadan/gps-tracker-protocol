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
    public class GpsTrackerHostedService : IHostedService
    {
        private readonly IGpsTrackerProtocolService _gpsTrackerService;
        private readonly IFleetDashboardService _fleetDashboardService;
        private readonly ILogger<GpsTrackerHostedService> _logger;

        public GpsTrackerHostedService(
            IGpsTrackerProtocolService gpsTrackerService,
            IFleetDashboardService fleetDashboardService,
            ILogger<GpsTrackerHostedService> logger)
        {
            _gpsTrackerService = gpsTrackerService;
            _fleetDashboardService = fleetDashboardService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting GPS Tracker Protocol v2.0");

            // Start the GPS tracker service
            _gpsTrackerService.Start();

            // Start the fleet dashboard analytics
            _fleetDashboardService.StartAnalytics();

            return Task.CompletedTask;
        }

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