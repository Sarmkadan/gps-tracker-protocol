// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Fuel types supported for fleet vehicle classification and consumption modelling.
/// </summary>
public enum FuelType
{
    /// <summary>Petrol (gasoline) internal combustion engine.</summary>
    Petrol = 1,
    /// <summary>Diesel compression-ignition engine.</summary>
    Diesel = 2,
    /// <summary>Battery electric vehicle — no fuel records expected.</summary>
    Electric = 3,
    /// <summary>Parallel or series hybrid powertrain.</summary>
    Hybrid = 4,
    /// <summary>Liquefied petroleum gas (LPG / autogas).</summary>
    LPG = 5,
    /// <summary>Compressed natural gas.</summary>
    CNG = 6
}

/// <summary>
/// Categories of fuel events that can be recorded against a fleet vehicle.
/// </summary>
public enum FuelEventType
{
    /// <summary>Fuel consumed during a journey segment.</summary>
    Consumption = 1,
    /// <summary>Fuel added at a filling station or depot.</summary>
    Refuel = 2,
    /// <summary>Fuel removed from the tank (maintenance procedure or theft investigation).</summary>
    Drain = 3
}

/// <summary>
/// Algorithm used when computing an optimized stop sequence.
/// </summary>
public enum RouteOptimizationAlgorithm
{
    /// <summary>Greedy nearest-neighbour construction. O(n²), suitable for real-time requests.</summary>
    NearestNeighbor = 1,
    /// <summary>Nearest-neighbour seed followed by 2-opt local search. Better tour quality at moderate cost.</summary>
    TwoOpt = 2
}

/// <summary>
/// A fleet vehicle linked to a GPS tracking device.
/// Extends device telemetry with vehicle-specific fuel and specification metadata.
/// </summary>
public record FleetVehicle
{
    /// <summary>Unique identifier for this fleet vehicle record.</summary>
    public required string Id { get; init; }

    /// <summary>ID of the GPS tracking device installed in this vehicle.</summary>
    public required string DeviceId { get; init; }

    /// <summary>Official vehicle registration (licence plate) number.</summary>
    public required string RegistrationNumber { get; init; }

    /// <summary>Vehicle manufacturer name (e.g. "Ford", "Volvo").</summary>
    public required string Make { get; init; }

    /// <summary>Vehicle model designation (e.g. "Transit", "FH16").</summary>
    public required string Model { get; init; }

    /// <summary>Four-digit model year.</summary>
    public int Year { get; init; }

    /// <summary>Primary fuel type, used to select the consumption model.</summary>
    public FuelType FuelType { get; init; } = FuelType.Diesel;

    /// <summary>Maximum tank capacity in litres. Zero for electric vehicles.</summary>
    public double TankCapacityLiters { get; init; }

    /// <summary>
    /// Manufacturer or measured baseline consumption in L/100 km.
    /// Applied when live fuel telemetry is unavailable to estimate consumption from driven distance.
    /// </summary>
    public double BaseConsumptionLper100km { get; init; } = 8.0;

    /// <summary>UTC timestamp when this vehicle was added to the fleet registry.</summary>
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Arbitrary key-value pairs for extended vehicle attributes such as driver ID, cost centre, or group tag.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}

/// <summary>
/// A single fuel event (consumption, refuel, or drain) recorded against a fleet vehicle.
/// </summary>
public record FuelRecord
{
    /// <summary>Unique identifier for this record.</summary>
    public required string Id { get; init; }

    /// <summary>Fleet vehicle this event belongs to.</summary>
    public required string VehicleId { get; init; }

    /// <summary>GPS device associated with the vehicle at recording time.</summary>
    public required string DeviceId { get; init; }

    /// <summary>Nature of this fuel event.</summary>
    public FuelEventType EventType { get; init; }

    /// <summary>Volume in litres that was consumed, added, or removed.</summary>
    public double FuelAmountLiters { get; init; }

    /// <summary>Odometer reading in kilometres at the moment of this event.</summary>
    public double OdometerKm { get; init; }

    /// <summary>Unit fuel price in local currency; null when unknown.</summary>
    public double? CostPerLiter { get; init; }

