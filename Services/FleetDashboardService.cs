#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Represents a summary of fleet status including device counts, online/offline status,
/// average speed of moving vehicles, and active alert count.
/// </summary>
/// <param name="TotalDevices">Total number of devices in the fleet.</param>
/// <param name="OnlineDevices">Number of devices currently online.</param>
/// <param name="OfflineDevices">Number of devices currently offline.</param>
/// <param name="AverageMovingSpeedKmh">Average speed of vehicles currently in motion.</param>
/// <param name="ActiveAlerts">Number of active alerts across the fleet.</param>
public sealed record FleetSummary(
    int TotalDevices,
    int OnlineDevices,
    int OfflineDevices,
    double AverageMovingSpeedKmh,
    int ActiveAlerts);

/// <summary>
/// Manages the fleet vehicle registry and exposes a real-time analytics dashboard
/// that aggregates live telemetry from GPS devices with fuel and route data.
/// </summary>
public interface IFleetDashboardService
{
    /// <summary>
    /// Registers a new vehicle in the fleet, linking it to an existing GPS device.
    /// Throws <see cref="DeviceException"/> when the referenced device does not exist.
    /// </summary>
    Task<FleetVehicle> RegisterVehicleAsync(FleetVehicle vehicle);

    /// <summary>Returns a fleet vehicle by its internal ID; null when not found.</summary>
    Task<FleetVehicle?> GetVehicleAsync(string vehicleId);

    /// <summary>Returns all registered fleet vehicles ordered by registration number.</summary>
    Task<IEnumerable<FleetVehicle>> GetAllVehiclesAsync();

    /// <summary>
    /// Replaces the stored vehicle record with the provided one.
    /// Throws <see cref="KeyNotFoundException"/> when the vehicle ID is unknown.
    /// </summary>
    Task<FleetVehicle> UpdateVehicleAsync(FleetVehicle vehicle);

    /// <summary>Removes a vehicle from the fleet registry. Returns false when not found.</summary>
    Task<bool> RemoveVehicleAsync(string vehicleId);

    /// <summary>
    /// Generates a real-time dashboard snapshot by querying live device telemetry,
    /// location history, and fuel records for all registered vehicles.
    /// </summary>
    Task<FleetDashboardSnapshot> GetDashboardSnapshotAsync();

    /// <summary>Returns current status and today's performance metrics for a single vehicle.</summary>
    Task<VehicleStatusSummary> GetVehicleStatusAsync(string vehicleId);

    /// <summary>
    /// Computes an optimised stop sequence for a vehicle, delegating to
    /// <see cref="IRouteOptimizationEngine"/> with the configured algorithm.
    /// </summary>
    Task<OptimizedRoute> OptimizeRouteAsync(
        string vehicleId,
        IReadOnlyList<RouteStop> stops,
        RouteOptimizationAlgorithm? algorithmOverride = null);

    /// <summary>
    /// Computes named fleet-wide KPI metrics over an arbitrary time window.
    /// </summary>
    Task<IReadOnlyDictionary<string, double>> ComputeFleetKpisAsync(DateTime from, DateTime to);

    /// <summary>
    /// Returns a summary of the fleet status including device counts, online/offline status,
    /// average speed of moving vehicles, and active alert count.
    /// </summary>
    Task<FleetSummary> GetFleetSummaryAsync();
}

/// <summary>
/// Production implementation of <see cref="IFleetDashboardService"/>.
/// Stores fleet vehicles in a thread-safe in-memory dictionary and delegates
/// telemetry queries to the core GPS tracker service layer.
/// </summary>
public sealed class FleetDashboardService : IFleetDashboardService
{
    private readonly ConcurrentDictionary<string, FleetVehicle> _vehicles = new();

    private readonly IDeviceService _deviceService;
    private readonly ILocationDataService _locationService;
    private readonly IFuelTrackingService _fuelTracking;
    private readonly IRouteOptimizationEngine _routeEngine;
    private readonly IGeofenceAlertingService _alertingService;
    private readonly FleetDashboardOptions _options;
    private readonly ILogger<FleetDashboardService> _logger;

