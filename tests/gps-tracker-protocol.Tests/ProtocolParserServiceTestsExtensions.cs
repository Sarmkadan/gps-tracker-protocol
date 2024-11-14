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
    /// <param name="protocol">The protocol type to use for the frame.</param>
    /// <param name="rawData">The raw frame data bytes.</param>
    /// <param name="receivedAt">Optional timestamp when the frame was received. Defaults to <see cref="DateTime.UtcNow"/>.</param>
    /// <returns>A configured <see cref="GpsFrame"/> instance for testing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rawData"/> is <see langword="null"/>.</exception>
    public static GpsFrame CreateTestFrame(this ProtocolParserServiceTests _, ProtocolType protocol, byte[] rawData, DateTime? receivedAt = null)
    {
        ArgumentNullException.ThrowIfNull(rawData);

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
    /// <param name="protocol">The protocol type to use for the frame.</param>
    /// <param name="rawString">The raw frame data as a string.</param>
    /// <param name="receivedAt">Optional timestamp when the frame was received. Defaults to <see cref="DateTime.UtcNow"/>.</param>
    /// <returns>A configured <see cref="GpsFrame"/> instance for testing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rawString"/> is <see langword="null"/>.</exception>
    public static GpsFrame CreateTestFrame(this ProtocolParserServiceTests _, ProtocolType protocol, string rawString, DateTime? receivedAt = null)
    {
        ArgumentNullException.ThrowIfNull(rawString);

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
    /// <param name="frame">The frame to parse.</param>
    /// <param name="expectedLatitude">The expected latitude coordinate.</param>
    /// <param name="expectedLongitude">The expected longitude coordinate.</param>
    /// <param name="tolerance">The coordinate comparison tolerance in degrees. Defaults to 0.0001.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="frame"/> is <see langword="null"/>.</exception>
    public static async Task ShouldParseCoordinatesAsync(this ProtocolParserService service, GpsFrame frame, double expectedLatitude, double expectedLongitude, double tolerance = 0.0001)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(frame);

        var result = await service.ParseFrameAsync(frame).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Latitude.Should().BeApproximately(expectedLatitude, tolerance);
        result.Longitude.Should().BeApproximately(expectedLongitude, tolerance);
    }

    /// <summary>
    /// Asserts that a frame parsing operation produces the expected timestamp.
    /// </summary>
    /// <param name="frame">The frame to parse.</param>
    /// <param name="expectedTimestamp">The expected timestamp value.</param>
    /// <param name="secondsTolerance">The time comparison tolerance in seconds. Defaults to 1.0.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="frame"/> is <see langword="null"/>.</exception>
    public static async Task ShouldParseTimestampAsync(this ProtocolParserService service, GpsFrame frame, DateTime expectedTimestamp, double secondsTolerance = 1.0)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(frame);

        var result = await service.ParseFrameAsync(frame).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Timestamp.Should().BeCloseTo(expectedTimestamp, TimeSpan.FromSeconds(secondsTolerance));
    }
}