    /// <summary>UTC timestamp of the event.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>Journey during which this consumption event occurred, if applicable.</summary>
    public string? JourneyId { get; init; }

    /// <summary>Latitude of the event location (e.g. a filling station). Null when unavailable.</summary>
    public double? Latitude { get; init; }

    /// <summary>Longitude of the event location. Null when unavailable.</summary>
    public double? Longitude { get; init; }

    /// <summary>Computed total cost of this event; null when unit price is unknown.</summary>
    public double? TotalCost => CostPerLiter.HasValue ? Math.Round(FuelAmountLiters * CostPerLiter.Value, 4) : null;
}

/// <summary>
/// A geographic stop in a route optimisation request, with optional time-window constraints.
/// </summary>
public record RouteStop
{
    /// <summary>Client-provided identifier used to correlate results back to the original stop list.</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable label for display and export purposes.</summary>
    public required string Label { get; init; }

    /// <summary>Decimal-degrees latitude of the stop.</summary>
    public required double Latitude { get; init; }

    /// <summary>Decimal-degrees longitude of the stop.</summary>
    public required double Longitude { get; init; }

    /// <summary>Time in minutes required to service this stop (loading, unloading, customer visit).</summary>
    public int ServiceTimeMinutes { get; init; }

    /// <summary>Earliest permitted arrival at this stop. Null means unconstrained.</summary>
    public TimeOnly? EarliestArrival { get; init; }

    /// <summary>Latest permitted arrival at this stop. Null means unconstrained.</summary>
    public TimeOnly? LatestArrival { get; init; }
}

/// <summary>
/// A sequenced stop within a computed optimised route, including per-segment travel estimates.
/// </summary>
public record OptimizedStop
{
    /// <summary>Reference to the original <see cref="RouteStop.Id"/>.</summary>
    public required string StopId { get; init; }

    /// <summary>Zero-based position in the optimised visit sequence.</summary>
    public required int Sequence { get; init; }

    /// <summary>Great-circle distance from the previous stop in kilometres.</summary>
    public required double DistanceFromPreviousKm { get; init; }

    /// <summary>Estimated road travel time from the previous stop in minutes.</summary>
    public required double EstimatedTravelMinutes { get; init; }

    /// <summary>Running total distance from the route origin to this stop in kilometres.</summary>
    public required double CumulativeDistanceKm { get; init; }
}

/// <summary>
/// Full result of a route optimisation computation, ready for driver dispatch or map display.
/// </summary>
public record OptimizedRoute
{
    /// <summary>Unique identifier for this route plan.</summary>
    public required string Id { get; init; }

    /// <summary>Fleet vehicle this route is planned for.</summary>
    public required string VehicleId { get; init; }

    /// <summary>Stops listed in the optimised visit order.</summary>
    public required IReadOnlyList<OptimizedStop> Stops { get; init; }

    /// <summary>Sum of all inter-stop distances in kilometres.</summary>
    public required double TotalDistanceKm { get; init; }

    /// <summary>Estimated total driving time in minutes, excluding stop service times.</summary>
    public required double EstimatedDurationMinutes { get; init; }

    /// <summary>Estimated fuel consumption for this route in litres.</summary>
    public required double EstimatedFuelLiters { get; init; }

    /// <summary>Estimated fuel cost for this route in local currency.</summary>
    public required double EstimatedFuelCost { get; init; }

    /// <summary>UTC timestamp when this optimisation was computed.</summary>
    public required DateTime ComputedAt { get; init; }

    /// <summary>Algorithm that produced this result.</summary>
    public required RouteOptimizationAlgorithm Algorithm { get; init; }
}

/// <summary>
/// Aggregated fuel consumption report for a fleet vehicle over a defined time period.
/// </summary>
public record FuelConsumptionReport
{
    /// <summary>Fleet vehicle this report covers.</summary>
    public required string VehicleId { get; init; }

    /// <summary>Inclusive UTC start of the reporting window.</summary>
    public required DateTime PeriodStart { get; init; }