    /// <summary>
    /// Initialises the service with all required dependencies.
    /// Call <see cref="FleetDashboardExtensions.AddFleetAnalyticsDashboard"/> to wire up via DI.
    /// </summary>
    public FleetDashboardService(
        IDeviceService deviceService,
        ILocationDataService locationService,
        IFuelTrackingService fuelTracking,
        IRouteOptimizationEngine routeEngine,
        IGeofenceAlertingService alertingService,
        FleetDashboardOptions options,
        ILogger<FleetDashboardService> logger)
    {
        _deviceService = deviceService;
        _locationService = locationService;
        _fuelTracking = fuelTracking;
        _routeEngine = routeEngine;
        _alertingService = alertingService;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<FleetVehicle> RegisterVehicleAsync(FleetVehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (string.IsNullOrWhiteSpace(vehicle.DeviceId))
            throw new ArgumentException("Vehicle must reference a valid device ID", nameof(vehicle));

        if (string.IsNullOrWhiteSpace(vehicle.RegistrationNumber))
            throw new ArgumentException("Registration number is required", nameof(vehicle));

        if (_options.MaxFleetSize > 0 && _vehicles.Count >= _options.MaxFleetSize)
            throw new InvalidOperationException(
                $"Fleet has reached the configured maximum of {_options.MaxFleetSize} vehicles");

        var device = await _deviceService.GetDeviceAsync(vehicle.DeviceId).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException(
                $"Device '{vehicle.DeviceId}' does not exist — vehicle cannot be registered without a linked device",
                vehicle.DeviceId);

        if (_vehicles.Values.Any(v =>
            v.RegistrationNumber.Equals(vehicle.RegistrationNumber, StringComparison.OrdinalIgnoreCase)))
            throw new ValidationException(
                $"A vehicle with registration '{vehicle.RegistrationNumber}' is already registered",
                nameof(FleetVehicle.RegistrationNumber),
                vehicle.RegistrationNumber);

        var registered = vehicle with
        {
            Id = string.IsNullOrWhiteSpace(vehicle.Id) ? Guid.NewGuid().ToString() : vehicle.Id,
            RegisteredAt = DateTime.UtcNow
        };

        _vehicles[registered.Id] = registered;

        _logger.LogInformation(
            "Fleet vehicle '{Reg}' registered (ID: {Id}, device: {DeviceId}, fuel: {Fuel})",
            registered.RegistrationNumber, registered.Id, registered.DeviceId, registered.FuelType);

        return registered;
    }

    /// <inheritdoc/>
    public Task<FleetVehicle?> GetVehicleAsync(string vehicleId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("Vehicle ID cannot be empty", nameof(vehicleId));

        _vehicles.TryGetValue(vehicleId, out var vehicle);
        return Task.FromResult(vehicle);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<FleetVehicle>> GetAllVehiclesAsync() =>
        Task.FromResult<IEnumerable<FleetVehicle>>(
            _vehicles.Values.OrderBy(v => v.RegistrationNumber).ToList());

    /// <inheritdoc/>
    public Task<FleetVehicle> UpdateVehicleAsync(FleetVehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (!_vehicles.ContainsKey(vehicle.Id))
            throw new KeyNotFoundException($"Vehicle '{vehicle.Id}' not found in the fleet registry");

        _vehicles[vehicle.Id] = vehicle;
        return Task.FromResult(vehicle);
    }

    /// <inheritdoc/>
    public Task<bool> RemoveVehicleAsync(string vehicleId)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("Vehicle ID cannot be empty", nameof(vehicleId));

        var removed = _vehicles.TryRemove(vehicleId, out _);
        if (removed)
            _logger.LogInformation("Fleet vehicle '{VehicleId}' removed from registry", vehicleId);

        return Task.FromResult(removed);
    }

    /// <inheritdoc/>
    public async Task<FleetDashboardSnapshot> GetDashboardSnapshotAsync()
    {
        var vehicles = _vehicles.Values.OrderBy(v => v.RegistrationNumber).ToList();

        if (vehicles.Count == 0)
            return BuildEmptySnapshot();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Fan out all per-vehicle queries concurrently to minimise latency.
        var statusTasks = vehicles.Select(v => BuildVehicleStatusAsync(v, today, tomorrow));
        var reportTasks = vehicles.Select(v => _fuelTracking.GetReportAsync(v.Id, today, tomorrow));

        var statuses = await Task.WhenAll(statusTasks).ConfigureAwait(false);
        var fuelReports = await Task.WhenAll(reportTasks).ConfigureAwait(false);

        var totalFuel = fuelReports.Sum(r => r.TotalFuelConsumedLiters);
        var totalDist = statuses.Sum(s => s.TodayDistanceKm);
        var avgEfficiency = totalDist > 0 ? Math.Round(totalFuel / totalDist * 100.0, 2) : 0.0;

        return new FleetDashboardSnapshot
        {
            GeneratedAt = DateTime.UtcNow,
            TotalVehicles = vehicles.Count,
            ActiveVehicles = statuses.Count(s => s.Status is
                DeviceStatus.Online or DeviceStatus.Moving or DeviceStatus.Idle or DeviceStatus.Parked),
            VehiclesInMotion = statuses.Count(s => s.Status == DeviceStatus.Moving),
            TotalFleetDistanceKm = Math.Round(totalDist, 2),
            TotalFuelConsumedLiters = Math.Round(totalFuel, 3),
            AverageFleetEfficiencyLper100km = avgEfficiency,
            VehicleSummaries = statuses,
            FuelReports = fuelReports,
            KpiMetrics = BuildKpiDictionary(statuses, fuelReports, totalDist, totalFuel, vehicles.Count)
        };
    }

    /// <inheritdoc/>
    public async Task<VehicleStatusSummary> GetVehicleStatusAsync(string vehicleId)
    {
        if (!_vehicles.TryGetValue(vehicleId, out var vehicle))
            throw new KeyNotFoundException($"Vehicle '{vehicleId}' not found in the fleet registry");

        var today = DateTime.UtcNow.Date;
        return await BuildVehicleStatusAsync(vehicle, today, today.AddDays(1)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<OptimizedRoute> OptimizeRouteAsync(
        string vehicleId,
        IReadOnlyList<RouteStop> stops,
        RouteOptimizationAlgorithm? algorithmOverride = null)
    {
        if (!_vehicles.TryGetValue(vehicleId, out var vehicle))
            throw new KeyNotFoundException($"Vehicle '{vehicleId}' not found in the fleet registry");

        return await _routeEngine.OptimizeAsync(vehicle, stops, algorithmOverride).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, double>> ComputeFleetKpisAsync(DateTime from, DateTime to)
    {
        if (to <= from)
            throw new ArgumentException("'to' must be strictly after 'from'");

        var vehicles = _vehicles.Values.ToList();
        if (vehicles.Count == 0)
            return new Dictionary<string, double>();

        var reports = await Task.WhenAll(vehicles.Select(v => _fuelTracking.GetReportAsync(v.Id, from, to))).ConfigureAwait(false);
        var totalDist = reports.Sum(r => r.TotalDistanceKm);
        var totalFuel = reports.Sum(r => r.TotalFuelConsumedLiters);

        return BuildKpiDictionary([], reports, totalDist, totalFuel, vehicles.Count);
    }

    /// <inheritdoc/>
    public async Task<FleetSummary> GetFleetSummaryAsync()
    {
        var vehicles = _vehicles.Values.ToList();
        if (vehicles.Count == 0)
        {
            return new FleetSummary(
                TotalDevices: 0,
                OnlineDevices: 0,
                OfflineDevices: 0,
                AverageMovingSpeedKmh: 0,
                ActiveAlerts: 0);
        }

        // Get device statuses to determine online/offline counts
        var deviceTasks = vehicles.Select(v => _deviceService.GetDeviceAsync(v.DeviceId));
        var devices = await Task.WhenAll(deviceTasks).ConfigureAwait(false);

        // Calculate online/offline counts based on last-seen threshold
        var onlineDevices = 0;
        var offlineDevices = 0;
        var totalSpeed = 0.0;
        var movingCount = 0;

        foreach (var (vehicle, device) in vehicles.Zip(devices))
        {
            if (device is null)
            {
                offlineDevices++;
                continue;
            }

            // Check if device is online based on last-seen threshold
            var isOnline = device.LastSeen >= DateTime.UtcNow.AddMinutes(-_options.SnapshotCacheTtl.TotalMinutes);
            if (isOnline)
            {
                onlineDevices++;
            }
            else
            {
                offlineDevices++;
            }

            // Get vehicle status to check if moving and get speed
            var status = await GetVehicleStatusAsync(vehicle.Id).ConfigureAwait(false);
            if (status.Status == DeviceStatus.Moving && status.CurrentSpeedKmh > 0)
            {
                totalSpeed += status.CurrentSpeedKmh.Value;
                movingCount++;
            }
        }

        // Calculate average speed of moving vehicles
        var averageMovingSpeed = movingCount > 0 ? totalSpeed / movingCount : 0;

        // Get active alerts count across all vehicles
        var activeAlerts = 0;
        foreach (var vehicle in vehicles)
        {
            var alerts = _alertingService.GetActiveAlerts(vehicle.DeviceId);
            activeAlerts += alerts.Count;
        }

        return new FleetSummary(
            TotalDevices: vehicles.Count,
            OnlineDevices: onlineDevices,
            OfflineDevices: offlineDevices,
            AverageMovingSpeedKmh: Math.Round(averageMovingSpeed, 2),
            ActiveAlerts: activeAlerts);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<VehicleStatusSummary> BuildVehicleStatusAsync(
        FleetVehicle vehicle, DateTime since, DateTime until)
    {
        var deviceTask = _deviceService.GetDeviceAsync(vehicle.DeviceId);
        var latestTask = _locationService.GetLatestLocationAsync(vehicle.DeviceId);
        var distanceTask = _locationService.CalculateTravelDistanceAsync(vehicle.DeviceId, since, until);
        var fuelTask = _fuelTracking.GetReportAsync(vehicle.Id, since, until);

        await Task.WhenAll(deviceTask, latestTask, distanceTask, fuelTask).ConfigureAwait(false);

        var device = deviceTask.Result;
        var latest = latestTask.Result;
        var todayDist = distanceTask.Result;
        var fuelReport = fuelTask.Result;

        // Prefer explicit fuel records; fall back to distance-based estimate when enabled.
        var todayFuel = fuelReport.TotalFuelConsumedLiters > 0
            ? fuelReport.TotalFuelConsumedLiters
            : _options.EnableDistanceBasedFallback
                ? _fuelTracking.EstimateFuelLiters(todayDist, vehicle.BaseConsumptionLper100km)
                : 0.0;

        var tankRemaining = vehicle.TankCapacityLiters > 0
            ? Math.Max(0.0, vehicle.TankCapacityLiters - todayFuel)
            : 0.0;

        return new VehicleStatusSummary
        {
            VehicleId = vehicle.Id,
            DeviceId = vehicle.DeviceId,
            RegistrationNumber = vehicle.RegistrationNumber,
            Status = device?.Status ?? DeviceStatus.Unknown,
            CurrentFuelEstimateLiters = Math.Round(tankRemaining, 2),
            TodayDistanceKm = Math.Round(todayDist, 2),
            TodayFuelConsumedLiters = Math.Round(todayFuel, 3),
            LastSeenAt = device?.LastSeen ?? DateTime.MinValue,
            CurrentLatitude = latest?.Latitude,
            CurrentLongitude = latest?.Longitude,
            CurrentSpeedKmh = latest?.Speed
        };
    }

    private static IReadOnlyDictionary<string, double> BuildKpiDictionary(
        IReadOnlyList<VehicleStatusSummary> statuses,
        IEnumerable<FuelConsumptionReport> reports,
        double totalDist,
        double totalFuel,
        int vehicleCount)
    {
        var reportList = reports.ToList();
        return new Dictionary<string, double>
        {
            ["fleet.vehicle_count"] = vehicleCount,
            ["fleet.vehicles_in_motion"] = statuses.Count(s => s.Status == DeviceStatus.Moving),
            ["fleet.total_distance_km"] = Math.Round(totalDist, 2),
            ["fleet.total_fuel_liters"] = Math.Round(totalFuel, 3),
            ["fleet.total_fuel_cost"] = Math.Round(reportList.Sum(r => r.TotalCost), 4),
            ["fleet.avg_consumption_l_per_100km"] = totalDist > 0
                ? Math.Round(totalFuel / totalDist * 100.0, 2)
                : 0.0,
            ["fleet.refuel_events"] = reportList.Sum(r => r.RefuelCount),
            ["fleet.active_vehicles"] = statuses.Count(s => s.Status is
                DeviceStatus.Online or DeviceStatus.Moving or DeviceStatus.Idle or DeviceStatus.Parked)
        };
    }

    private static FleetDashboardSnapshot BuildEmptySnapshot() => new()
    {
        GeneratedAt = DateTime.UtcNow,
        TotalVehicles = 0,
        ActiveVehicles = 0,
        VehiclesInMotion = 0,
        TotalFleetDistanceKm = 0,
        TotalFuelConsumedLiters = 0,
        AverageFleetEfficiencyLper100km = 0,
        VehicleSummaries = [],
        FuelReports = [],
        KpiMetrics = new Dictionary<string, double>()
    };
}