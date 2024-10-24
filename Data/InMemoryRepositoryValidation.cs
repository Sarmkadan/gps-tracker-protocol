#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using System.Globalization;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="InMemoryRepository{T}"/> and derived classes.
/// </summary>
public static class InMemoryRepositoryValidation
{
    /// <summary>
    /// Validates the state of an in-memory repository by checking its public interface.
    /// </summary>
    /// <param name="value">The repository to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this InMemoryRepository<LocationData> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate LocationData-specific constraints by checking all entities
        // Since we can't access protected _store directly, we use the public GetAllAsync method
        var allEntities = value.GetAllAsync().Result; // Safe for in-memory repo

        foreach (var entity in allEntities)
        {
            problems.AddRange(ValidateLocationData(entity));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the repository is in a valid state.
    /// </summary>
    /// <param name="value">The repository to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this InMemoryRepository<LocationData> value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the repository is in a valid state, throwing if not.
    /// </summary>
    /// <param name="value">The repository to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the repository is invalid.</exception>
    public static void EnsureValid(this InMemoryRepository<LocationData> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Repository is invalid. Problems: {string.Join(", ", problems)}");
        }
    }

    /// <summary>
    /// Validates a single LocationData entity.
    /// </summary>
    /// <param name="location">The location data to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="location"/> is null.</exception>
    public static IReadOnlyList<string> ValidateLocationData(LocationData? location)
    {
        if (location is null)
        {
            return ["LocationData entity is null"];
        }

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(location.Id))
        {
            problems.Add("LocationData.Id is null or whitespace");
        }
        else if (!Guid.TryParse(location.Id, out _))
        {
            problems.Add("LocationData.Id is not a valid GUID");
        }

        if (string.IsNullOrWhiteSpace(location.DeviceId))
        {
            problems.Add("LocationData.DeviceId is null or whitespace");
        }

        if (double.IsNaN(location.Latitude) || double.IsInfinity(location.Latitude))
        {
            problems.Add("LocationData.Latitude is NaN or Infinity");
        }
        else if (location.Latitude < -90.0 || location.Latitude > 90.0)
        {
            problems.Add("LocationData.Latitude is out of range [-90, 90]");
        }

        if (double.IsNaN(location.Longitude) || double.IsInfinity(location.Longitude))
        {
            problems.Add("LocationData.Longitude is NaN or Infinity");
        }
        else if (location.Longitude < -180.0 || location.Longitude > 180.0)
        {
            problems.Add("LocationData.Longitude is out of range [-180, 180]");
        }

        if (double.IsNaN(location.Altitude))
        {
            problems.Add("LocationData.Altitude is NaN");
        }

        if (double.IsNaN(location.Speed))
        {
            problems.Add("LocationData.Speed is NaN");
        }
        else if (location.Speed < 0)
        {
            problems.Add("LocationData.Speed is negative");
        }

        if (double.IsNaN(location.Bearing))
        {
            problems.Add("LocationData.Bearing is NaN");
        }
        else if (location.Bearing < 0 || location.Bearing > 360)
        {
            problems.Add("LocationData.Bearing is out of range [0, 360]");
        }

        if (location.Timestamp == default)
        {
            problems.Add("LocationData.Timestamp is default (DateTime.MinValue)");
        }
        else if (location.Timestamp > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("LocationData.Timestamp is in the future");
        }
        else if (location.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("LocationData.Timestamp is more than 1 year in the past");
        }

        if (double.IsNaN(location.Accuracy))
        {
            problems.Add("LocationData.Accuracy is NaN");
        }
        else if (location.Accuracy < 0)
        {
            problems.Add("LocationData.Accuracy is negative");
        }

        if (location.SatelliteCount < 0)
        {
            problems.Add("LocationData.SatelliteCount is negative");
        }
        else if (location.SatelliteCount > 100)
        {
            problems.Add("LocationData.SatelliteCount exceeds reasonable maximum (100)");
        }

        // ProtocolType is an enum, so it will always have a valid value
        // but we can check if it's the default Unknown value
        if (location.Protocol == ProtocolType.Unknown)
        {
            problems.Add("LocationData.Protocol is Unknown (default value)");
        }

        if (location.ExtendedData is null)
        {
            problems.Add("LocationData.ExtendedData dictionary is null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method parameters for GetByDeviceIdAsync.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParameters(string deviceId)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            problems.Add("deviceId is null or whitespace");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method parameters for GetByTimeRangeAsync.
    /// </summary>
    /// <param name="start">The start of the time range.</param>
    /// <param name="end">The end of the time range.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParameters(DateTime start, DateTime end)
    {
        var problems = new List<string>();

        if (start == default)
        {
            problems.Add("start is default (DateTime.MinValue)");
        }

        if (end == default)
        {
            problems.Add("end is default (DateTime.MinValue)");
        }
        else if (end < start)
        {
            problems.Add("end is before start");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method parameters for GetByDeviceAndTimeRangeAsync.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="start">The start of the time range.</param>
    /// <param name="end">The end of the time range.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParameters(string deviceId, DateTime start, DateTime end)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            problems.Add("deviceId is null or whitespace");
        }

        problems.AddRange(ValidateParameters(start, end));

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method parameters for GetWithinRadiusAsync.
    /// </summary>
    /// <param name="latitude">The latitude coordinate.</param>
    /// <param name="longitude">The longitude coordinate.</param>
    /// <param name="radiusKm">The radius in kilometers.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParameters(double latitude, double longitude, double radiusKm)
    {
        var problems = new List<string>();

        if (double.IsNaN(latitude) || double.IsInfinity(latitude))
        {
            problems.Add("latitude is NaN or Infinity");
        }
        else if (latitude < -90.0 || latitude > 90.0)
        {
            problems.Add("latitude is out of range [-90, 90]");
        }

        if (double.IsNaN(longitude) || double.IsInfinity(longitude))
        {
            problems.Add("longitude is NaN or Infinity");
        }
        else if (longitude < -180.0 || longitude > 180.0)
        {
            problems.Add("longitude is out of range [-180, 180]");
        }

        if (double.IsNaN(radiusKm) || double.IsInfinity(radiusKm))
        {
            problems.Add("radiusKm is NaN or Infinity");
        }
        else if (radiusKm <= 0)
        {
            problems.Add("radiusKm must be positive");
        }
        else if (radiusKm > 10000)
        {
            problems.Add("radiusKm exceeds reasonable maximum (10000 km)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method parameters for DeleteOlderThanAsync.
    /// </summary>
    /// <param name="dateTime">The cutoff date/time.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateParameters(DateTime dateTime)
    {
        var problems = new List<string>();

        if (dateTime == default)
        {
            problems.Add("dateTime is default (DateTime.MinValue)");
        }

        return problems.AsReadOnly();
    }
}