#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpsTrackerProtocol.Tests;

public static class GpsUtilitiesTestsValidation
{
    /// <summary>
    /// Validates coordinate values for common issues.
    /// </summary>
    /// <param name="lat">Latitude to validate in decimal degrees</param>
    /// <param name="lon">Longitude to validate in decimal degrees</param>
    /// <returns>A list of human-readable problem descriptions, or empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are NaN or infinite</exception>
    public static IReadOnlyList<string> ValidateCoordinates(double lat, double lon)
    {
        if (double.IsNaN(lat))
        {
            throw new ArgumentOutOfRangeException(nameof(lat), "Latitude cannot be NaN");
        }
        if (double.IsNaN(lon))
        {
            throw new ArgumentOutOfRangeException(nameof(lon), "Longitude cannot be NaN");
        }
        if (double.IsInfinity(lat))
        {
            throw new ArgumentOutOfRangeException(nameof(lat), "Latitude cannot be infinite");
        }
        if (double.IsInfinity(lon))
        {
            throw new ArgumentOutOfRangeException(nameof(lon), "Longitude cannot be infinite");
        }

        var problems = new List<string>();

        // Validate that latitude is within valid range [-90, 90]
        if (lat < -90 || lat > 90)
        {
            problems.Add($"Latitude {lat} is out of range [-90, 90]");
        }

        // Validate that longitude is within valid range [-180, 180]
        if (lon < -180 || lon > 180)
        {
            problems.Add($"Longitude {lon} is out of range [-180, 180]");
        }

        return problems;
    }

    /// <summary>
    /// Validates distance value for common issues.
    /// </summary>
    /// <param name="distanceKm">Distance in kilometers to validate</param>
    /// <returns>A list of human-readable problem descriptions, or empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when distance is NaN or infinite</exception>
    public static IReadOnlyList<string> ValidateDistance(double distanceKm)
    {
        if (double.IsNaN(distanceKm))
        {
            throw new ArgumentOutOfRangeException(nameof(distanceKm), "Distance cannot be NaN");
        }
        if (double.IsInfinity(distanceKm))
        {
            throw new ArgumentOutOfRangeException(nameof(distanceKm), "Distance cannot be infinite");
        }

        var problems = new List<string>();

        // Validate that distance is non-negative
        if (distanceKm < 0)
        {
            problems.Add($"DistanceKm {distanceKm} is negative");
        }

        return problems;
    }

    /// <summary>
    /// Validates bearing value for common issues.
    /// </summary>
    /// <param name="bearing">Bearing in degrees to validate</param>
    /// <returns>A list of human-readable problem descriptions, or empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bearing is NaN or infinite</exception>
    public static IReadOnlyList<string> ValidateBearing(double bearing)
    {
        if (double.IsNaN(bearing))
        {
            throw new ArgumentOutOfRangeException(nameof(bearing), "Bearing cannot be NaN");
        }
        if (double.IsInfinity(bearing))
        {
            throw new ArgumentOutOfRangeException(nameof(bearing), "Bearing cannot be infinite");
        }

        var problems = new List<string>();

        // Validate that bearing is within valid range [0, 360)
        if (bearing < 0 || bearing >= 360)
        {
            problems.Add($"Bearing {bearing} is out of range [0, 360)");
        }

        return problems;
    }

