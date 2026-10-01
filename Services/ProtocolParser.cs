#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Unified parser that accepts raw byte data, detects the GPS protocol,
/// extracts frames, and returns structured <see cref="LocationData"/>.
/// Delegates position decoding to <see cref="PositionDecoder"/>.
/// </summary>
public class ProtocolParser
{
    private readonly PositionDecoder _positionDecoder;
    private readonly ProtocolAutoDetector _autoDetector;
    private readonly ILogger<ProtocolParser> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ProtocolParser"/>.
    /// </summary>
    /// <param name="positionDecoder">Decoder for coordinate extraction.</param>
    /// <param name="autoDetector">Auto-detection service for protocol identification.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public ProtocolParser(
        PositionDecoder positionDecoder,
        ProtocolAutoDetector autoDetector,
        ILogger<ProtocolParser> logger)
    {
        _positionDecoder = positionDecoder;
        _autoDetector = autoDetector;
        _logger = logger;
    }

    /// <summary>
    /// Parses raw bytes into a <see cref="ParseResult{LocationData}"/>.
    /// Detects the protocol automatically and delegates to the appropriate decoder.
    /// </summary>
    /// <param name="rawData">Raw bytes received from a device.</param>
    /// <returns>A parse result containing either the decoded location or an error.</returns>
    public ParseResult<LocationData> Parse(byte[] rawData)
    {
        if (rawData.Length == 0)
            return ParseResult<LocationData>.Failure("EMPTY_PAYLOAD", "Empty payload", 0);

        var detection = _autoDetector.Detect(rawData);
        if (detection.Result != ProtocolDetectionResult.Detected)
        {
            _logger.LogWarning("Unable to detect protocol for {Length}-byte payload (result: {Result})",
                rawData.Length, detection.Result);
            return ParseResult<LocationData>.Failure("UNKNOWN_PROTOCOL", "Unknown protocol", 0);
        }

        _logger.LogDebug("Detected protocol {Protocol}", detection.DetectedProtocol);

        var frame = new GpsFrame
        {
            FrameId = Guid.NewGuid().ToString(),
            Protocol = detection.DetectedProtocol,
            RawData = rawData,
            ReceivedAt = DateTime.UtcNow
        };

        var location = _positionDecoder.Decode(frame);
        if (location is null)
            return ParseResult<LocationData>.Failure("DECODE_FAILED",
                $"Failed to decode position from {detection.DetectedProtocol} frame", 0);

        return ParseResult<LocationData>.Success(location);
    }

    /// <summary>
    /// Parses raw bytes with an explicitly specified protocol, bypassing auto-detection.
    /// </summary>
    /// <param name="rawData">Raw bytes received from a device.</param>
    /// <param name="protocol">The known protocol type.</param>
    /// <returns>A parse result containing either the decoded location or an error.</returns>
    public ParseResult<LocationData> Parse(byte[] rawData, ProtocolType protocol)
    {
        if (rawData.Length == 0)
            return ParseResult<LocationData>.Failure("EMPTY_PAYLOAD", "Empty payload", 0);

        var frame = new GpsFrame
        {
            FrameId = Guid.NewGuid().ToString(),
            Protocol = protocol,
            RawData = rawData,
            ReceivedAt = DateTime.UtcNow
        };

        var location = _positionDecoder.Decode(frame);
        if (location is null)
            return ParseResult<LocationData>.Failure("DECODE_FAILED",
                $"Failed to decode position from {protocol} frame", 0);

        return ParseResult<LocationData>.Success(location);
    }

    /// <summary>
    /// Attempts to parse multiple concatenated frames from a single TCP read buffer.
    /// Returns all successfully parsed locations; logs and skips failures.
    /// </summary>
    /// <param name="buffer">Raw buffer potentially containing multiple frames.</param>
    /// <param name="frameSize">Expected size of each frame in bytes.</param>
    /// <param name="protocol">The protocol type for all frames.</param>
    /// <returns>List of successfully decoded locations.</returns>
    public IReadOnlyList<LocationData> ParseBatch(byte[] buffer, int frameSize, ProtocolType protocol)
    {
        var results = new List<LocationData>();
        if (frameSize <= 0)
        {
            _logger.LogWarning("Invalid frame size: {FrameSize}", frameSize);
            return results;
        }

        int offset = 0;
        while (offset + frameSize <= buffer.Length)
        {
            var segment = new byte[frameSize];
            Array.Copy(buffer, offset, segment, 0, frameSize);

            var result = Parse(segment, protocol);
            if (result.IsSuccess && result.Value is not null)
                results.Add(result.Value);
            else
                _logger.LogDebug("Skipped frame at offset {Offset}", offset);

            offset += frameSize;
        }

        if (offset < buffer.Length)
            _logger.LogDebug("Trailing {Count} bytes after last complete frame", buffer.Length - offset);

        return results;
    }
}
