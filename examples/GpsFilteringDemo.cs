#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace GpsTrackerProtocol.Examples;

using System;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

/// <summary>
/// Demonstrates the GPS fix quality filtering feature.
/// Shows how the LocationSanityFilter rejects garbage GPS points.
/// </summary>
public static class GpsFilteringDemo
{
    public static void Run()
    {
        Console.WriteLine("=== GPS Fix Quality Filtering Demo ===\n");

        var filter = new LocationSanityFilter();

        // Example 1: Valid location
        Console.WriteLine("Example 1: Valid GPS location");
        var validLocation = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 40.7128,
            Longitude = -74.0060, // New York City
            Timestamp = DateTime.UtcNow,
            Speed = 65.5,
            Altitude = 10.0,
            SatelliteCount = 8
        };

        var result1 = filter.FilterLocation(validLocation);
        Console.WriteLine($"  Result: {(result1.IsAccepted ? "✓ ACCEPTED" : "✗ REJECTED")}");
        Console.WriteLine($"  Decision: {result1.Decision}");
        Console.WriteLine();

        // Example 2: Null island (0,0) - common cold start issue
        Console.WriteLine("Example 2: Null island coordinates (0,0) - Cold start garbage");
        var nullIsland = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 0,
            Longitude = 0,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 0,
            SatelliteCount = 0
        };

        var result2 = filter.FilterLocation(nullIsland);
        Console.WriteLine($"  Result: {(result2.IsAccepted ? "✓ ACCEPTED" : "✗ REJECTED")}");
        Console.WriteLine($"  Decision: {result2.Decision}");
        Console.WriteLine($"  Reason: {result2.Details}");
        Console.WriteLine();

        // Example 3: Invalid coordinates
        Console.WriteLine("Example 3: Invalid coordinates (latitude > 90)");
        var invalidCoords = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 100, // Invalid!
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 0,
            SatelliteCount = 5
        };

        var result3 = filter.FilterLocation(invalidCoords);
        Console.WriteLine($"  Result: {(result3.IsAccepted ? "✓ ACCEPTED" : "✗ REJECTED")}");
        Console.WriteLine($"  Decision: {result3.Decision}");
        Console.WriteLine();

        // Example 4: Future timestamp
        Console.WriteLine("Example 4: Future timestamp (device clock is wrong)");
        var futureTime = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow.AddHours(2), // 2 hours in future
            Speed = 50,
            Altitude = 10.0,
            SatelliteCount = 8
        };

        var result4 = filter.FilterLocation(futureTime);
        Console.WriteLine($"  Result: {(result4.IsAccepted ? "✓ ACCEPTED" : "✗ REJECTED")}");
        Console.WriteLine($"  Decision: {result4.Decision}");
        Console.WriteLine();

        // Example 5: Impossible speed jump
        Console.WriteLine("Example 5: Impossible speed between consecutive points");
        var previous = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow.AddMinutes(-10),
            Speed = 0,
            Altitude = 10.0,
            SatelliteCount = 8
        };

        var impossibleSpeed = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 34.0522, // Los Angeles - ~3900 km from NYC
            Longitude = -118.2437,
            Timestamp = DateTime.UtcNow,
            Speed = 2000, // Impossible!
            Altitude = 10.0,
            SatelliteCount = 8
        };

        var result5 = filter.FilterLocation(impossibleSpeed, previous);
        Console.WriteLine($"  Previous: ({previous.Latitude:F4}, {previous.Longitude:F4}) at {previous.Timestamp:HH:mm:ss}");
        Console.WriteLine($"  Current:  ({impossibleSpeed.Latitude:F4}, {impossibleSpeed.Longitude:F4}) at {impossibleSpeed.Timestamp:HH:mm:ss}");
        Console.WriteLine($"  Time diff: {(impossibleSpeed.Timestamp - previous.Timestamp).TotalMinutes:F1} minutes");
        Console.WriteLine($"  Result: {(result5.IsAccepted ? "✓ ACCEPTED" : "✗ REJECTED")}");
        Console.WriteLine($"  Decision: {result5.Decision}");
        Console.WriteLine($"  Reason: {result5.Details}");
        Console.WriteLine();

        // Example 6: Cold start with 0 satellites (should be accepted)
        Console.WriteLine("Example 6: Cold start scenario (0 satellites but valid coordinates)");
        var coldStart = new LocationData
        {
            Id = Guid.NewGuid().ToString(),
            DeviceId = "TRK-001",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Timestamp = DateTime.UtcNow,
            Speed = 0,
            Altitude = 10.0,
            SatelliteCount = 0 // Cold start - device has fix but no satellites yet
        };

        var result6 = filter.FilterLocation(coldStart);
        Console.WriteLine($"  Result: {(result6.IsAccepted ? "✓ ACCEPTED" : "✗ REJECTED")}");
        Console.WriteLine($"  Decision: {result6.Decision}");
        Console.WriteLine($"  Note: Cold start points are accepted for initial fix acquisition");
        Console.WriteLine();

        // Summary
        Console.WriteLine("=== Summary ===");
        Console.WriteLine("The LocationSanityFilter successfully filters out:");
        Console.WriteLine("  • Null island coordinates (0,0)");
        Console.WriteLine("  • Invalid coordinate values");
        Console.WriteLine("  • Future/past timestamps");
        Console.WriteLine("  • Impossible speed jumps between points");
        Console.WriteLine("  • Invalid altitudes");
        Console.WriteLine("  • Invalid GPS fix states");
        Console.WriteLine();
        Console.WriteLine("Accepted points: " + filter.Configuration.RejectNullIsland + "/" + filter.Configuration.RejectInvalidCoordinates + "/" + filter.Configuration.RejectNoSatelliteFix);
    }
}
