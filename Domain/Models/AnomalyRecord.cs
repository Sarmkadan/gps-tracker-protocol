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
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Amount of fuel that disappeared, in litres.
    /// </summary>
    public double DropAmountLiters { get; set; }

    /// <summary>
    /// How long the drop occurred over.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
