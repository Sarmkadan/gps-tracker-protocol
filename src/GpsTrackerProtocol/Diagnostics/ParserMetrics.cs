#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Implementation of parser metrics using System.Diagnostics.Metrics
// =====================================================================

namespace GpsTrackerProtocol.Diagnostics;

using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using GpsTrackerProtocol.Domain;

/// <summary>
/// Implementation of parser metrics using System.Diagnostics.Metrics.
/// Tracks counters for parsed/failed/unknown frames and parse duration histogram.
/// Optionally captures failed frames in a bounded ring buffer for offline diagnosis.
/// </summary>
public sealed class ParserMetrics : IParserMetrics, IDisposable
{
    private const string MeterName = "GpsTrackerProtocol.Parser";
    private const int MaxFailedFrames = 1000; // Bounded ring buffer size

    private readonly Meter _meter;
    private readonly Counter<long> _parsedFramesCounter;
    private readonly Counter<long> _failedFramesCounter;
    private readonly Counter<long> _unknownProtocolCounter;
    private readonly Counter<long> _unknownFirmwareCounter;
    private readonly Histogram<double> _parseDurationHistogram;
    private readonly ConcurrentQueue<FailedFrameCapture> _failedFramesQueue;
    private readonly object _queueLock = new();

    /// <summary>
    /// Initializes a new instance of the ParserMetrics class.
    /// </summary>
    public ParserMetrics()
    {
        _meter = new Meter(MeterName);

        _parsedFramesCounter = _meter.CreateCounter<long>(
            "parser.parsed.frames.total",
            "frames",
            "Total number of successfully parsed frames");

        _failedFramesCounter = _meter.CreateCounter<long>(
            "parser.failed.frames.total",
            "frames",
            "Total number of frames that failed to parse");

        _unknownProtocolCounter = _meter.CreateCounter<long>(
            "parser.unknown.protocol.frames.total",
            "frames",
            "Total number of frames with unknown protocol");

        _unknownFirmwareCounter = _meter.CreateCounter<long>(
            "parser.unknown.firmware.frames.total",
            "frames",
            "Total number of frames from unknown firmware versions");

        _parseDurationHistogram = _meter.CreateHistogram<double>(
            "parser.parse.duration.ms",
            "ms",
            "Duration of frame parsing operations");

        _failedFramesQueue = new ConcurrentQueue<FailedFrameCapture>();
    }

    /// <summary>
    /// Records a successful frame parse.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    public void RecordParseSuccess(ProtocolType protocol, double durationMs)
    {
        _parsedFramesCounter.Add(1, new KeyValuePair<string, object>("protocol", protocol.ToString()));
        _parseDurationHistogram.Record(durationMs, new KeyValuePair<string, object>("protocol", protocol.ToString()));
    }

    /// <summary>
    /// Records a failed frame parse.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    public void RecordParseFailure(ProtocolType protocol, string errorCode, double durationMs)
    {
        _failedFramesCounter.Add(1, new KeyValuePair<string, object>("protocol", protocol.ToString()), new KeyValuePair<string, object>("error", errorCode));
        _parseDurationHistogram.Record(durationMs, new KeyValuePair<string, object>("protocol", protocol.ToString()));
    }

    /// <summary>
    /// Records a frame with unknown protocol.
    /// </summary>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    public void RecordUnknownProtocol(double durationMs)
    {
        _unknownProtocolCounter.Add(1);
        _parseDurationHistogram.Record(durationMs);
    }

    /// <summary>
    /// Records a frame that was detected but couldn't be parsed due to unknown firmware.
    /// </summary>
    /// <param name="protocol">The detected protocol type.</param>
    /// <param name="durationMs">Parse duration in milliseconds.</param>
    public void RecordUnknownFirmware(ProtocolType protocol, double durationMs)
    {
        _unknownFirmwareCounter.Add(1, new KeyValuePair<string, object>("protocol", protocol.ToString()));
        _parseDurationHistogram.Record(durationMs, new KeyValuePair<string, object>("protocol", protocol.ToString()));
    }

    /// <summary>
    /// Captures a failed frame for offline diagnosis.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <param name="rawDataHex">Hex-encoded raw frame data.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="timestamp">The timestamp when the failure occurred.</param>
    public void CaptureFailedFrame(ProtocolType protocol, string rawDataHex, string errorCode, DateTime timestamp)
    {
        var capture = new FailedFrameCapture(protocol, rawDataHex, errorCode, timestamp, 0);

        lock (_queueLock)
        {
            // Enqueue the new capture
            _failedFramesQueue.Enqueue(capture);

            // If queue exceeds capacity, dequeue from the front
            if (_failedFramesQueue.Count > MaxFailedFrames)
            {
                _failedFramesQueue.TryDequeue(out _);
            }
        }
    }

    /// <summary>
    /// Gets the last N failed frames for diagnostics.
    /// </summary>
    /// <param name="count">Maximum number of frames to return.</param>
    /// <returns>List of failed frames with metadata.</returns>
    public IReadOnlyList<FailedFrameCapture> GetFailedFrames(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        lock (_queueLock)
        {
            if (_failedFramesQueue.IsEmpty)
            {
                return Array.Empty<FailedFrameCapture>();
            }

            // Convert queue to list and return the most recent frames
            var frames = _failedFramesQueue.ToList();

            if (frames.Count <= count)
            {
                return frames.AsReadOnly();
            }

            // Return the last N frames (most recent)
            return frames.Skip(frames.Count - count).ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Disposes the meter instrumentation.
    /// </summary>
    public void Dispose()
    {
        _meter?.Dispose();
        GC.SuppressFinalize(this);
    }
}