    /// <summary>Exclusive UTC end of the reporting window.</summary>
    public required DateTime PeriodEnd { get; init; }

    /// <summary>Total fuel consumed (L) during the window.</summary>
    public required double TotalFuelConsumedLiters { get; init; }

    /// <summary>Total distance driven (km) during the window.</summary>
    public required double TotalDistanceKm { get; init; }

    /// <summary>Mean consumption rate (L/100 km); zero when no distance was covered.</summary>
    public required double AverageConsumptionLper100km { get; init; }

    /// <summary>Sum of all costed fuel events in local currency.</summary>
    public required double TotalCost { get; init; }

    /// <summary>Number of refuelling events within the window.</summary>
    public required int RefuelCount { get; init; }

    /// <summary>Individual records contributing to this aggregation.</summary>
    public required IReadOnlyList<FuelRecord> Records { get; init; }
}

/// <summary>
/// Current operational status and today's performance metrics for a single fleet vehicle.
/// </summary>
public record VehicleStatusSummary
{
    /// <summary>Fleet vehicle identifier.</summary>
    public required string VehicleId { get; init; }

    /// <summary>Linked GPS device identifier.</summary>
    public required string DeviceId { get; init; }

    /// <summary>Vehicle registration number, for display and reporting.</summary>
    public required string RegistrationNumber { get; init; }

    /// <summary>Current operational status reported by the linked GPS device.</summary>
    public required DeviceStatus Status { get; init; }

    /// <summary>Estimated remaining fuel in litres, derived from tank capacity minus today's consumption.</summary>
    public required double CurrentFuelEstimateLiters { get; init; }

    /// <summary>Distance driven so far today in kilometres.</summary>
    public required double TodayDistanceKm { get; init; }

    /// <summary>Fuel consumed so far today in litres.</summary>
    public required double TodayFuelConsumedLiters { get; init; }

    /// <summary>UTC timestamp of the most recent device heartbeat or location fix.</summary>
    public required DateTime LastSeenAt { get; init; }

    /// <summary>Most recent GPS latitude; null when no fix is available.</summary>
    public double? CurrentLatitude { get; init; }

    /// <summary>Most recent GPS longitude; null when no fix is available.</summary>
    public double? CurrentLongitude { get; init; }

    /// <summary>Speed reported at the last GPS fix in km/h; null when unavailable.</summary>
    public double? CurrentSpeedKmh { get; init; }
}

/// <summary>
/// Point-in-time snapshot of the entire fleet, aggregated for dashboard display.
/// </summary>
public record FleetDashboardSnapshot
{
    /// <summary>UTC timestamp when this snapshot was generated.</summary>
    public required DateTime GeneratedAt { get; init; }

    /// <summary>Total number of vehicles registered in the fleet.</summary>
    public required int TotalVehicles { get; init; }

    /// <summary>Vehicles currently in an online, idle, moving, or parked state.</summary>
    public required int ActiveVehicles { get; init; }

    /// <summary>Vehicles actively reporting a <see cref="DeviceStatus.Moving"/> status.</summary>
    public required int VehiclesInMotion { get; init; }

    /// <summary>Aggregate distance driven by the whole fleet today in kilometres.</summary>
    public required double TotalFleetDistanceKm { get; init; }

    /// <summary>Total fuel consumed across the fleet today in litres.</summary>
    public required double TotalFuelConsumedLiters { get; init; }

    /// <summary>Fleet-wide average fuel efficiency in L/100 km; zero when no distance has been reported.</summary>
    public required double AverageFleetEfficiencyLper100km { get; init; }

    /// <summary>Per-vehicle status summaries, ordered by registration number.</summary>
    public required IReadOnlyList<VehicleStatusSummary> VehicleSummaries { get; init; }

    /// <summary>Per-vehicle fuel consumption reports for today's window.</summary>
    public required IReadOnlyList<FuelConsumptionReport> FuelReports { get; init; }

    /// <summary>Named KPI metrics computed for this snapshot period.</summary>
    public IReadOnlyDictionary<string, double> KpiMetrics { get; init; } = new Dictionary<string, double>();
}
