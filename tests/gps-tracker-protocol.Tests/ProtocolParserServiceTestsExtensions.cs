#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace GpsTrackerProtocol.Tests;

public static class ProtocolParserServiceTestsExtensions
{
    /// <summary>
    /// Creates a test frame with the specified protocol and raw data for testing.
    /// </summary>
    public static GpsFrame CreateTestFrame(this ProtocolParserServiceTests _, ProtocolType protocol, byte[] rawData, DateTime? receivedAt = null)
    {
        return new GpsFrame
        {
            FrameId = $"test-frame-{protocol}",
            Protocol = protocol,
            RawData = rawData,
            ReceivedAt = receivedAt ?? DateTime.UtcNow,
            SourceAddress = "127.0.0.1",
            SourcePort = 12345,
            IsValidChecksum = true
        };
    }

    /// <summary>
    /// Creates a test frame with ASCII string data for protocols that use text-based formats.
    /// </summary>
    public static GpsFrame CreateTestFrame(this ProtocolParserServiceTests _, ProtocolType protocol, string rawString, DateTime? receivedAt = null)
    {
        var rawData = Encoding.ASCII.GetBytes(rawString);
        return new GpsFrame
        {
            FrameId = $"test-frame-{protocol}",
            Protocol = protocol,
            RawData = rawData,
            ReceivedAt = receivedAt ?? DateTime.UtcNow,
            SourceAddress = "127.0.0.1",
            SourcePort = 12345,
            IsValidChecksum = true
        };
    }

    /// <summary>
    /// Asserts that a frame parsing operation produces the expected location coordinates.
    /// </summary>
    public static async Task ShouldParseCoordinatesAsync(this ProtocolParserService service, GpsFrame frame, double expectedLatitude, double expectedLongitude, double tolerance = 0.0001)
    {
        var result = await service.ParseFrameAsync(frame).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Latitude.Should().BeApproximately(expectedLatitude, tolerance);
        result.Longitude.Should().BeApproximately(expectedLongitude, tolerance);
    }

    /// <summary>
    /// Asserts that a frame parsing operation produces the expected timestamp.
    /// </summary>
    public static async Task ShouldParseTimestampAsync(this ProtocolParserService service, GpsFrame frame, DateTime expectedTimestamp, double secondsTolerance = 1.0)
    {
        var result = await service.ParseFrameAsync(frame).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Timestamp.Should().BeCloseTo(expectedTimestamp, TimeSpan.FromSeconds(secondsTolerance));
    }
}