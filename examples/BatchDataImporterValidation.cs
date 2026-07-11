#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Validation helpers for BatchDataImporter to ensure data integrity before import operations.
/// </summary>
public static class BatchDataImporterValidation
{
    /// <summary>
    /// Validates a file path for import operations.
    /// </summary>
    /// <param name="filePath">The file path to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            errors.Add("File path cannot be null or empty.");
            return errors;
        }

        if (!File.Exists(filePath))
        {
            errors.Add($"File does not exist: {filePath}");
        }

        return errors;
    }

    /// <summary>
    /// Validates CSV location data line parameters.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="speed">Speed value</param>
    /// <param name="timestamp">Timestamp of location</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static IReadOnlyList<string> Validate(
        string deviceId,
        double latitude,
        double longitude,
        double speed,
        DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        var errors = new List<string>();

        if (latitude < -90.0 || latitude > 90.0)
        {
            errors.Add("Latitude must be between -90.0 and 90.0 degrees.");
        }

        if (longitude < -180.0 || longitude > 180.0)
        {
            errors.Add("Longitude must be between -180.0 and 180.0 degrees.");
        }

        if (speed < 0.0)
        {
            errors.Add("Speed cannot be negative.");
        }

        if (timestamp == default)
        {
            errors.Add("Timestamp cannot be default (Unix epoch).");
        }
        else
        {
            var now = DateTime.UtcNow;
            if (timestamp > now.AddHours(1))
            {
                errors.Add("Timestamp cannot be in the future.");
            }

            if (timestamp < now.AddYears(-1))
            {
                errors.Add("Timestamp cannot be more than 1 year in the past.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates device import parameters.
    /// </summary>
    /// <param name="imei">Device IMEI number</param>
    /// <param name="deviceName">Device display name</param>
    /// <param name="protocol">Communication protocol</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="imei"/> or <paramref name="deviceName"/> or <paramref name="protocol"/> is null.</exception>
    public static IReadOnlyList<string> Validate(
        string imei,
        string deviceName,
        string protocol)
    {
        ArgumentNullException.ThrowIfNull(imei);
        ArgumentNullException.ThrowIfNull(deviceName);
        ArgumentNullException.ThrowIfNull(protocol);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(imei))
        {
            errors.Add("IMEI cannot be null or empty.");
        }
        else if (imei.Length < 14 || imei.Length > 16)
        {
            errors.Add("IMEI must be between 14 and 16 characters long.");
        }

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            errors.Add("Device name cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(protocol))
        {
            errors.Add("Protocol cannot be null or empty.");
        }

        return errors;
    }

    /// <summary>
    /// Checks if a file path is valid for import operations.
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null.</exception>
    public static bool IsValid(this string filePath)
    {
        return filePath.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if CSV location data parameters are valid.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="speed">Speed value</param>
    /// <param name="timestamp">Timestamp of location</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static bool IsValid(
        string deviceId,
        double latitude,
        double longitude,
        double speed,
        DateTime timestamp)
    {
        return Validate(deviceId, latitude, longitude, speed, timestamp).Count == 0;
    }

    /// <summary>
    /// Checks if device import parameters are valid.
    /// </summary>
    /// <param name="imei">Device IMEI number</param>
    /// <param name="deviceName">Device display name</param>
    /// <param name="protocol">Communication protocol</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="imei"/> or <paramref name="deviceName"/> or <paramref name="protocol"/> is null.</exception>
    public static bool IsValid(
        string imei,
        string deviceName,
        string protocol)
    {
        return Validate(imei, deviceName, protocol).Count == 0;
    }

    /// <summary>
    /// Ensures a file path is valid for import operations, throwing an exception if not.
    /// </summary>
    /// <param name="filePath">The file path to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null.</exception>
    public static void EnsureValid(this string filePath)
    {
        var errors = filePath.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Ensures CSV location data parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="latitude">Latitude coordinate</param>
    /// <param name="longitude">Longitude coordinate</param>
    /// <param name="speed">Speed value</param>
    /// <param name="timestamp">Timestamp of location</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static void EnsureValid(
        string deviceId,
        double latitude,
        double longitude,
        double speed,
        DateTime timestamp)
    {
        var errors = Validate(deviceId, latitude, longitude, speed, timestamp);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Ensures device import parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="imei">Device IMEI number</param>
    /// <param name="deviceName">Device display name</param>
    /// <param name="protocol">Communication protocol</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="imei"/> or <paramref name="deviceName"/> or <paramref name="protocol"/> is null.</exception>
    public static void EnsureValid(
        string imei,
        string deviceName,
        string protocol)
    {
        var errors = Validate(imei, deviceName, protocol);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }
}