    /// <summary>
    /// Validates speed value for common issues.
    /// </summary>
    /// <param name="speed">Speed to validate</param>
    /// <returns>A list of human-readable problem descriptions, or empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when speed is NaN or infinite</exception>
    public static IReadOnlyList<string> ValidateSpeed(double speed)
    {
        if (double.IsNaN(speed))
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed cannot be NaN");
        }
        if (double.IsInfinity(speed))
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed cannot be infinite");
        }

        var problems = new List<string>();

        // Validate that speed is non-negative
        if (speed < 0)
        {
            problems.Add($"Speed {speed} is negative");
        }

        return problems;
    }

    /// <summary>
    /// Validates zoom level value for common issues.
    /// </summary>
    /// <param name="zoomLevel">Zoom level to validate</param>
    /// <returns>A list of human-readable problem descriptions, or empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when zoom level is NaN or infinite</exception>
    public static IReadOnlyList<string> ValidateZoomLevel(double zoomLevel)
    {
        if (double.IsNaN(zoomLevel))
        {
            throw new ArgumentOutOfRangeException(nameof(zoomLevel), "Zoom level cannot be NaN");
        }
        if (double.IsInfinity(zoomLevel))
        {
            throw new ArgumentOutOfRangeException(nameof(zoomLevel), "Zoom level cannot be infinite");
        }

        var problems = new List<string>();

        // Validate that zoom level is within valid range [0, 20]
        if (zoomLevel < 0 || zoomLevel > 20)
        {
            problems.Add($"ZoomLevel {zoomLevel} is out of range [0, 20]");
        }

        return problems;
    }

    /// <summary>
    /// Validates bounding box coordinates for common issues.
    /// </summary>
    /// <param name="minLat">Minimum latitude in decimal degrees</param>
    /// <param name="maxLat">Maximum latitude in decimal degrees</param>
    /// <param name="minLon">Minimum longitude in decimal degrees</param>
    /// <param name="maxLon">Maximum longitude in decimal degrees</param>
    /// <returns>A list of human-readable problem descriptions, or empty if valid</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is NaN or infinite</exception>
    public static IReadOnlyList<string> ValidateBoundingBox(double minLat, double maxLat, double minLon, double maxLon)
    {
        if (double.IsNaN(minLat))
        {
            throw new ArgumentOutOfRangeException(nameof(minLat), "MinLat cannot be NaN");
        }
        if (double.IsNaN(maxLat))
        {
            throw new ArgumentOutOfRangeException(nameof(maxLat), "MaxLat cannot be NaN");
        }
        if (double.IsNaN(minLon))
        {
            throw new ArgumentOutOfRangeException(nameof(minLon), "MinLon cannot be NaN");
        }
        if (double.IsNaN(maxLon))
        {
            throw new ArgumentOutOfRangeException(nameof(maxLon), "MaxLon cannot be NaN");
        }
        if (double.IsInfinity(minLat))
        {
            throw new ArgumentOutOfRangeException(nameof(minLat), "MinLat cannot be infinite");
        }
        if (double.IsInfinity(maxLat))
        {
            throw new ArgumentOutOfRangeException(nameof(maxLat), "MaxLat cannot be infinite");
        }
        if (double.IsInfinity(minLon))
        {
            throw new ArgumentOutOfRangeException(nameof(minLon), "MinLon cannot be infinite");
        }
        if (double.IsInfinity(maxLon))
        {
            throw new ArgumentOutOfRangeException(nameof(maxLon), "MaxLon cannot be infinite");
        }

        var problems = new List<string>();

        // Validate individual coordinates first
        var coordProblems = ValidateCoordinates(minLat, minLon);
        if (coordProblems.Count > 0)
        {
            problems.AddRange(coordProblems);
        }

        coordProblems = ValidateCoordinates(maxLat, maxLon);
        if (coordProblems.Count > 0)
        {
            problems.AddRange(coordProblems);
        }

        // Validate that bounding box coordinates are in correct order
        if (minLat > maxLat)
        {
            problems.Add("MinLat is greater than MaxLat");
        }

        if (minLon > maxLon)
        {
            problems.Add("MinLon is greater than MaxLon");
        }

        return problems;
    }

    /// <summary>
    /// Checks if coordinate values are valid.
    /// </summary>
    /// <param name="lat">Latitude to check in decimal degrees</param>
    /// <param name="lon">Longitude to check in decimal degrees</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are NaN or infinite</exception>
    public static bool IsValidCoordinates(double lat, double lon)
        => ValidateCoordinates(lat, lon).Count == 0;

    /// <summary>
    /// Checks if distance value is valid.
    /// </summary>
    /// <param name="distanceKm">Distance to check in kilometers</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when distance is NaN or infinite</exception>
    public static bool IsValidDistance(double distanceKm)
        => ValidateDistance(distanceKm).Count == 0;

    /// <summary>
    /// Checks if bearing value is valid.
    /// </summary>
    /// <param name="bearing">Bearing to check in degrees</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bearing is NaN or infinite</exception>
    public static bool IsValidBearing(double bearing)
        => ValidateBearing(bearing).Count == 0;

    /// <summary>
    /// Checks if speed value is valid.
    /// </summary>
    /// <param name="speed">Speed to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when speed is NaN or infinite</exception>
    public static bool IsValidSpeed(double speed)
        => ValidateSpeed(speed).Count == 0;

    /// <summary>
    /// Checks if zoom level value is valid.
    /// </summary>
    /// <param name="zoomLevel">Zoom level to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when zoom level is NaN or infinite</exception>
    public static bool IsValidZoomLevel(double zoomLevel)
        => ValidateZoomLevel(zoomLevel).Count == 0;

    /// <summary>
    /// Checks if bounding box coordinates are valid.
    /// </summary>
    /// <param name="minLat">Minimum latitude in decimal degrees</param>
    /// <param name="maxLat">Maximum latitude in decimal degrees</param>
    /// <param name="minLon">Minimum longitude in decimal degrees</param>
    /// <param name="maxLon">Maximum longitude in decimal degrees</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is NaN or infinite</exception>
    public static bool IsValidBoundingBox(double minLat, double maxLat, double minLon, double maxLon)
        => ValidateBoundingBox(minLat, maxLat, minLon, maxLon).Count == 0;

    /// <summary>
    /// Ensures that coordinate values are valid, throwing an exception if not.
    /// </summary>
    /// <param name="lat">Latitude to validate in decimal degrees</param>
    /// <param name="lon">Longitude to validate in decimal degrees</param>
    /// <exception cref="ArgumentException">Thrown when coordinates are invalid</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are NaN or infinite</exception>
    public static void EnsureValidCoordinates(double lat, double lon)
    {
        var problems = ValidateCoordinates(lat, lon);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Coordinates are invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that distance value is valid, throwing an exception if not.
    /// </summary>
    /// <param name="distanceKm">Distance to validate in kilometers</param>
    /// <exception cref="ArgumentException">Thrown when distance is invalid</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when distance is NaN or infinite</exception>
    public static void EnsureValidDistance(double distanceKm)
    {
        var problems = ValidateDistance(distanceKm);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Distance is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that bearing value is valid, throwing an exception if not.
    /// </summary>
    /// <param name="bearing">Bearing to validate in degrees</param>
    /// <exception cref="ArgumentException">Thrown when bearing is invalid</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bearing is NaN or infinite</exception>
    public static void EnsureValidBearing(double bearing)
    {
        var problems = ValidateBearing(bearing);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Bearing is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that speed value is valid, throwing an exception if not.
    /// </summary>
    /// <param name="speed">Speed to validate</param>
    /// <exception cref="ArgumentException">Thrown when speed is invalid</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when speed is NaN or infinite</exception>
    public static void EnsureValidSpeed(double speed)
    {
        var problems = ValidateSpeed(speed);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Speed is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that zoom level value is valid, throwing an exception if not.
    /// </summary>
    /// <param name="zoomLevel">Zoom level to validate</param>
    /// <exception cref="ArgumentException">Thrown when zoom level is invalid</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when zoom level is NaN or infinite</exception>
    public static void EnsureValidZoomLevel(double zoomLevel)
    {
        var problems = ValidateZoomLevel(zoomLevel);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ZoomLevel is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that bounding box coordinates are valid, throwing an exception if not.
    /// </summary>
    /// <param name="minLat">Minimum latitude in decimal degrees</param>
    /// <param name="maxLat">Maximum latitude in decimal degrees</param>
    /// <param name="minLon">Minimum longitude in decimal degrees</param>
    /// <param name="maxLon">Maximum longitude in decimal degrees</param>
    /// <exception cref="ArgumentException">Thrown when bounding box is invalid</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is NaN or infinite</exception>
    public static void EnsureValidBoundingBox(double minLat, double maxLat, double minLon, double maxLon)
    {
        var problems = ValidateBoundingBox(minLat, maxLat, minLon, maxLon);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Bounding box is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}
