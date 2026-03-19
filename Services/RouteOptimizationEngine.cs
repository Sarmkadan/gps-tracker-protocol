// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Computes optimised visit sequences for fleet route planning.
/// Accepts a list of <see cref="RouteStop"/> waypoints and returns an
/// <see cref="OptimizedRoute"/> with sequenced stops and travel estimates.
/// </summary>
public interface IRouteOptimizationEngine
{
    /// <summary>
    /// Optimises the visit order of <paramref name="stops"/> for the given <paramref name="vehicle"/>.
    /// </summary>
    /// <param name="vehicle">Fleet vehicle whose fuel baseline and ID are embedded in the result.</param>
    /// <param name="stops">Unordered list of stops to visit. Must contain at least one entry.</param>
    /// <param name="algorithmOverride">
    /// Optional algorithm override; when null the value from <see cref="FleetDashboardOptions"/> is used.
    /// </param>
    /// <returns>An <see cref="OptimizedRoute"/> with stops in the computed visit order.</returns>
    Task<OptimizedRoute> OptimizeAsync(
        FleetVehicle vehicle,
        IReadOnlyList<RouteStop> stops,
        RouteOptimizationAlgorithm? algorithmOverride = null);
}

/// <summary>
/// Route optimisation engine implementing nearest-neighbour construction
/// with optional 2-opt local-search refinement.
/// </summary>
/// <remarks>
/// Nearest-neighbour builds an initial tour in O(n²) time, which is fast enough
/// for real-time requests up to a few hundred stops.  The 2-opt improvement pass
/// iteratively reverses sub-sequences whose swap reduces total tour length,
/// typically improving solution quality by 5–15 % over the initial construction.
/// </remarks>
public sealed class RouteOptimizationEngine : IRouteOptimizationEngine
{
    private const double EarthRadiusKm = 6371.0;

    private readonly FleetDashboardOptions _options;
    private readonly IFuelTrackingService _fuelTracking;
    private readonly ILogger<RouteOptimizationEngine> _logger;

    /// <summary>Initialises the engine with dashboard options, fuel tracking, and a logger.</summary>
    public RouteOptimizationEngine(
        FleetDashboardOptions options,
        IFuelTrackingService fuelTracking,
        ILogger<RouteOptimizationEngine> logger)
    {
        _options = options;
        _fuelTracking = fuelTracking;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<OptimizedRoute> OptimizeAsync(
        FleetVehicle vehicle,
        IReadOnlyList<RouteStop> stops,
        RouteOptimizationAlgorithm? algorithmOverride = null)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(stops);

        if (stops.Count == 0)
            throw new ArgumentException("At least one stop is required for route optimisation", nameof(stops));

        if (stops.Count > _options.MaxStopsPerRoute)
            throw new ArgumentOutOfRangeException(nameof(stops),
                $"Stop count {stops.Count} exceeds the configured maximum of {_options.MaxStopsPerRoute}");

        var algorithm = algorithmOverride ?? _options.DefaultAlgorithm;

        _logger.LogInformation(
            "Optimising {Count}-stop route for vehicle {Reg} using {Algorithm}",
            stops.Count, vehicle.RegistrationNumber, algorithm);

        var distMatrix = BuildDistanceMatrix(stops);
        var sequence = NearestNeighbor(distMatrix, stops.Count);

        if (algorithm == RouteOptimizationAlgorithm.TwoOpt)
            TwoOptImprove(sequence, distMatrix);

        var optimizedStops = BuildOptimizedStops(stops, sequence, distMatrix, _options.AverageRoadSpeedKmh);

        var totalDist = optimizedStops.LastOrDefault()?.CumulativeDistanceKm ?? 0;
        var travelMin = optimizedStops.Sum(s => s.EstimatedTravelMinutes);
        var serviceMin = stops.Sum(s => s.ServiceTimeMinutes);
        var fuelLiters = _fuelTracking.EstimateFuelLiters(totalDist, vehicle.BaseConsumptionLper100km);
        var fuelCost = Math.Round(fuelLiters * _options.DefaultFuelPricePerLiter, 4);

        _logger.LogDebug(
            "Route computed: {Dist:F1} km, {Time:F0} min drive + {Svc} min service, ~{Fuel:F1} L",
            totalDist, travelMin, serviceMin, fuelLiters);

        return Task.FromResult(new OptimizedRoute
        {
            Id = Guid.NewGuid().ToString(),
            VehicleId = vehicle.Id,
            Stops = optimizedStops,
            TotalDistanceKm = Math.Round(totalDist, 2),
            EstimatedDurationMinutes = Math.Round(travelMin + serviceMin, 1),
            EstimatedFuelLiters = fuelLiters,
            EstimatedFuelCost = fuelCost,
            ComputedAt = DateTime.UtcNow,
            Algorithm = algorithm
        });
    }

