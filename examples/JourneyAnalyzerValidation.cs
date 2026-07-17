#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Validation helpers for JourneyAnalyzer to ensure data integrity before analysis operations.
/// </summary>
public sealed static class JourneyAnalyzerValidation
{
    /// <summary>
    /// Validates device identifier for journey analysis operations.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <returns>List of validation errors, empty if valid</returns>
    public static IReadOnlyList<string> Validate(this string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            errors.Add("Device ID cannot be null or empty.");
            return errors;
        }

        if (deviceId.Length > 100)
        {
            errors.Add("Device ID cannot exceed 100 characters.");
        }

        return errors;
    }

    /// <summary>
    /// Validates device identifier and waypoint count for journey simulation.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="waypointCount">Number of waypoints to simulate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static IReadOnlyList<string> Validate(
        string deviceId,
        int waypointCount)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        var errors = new List<string>();

        var deviceErrors = deviceId.Validate();
        if (deviceErrors.Count > 0)
        {
            errors.AddRange(deviceErrors);
        }

        if (waypointCount < 1)
        {
            errors.Add("Waypoint count must be at least 1.");
        }
        else if (waypointCount > 1000)
        {
            errors.Add("Waypoint count cannot exceed 1000.");
        }

        return errors;
    }

    /// <summary>
    /// Validates JourneyAnalyzer instance state before operations.
    /// </summary>
    /// <param name="value">JourneyAnalyzer instance to validate</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this JourneyAnalyzer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // JourneyAnalyzer has no public state to validate
        // All state is initialized in constructor via DI

        return errors;
    }

    /// <summary>
    /// Checks if a device identifier is valid for journey analysis.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static bool IsValid(this string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        return deviceId.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if device identifier and waypoint count are valid for simulation.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="waypointCount">Number of waypoints to simulate</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    public static bool IsValid(
        string deviceId,
        int waypointCount)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        return Validate(deviceId, waypointCount).Count == 0;
    }

    /// <summary>
    /// Checks if a JourneyAnalyzer instance is valid.
    /// </summary>
    /// <param name="value">JourneyAnalyzer instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this JourneyAnalyzer value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures a device identifier is valid for journey analysis, throwing an exception if not.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        var errors = deviceId.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Ensures device identifier and waypoint count are valid for simulation, throwing an exception if not.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="waypointCount">Number of waypoints to simulate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(
        string deviceId,
        int waypointCount)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        var errors = Validate(deviceId, waypointCount);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Ensures a JourneyAnalyzer instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">JourneyAnalyzer instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValid(this JourneyAnalyzer value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }
}