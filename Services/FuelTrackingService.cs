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

/// <summary>
/// Records and reports fuel events for fleet vehicles.
/// Supports explicit event recording as well as distance-based consumption estimation.
/// </summary>
public interface IFuelTrackingService
{
    /// <summary>
    /// Persists a fuel event (consumption, refuel, or drain) for a fleet vehicle.
    /// </summary>
    Task<FuelRecord> RecordFuelEventAsync(FuelRecord record);

    /// <summary>
    /// Returns all fuel records for a vehicle, optionally filtered by event type,
    /// ordered by descending timestamp.
    /// </summary>
    Task<IEnumerable<FuelRecord>> GetRecordsAsync(string vehicleId, FuelEventType? type = null);

    /// <summary>Removes a specific fuel record by its ID.</summary>
    Task<bool> DeleteRecordAsync(string recordId);

    /// <summary>
    /// Builds an aggregated fuel consumption report for a vehicle over an explicit time window.
    /// Distance is derived from odometer progression across consumption records within the window.
    /// </summary>
    Task<FuelConsumptionReport> GetReportAsync(string vehicleId, DateTime periodStart, DateTime periodEnd);

    /// <summary>
    /// Estimates fuel consumption in litres for a given distance and consumption rate.
    /// Pure calculation — does not create any records.
    /// </summary>
    double EstimateFuelLiters(double distanceKm, double consumptionLper100km);
}

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IFuelTrackingService"/>.
/// Suitable for single-node deployments and integration testing.
/// </summary>
public sealed class FuelTrackingService : IFuelTrackingService
{
    private readonly ConcurrentDictionary<string, FuelRecord> _records = new();
    private readonly ILogger<FuelTrackingService> _logger;

    /// <summary>Initialises the service with the provided logger.</summary>
    public FuelTrackingService(ILogger<FuelTrackingService> logger) => _logger = logger;

    /// <inheritdoc/>
    public Task<FuelRecord> RecordFuelEventAsync(FuelRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (string.IsNullOrWhiteSpace(record.VehicleId))
            throw new ArgumentException("Vehicle ID is required on a fuel record", nameof(record));

        if (string.IsNullOrWhiteSpace(record.DeviceId))
            throw new ArgumentException("Device ID is required on a fuel record", nameof(record));

        if (record.FuelAmountLiters <= 0)
            throw new ValidationException(
                "Fuel amount must be greater than zero",
                nameof(FuelRecord.FuelAmountLiters),
                record.FuelAmountLiters);

        var stored = record with
        {
            Id = string.IsNullOrWhiteSpace(record.Id) ? Guid.NewGuid().ToString() : record.Id
        };

        _records[stored.Id] = stored;

        _logger.LogDebug(
            "Fuel event {Type} recorded for vehicle {VehicleId}: {Liters:F2} L at {Time:O}",
            stored.EventType, stored.VehicleId, stored.FuelAmountLiters, stored.Timestamp);

        return Task.FromResult(stored);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<FuelRecord>> GetRecordsAsync(string vehicleId, FuelEventType? type = null)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("Vehicle ID cannot be empty", nameof(vehicleId));

        var results = _records.Values
            .Where(r => r.VehicleId == vehicleId)
            .Where(r => type is null || r.EventType == type)
            .OrderByDescending(r => r.Timestamp)
            .ToList();

        return Task.FromResult<IEnumerable<FuelRecord>>(results);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteRecordAsync(string recordId)
    {
        if (string.IsNullOrWhiteSpace(recordId))
            throw new ArgumentException("Record ID cannot be empty", nameof(recordId));

        return Task.FromResult(_records.TryRemove(recordId, out _));
    }

    /// <inheritdoc/>
    public Task<FuelConsumptionReport> GetReportAsync(
        string vehicleId, DateTime periodStart, DateTime periodEnd)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("Vehicle ID cannot be empty", nameof(vehicleId));

        if (periodEnd <= periodStart)
            throw new ArgumentException("Period end must be strictly after period start");

        var inPeriod = _records.Values
            .Where(r => r.VehicleId == vehicleId
                        && r.Timestamp >= periodStart
                        && r.Timestamp < periodEnd)
            .OrderBy(r => r.Timestamp)
            .ToList();

        // Sum consumption and drain events; refuels are excluded from "consumed" totals.
        var consumed = inPeriod
            .Where(r => r.EventType is FuelEventType.Consumption or FuelEventType.Drain)
            .Sum(r => r.FuelAmountLiters);

        // Derive distance from the odometer progression across all records that carry a reading.
        var odoReadings = inPeriod
            .Where(r => r.OdometerKm > 0)
            .Select(r => r.OdometerKm)
            .OrderBy(v => v)
            .ToList();

        var totalDistance = odoReadings.Count >= 2
            ? odoReadings[^1] - odoReadings[0]
            : 0.0;

        var avgConsumption = totalDistance > 0 ? consumed / totalDistance * 100.0 : 0.0;
        var totalCost = inPeriod.Sum(r => r.TotalCost ?? 0.0);
        var refuelCount = inPeriod.Count(r => r.EventType == FuelEventType.Refuel);

        return Task.FromResult(new FuelConsumptionReport
        {
            VehicleId = vehicleId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalFuelConsumedLiters = Math.Round(consumed, 3),
            TotalDistanceKm = Math.Round(totalDistance, 2),
            AverageConsumptionLper100km = Math.Round(avgConsumption, 2),
            TotalCost = Math.Round(totalCost, 4),
            RefuelCount = refuelCount,
            Records = inPeriod
        });
    }

    /// <inheritdoc/>
    public double EstimateFuelLiters(double distanceKm, double consumptionLper100km)
    {
        if (distanceKm <= 0 || consumptionLper100km <= 0)
            return 0.0;

        return Math.Round(distanceKm * consumptionLper100km / 100.0, 3);
    }
}
