#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

namespace GpsTrackerProtocol.Tests;

/// <summary>
/// Provides validation helpers for domain models and services tested in <see cref="DomainAndServiceTests"/>.
/// These extension methods allow validation of domain models (LocationData, Device, GpsFrame)
/// and services (GeofenceService) that are tested by the test methods in DomainAndServiceTests.
/// </summary>
public static class DomainAndServiceTestsValidation
{
    /// <summary>
    /// Validates a LocationData instance.
    /// </summary>
    /// <param name="value">The location data to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this LocationData? value)
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(value.DeviceId))
        {
            errors.Add("LocationData.DeviceId cannot be null or empty");
        }

        if (value.Latitude < -90 || value.Latitude > 90)
        {
            errors.Add("LocationData.Latitude must be between -90 and 90 degrees");
        }

        if (value.Longitude < -180 || value.Longitude > 180)
        {
            errors.Add("LocationData.Longitude must be between -180 and 180 degrees");
        }

        if (value.Speed < 0)
        {
            errors.Add("LocationData.Speed cannot be negative");
        }

        if (value.Bearing < 0 || value.Bearing > 360)
        {
            errors.Add("LocationData.Bearing must be between 0 and 360 degrees");
        }

        if (value.SatelliteCount < 0)
        {
            errors.Add("LocationData.SatelliteCount cannot be negative");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a Device instance.
    /// </summary>
    /// <param name="value">The device to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this Device? value)
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(value.Id))
        {
            errors.Add("Device.Id cannot be null or empty");
        }

        if (string.IsNullOrEmpty(value.Imei))
        {
            errors.Add("Device.Imei cannot be null or empty");
        }
        else if (value.Imei.Length != 15 || !value.Imei.All(char.IsDigit))
        {
            errors.Add("Device.Imei must be exactly 15 numeric digits");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a GpsFrame instance.
    /// </summary>
    /// <param name="value">The GPS frame to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this GpsFrame? value)
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(value);

        if (value.RawData == null || value.RawData.Length == 0)
        {
            errors.Add("GpsFrame.RawData cannot be null or empty");
        }
        else if (value.Protocol == ProtocolType.GT06 && value.RawData.Length < 15)
        {
            errors.Add("GT06 protocol requires at least 15 bytes of raw data");
        }

        if (!value.IsValidChecksum)
        {
            errors.Add("GpsFrame has invalid checksum");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a GeofenceService instance.
    /// </summary>
    /// <param name="value">The geofence service to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this GeofenceService? value)
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(value);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a LocationData instance is valid.
    /// </summary>
    /// <param name="value">The location data to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this LocationData? value) => !Validate(value).Any();

    /// <summary>
    /// Checks if a Device instance is valid.
    /// </summary>
    /// <param name="value">The device to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this Device? value) => !Validate(value).Any();

    /// <summary>
    /// Checks if a GpsFrame instance is valid.
    /// </summary>
    /// <param name="value">The GPS frame to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this GpsFrame? value) => !Validate(value).Any();

    /// <summary>
    /// Checks if a GeofenceService instance is valid.
    /// </summary>
    /// <param name="value">The geofence service to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this GeofenceService? value) => !Validate(value).Any();

    /// <summary>
    /// Ensures a LocationData instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The location data to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this LocationData? value)
    {
        var errors = Validate(value);
        if (errors.Any())
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }
    }

    /// <summary>
    /// Ensures a Device instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The device to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this Device? value)
    {
        var errors = Validate(value);
        if (errors.Any())
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }
    }

    /// <summary>
    /// Ensures a GpsFrame instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The GPS frame to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this GpsFrame? value)
    {
        var errors = Validate(value);
        if (errors.Any())
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }
    }

    /// <summary>
    /// Ensures a GeofenceService instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The geofence service to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this GeofenceService? value)
    {
        var errors = Validate(value);
        if (errors.Any())
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }
    }
}