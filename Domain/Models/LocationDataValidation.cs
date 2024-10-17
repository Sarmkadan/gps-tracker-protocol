#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="LocationData"/> instances.
/// </summary>
public static class LocationDataValidation
{
    /// <summary>
    /// Validates a LocationData instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The location data to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    public static IReadOnlyList<string> Validate(this LocationData value)
    {
        if (value is null)
        {
            return ["LocationData cannot be null"];
        }

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace");
        }

        // Validate DeviceId
        if (string.IsNullOrWhiteSpace(value.DeviceId))
        {
            errors.Add("DeviceId cannot be null or whitespace");
        }

        // Validate Latitude (-90 to 90 degrees)
        if (double.IsNaN(value.Latitude) || double.IsInfinity(value.Latitude))
        {
            errors.Add("Latitude must be a valid number");
        }
        else if (value.Latitude < -90 || value.Latitude > 90)
        {
            errors.Add("Latitude must be between -90 and 90 degrees");
        }

        // Validate Longitude (-180 to 180 degrees)
        if (double.IsNaN(value.Longitude) || double.IsInfinity(value.Longitude))
        {
            errors.Add("Longitude must be a valid number");
        }
        else if (value.Longitude < -180 || value.Longitude > 180)
        {
            errors.Add("Longitude must be between -180 and 180 degrees");
        }

        // Validate Altitude (reasonable range for Earth)
        if (double.IsNaN(value.Altitude) || double.IsInfinity(value.Altitude))
        {
            errors.Add("Altitude must be a valid number");
        }
        else if (value.Altitude < -10000 || value.Altitude > 10000)
        {
            errors.Add("Altitude must be between -10000 and 10000 meters");
        }

        // Validate Speed (0 to reasonable maximum for vehicles)
        if (double.IsNaN(value.Speed) || double.IsInfinity(value.Speed))
        {
            errors.Add("Speed must be a valid number");
        }
        else if (value.Speed < 0 || value.Speed > 1000)
        {
            errors.Add("Speed must be between 0 and 1000 km/h");
        }

        // Validate Bearing (0 to 360 degrees)
        if (double.IsNaN(value.Bearing) || double.IsInfinity(value.Bearing))
        {
            errors.Add("Bearing must be a valid number");
        }
        else if (value.Bearing < 0 || value.Bearing > 360)
        {
            errors.Add("Bearing must be between 0 and 360 degrees");
        }

        // Validate Timestamp (not default DateTime)
        if (value.Timestamp == default)
        {
            errors.Add("Timestamp cannot be default(DateTime)");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddHours(1))
        {
            errors.Add("Timestamp cannot be in the future");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("Timestamp cannot be more than 1 year in the past");
        }

        // Validate Accuracy (0 to reasonable maximum)
        if (double.IsNaN(value.Accuracy) || double.IsInfinity(value.Accuracy))
        {
            errors.Add("Accuracy must be a valid number");
        }
        else if (value.Accuracy < 0 || value.Accuracy > 1000)
        {
            errors.Add("Accuracy must be between 0 and 1000 meters");
        }

        // Validate SatelliteCount (0 to reasonable maximum)
        if (value.SatelliteCount < 0 || value.SatelliteCount > 100)
        {
            errors.Add("SatelliteCount must be between 0 and 100");
        }

        // Validate Protocol
        if (!Enum.IsDefined(typeof(ProtocolType), value.Protocol))
        {
            errors.Add("Protocol must be a valid ProtocolType value");
        }

        // Validate ExtendedData (not null, but can be empty)
        if (value.ExtendedData is null)
        {
            errors.Add("ExtendedData cannot be null");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a LocationData instance is valid.
    /// </summary>
    /// <param name="value">The location data to check.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(this LocationData value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures a LocationData instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The location data to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the location data is invalid.</exception>
    public static void EnsureValid(this LocationData value)
    {
        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"LocationData is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}