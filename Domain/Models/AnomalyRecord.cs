using System;

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Represents a detected fuel‑level anomaly (e.g., possible theft) for a vehicle.
/// </summary>
public sealed class AnomalyRecord
{
    /// <summary>
    /// The timestamp of the reading that triggered the anomaly.
    /// </summary>
    /// <value>The timestamp of the reading that triggered the anomaly.</value>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Amount of fuel that disappeared, in litres.
    /// </summary>
    /// <value>Amount of fuel that disappeared, in litres.</value>
    public double DropAmountLiters { get; set; }

    /// <summary>
    /// How long the drop occurred over.
    /// </summary>
    /// <value>How long the drop occurred over.</value>
    public TimeSpan Duration { get; set; }

    public override string ToString() => $"AnomalyRecord {{ Timestamp = {Timestamp}, DropAmountLiters = {DropAmountLiters}, Duration = {Duration} }}";
}