    // -------------------------------------------------------------------------
    // Nearest-neighbour construction heuristic
    // -------------------------------------------------------------------------

    private static int[] NearestNeighbor(double[,] dist, int count)
    {
        var visited = new bool[count];
        var route = new int[count];
        route[0] = 0;
        visited[0] = true;

        for (var step = 1; step < count; step++)
        {
            var current = route[step - 1];
            var nearest = -1;
            var nearestDist = double.MaxValue;

            for (var j = 0; j < count; j++)
            {
                if (!visited[j] && dist[current, j] < nearestDist)
                {
                    nearest = j;
                    nearestDist = dist[current, j];
                }
            }

            route[step] = nearest;
            visited[nearest] = true;
        }

        return route;
    }

    // -------------------------------------------------------------------------
    // 2-opt local search
    // Repeatedly reverses sub-tours when doing so reduces the total tour length.
    // -------------------------------------------------------------------------

    private static void TwoOptImprove(int[] route, double[,] dist)
    {
        var n = route.Length;
        bool improved;

        do
        {
            improved = false;
            for (var i = 1; i < n - 1; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    // Cost of current edges: (i-1→i) + (j→j+1)
                    // Cost of swapped edges: (i-1→j) + (i→j+1)
                    var next = j + 1 < n ? route[j + 1] : route[0];
                    var currentCost = dist[route[i - 1], route[i]] + dist[route[j], next];
                    var swappedCost = dist[route[i - 1], route[j]] + dist[route[i], next];

                    if (swappedCost < currentCost - 1e-10)
                    {
                        Array.Reverse(route, i, j - i + 1);
                        improved = true;
                    }
                }
            }
        } while (improved);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static double[,] BuildDistanceMatrix(IReadOnlyList<RouteStop> stops)
    {
        var n = stops.Count;
        var matrix = new double[n, n];

        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
            matrix[i, j] = i == j ? 0.0 : HaversineKm(stops[i], stops[j]);

        return matrix;
    }

    private static IReadOnlyList<OptimizedStop> BuildOptimizedStops(
        IReadOnlyList<RouteStop> stops,
        int[] sequence,
        double[,] dist,
        double avgSpeedKmh)
    {
        var result = new List<OptimizedStop>(sequence.Length);
        var cumulative = 0.0;

        for (var i = 0; i < sequence.Length; i++)
        {
            var segmentDist = i == 0 ? 0.0 : dist[sequence[i - 1], sequence[i]];
            cumulative += segmentDist;
            var travelMin = avgSpeedKmh > 0 && segmentDist > 0
                ? segmentDist / avgSpeedKmh * 60.0
                : 0.0;

            result.Add(new OptimizedStop
            {
                StopId = stops[sequence[i]].Id,
                Sequence = i,
                DistanceFromPreviousKm = Math.Round(segmentDist, 3),
                EstimatedTravelMinutes = Math.Round(travelMin, 1),
                CumulativeDistanceKm = Math.Round(cumulative, 3)
            });
        }

        return result;
    }

    private static double HaversineKm(RouteStop a, RouteStop b)
    {
        var dLat = ToRad(b.Latitude - a.Latitude);
        var dLon = ToRad(b.Longitude - a.Longitude);
        var x = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(a.Latitude)) * Math.Cos(ToRad(b.Latitude))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(x), Math.Sqrt(1 - x));
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180.0;
}
