#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Interface for parser metrics and observability
// =====================================================================

namespace GpsTrackerProtocol.Diagnostics;

using System.Diagnostics.Metrics;
using GpsTrackerProtocol.Domain;

/// <summary>
/// Interface for tracking protocol parsing metrics and failures.
/// </summary>
public interface IParserMetrics
{
    /// <summary>
    /// Records a successful frame parse.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    void RecordParseSuccess(ProtocolType protocol, double durationMs);

    /// <summary>
    /// Records a failed frame parse.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    void RecordParseFailure(ProtocolType protocol, string errorCode, double durationMs);

    /// <summary>
    /// Records a frame with unknown protocol.
    /// </summary>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    void RecordUnknownProtocol(double durationMs);

    /// <summary>
    /// Records a frame that was detected but couldn't be parsed due to unknown firmware.
    /// </summary>
    /// <param name="protocol">The detected protocol type.</param>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    void RecordUnknownFirmware(ProtocolType protocol, double durationMs);

    /// <summary>
    /// Captures a failed frame for offline diagnosis.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <param name="rawDataHex">Hex-encoded raw frame data.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="timestamp">The timestamp when the failure occurred.</param>
    void CaptureFailedFrame(ProtocolType protocol, string rawDataHex, string errorCode, DateTime timestamp);

    /// <summary>
    /// Gets the last N failed frames for diagnostics.
    /// </summary>
    /// <param name="count">Maximum number of frames to return.</param>
    /// <returns>List of failed frames with metadata.</returns>
    IReadOnlyList<FailedFrameCapture> GetFailedFrames(int count);
}

/// <summary>
/// Represents a captured failed frame for offline diagnosis.
/// </summary>
/// <param name="Protocol">The protocol type.</param>
/// <param name="RawDataHex">Hex-encoded raw frame data.</param>
/// <param name="ErrorCode">The error code.</param>
/// <param name="Timestamp">The timestamp when the failure occurred.</param>
/// <param name="DurationMs">The parse duration in milliseconds.</param>
public readonly record struct FailedFrameCapture(
    ProtocolType Protocol,
    string RawDataHex,
    string ErrorCode,
    DateTime Timestamp,
    double DurationMs);
