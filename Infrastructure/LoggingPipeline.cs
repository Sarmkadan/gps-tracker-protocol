// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Infrastructure;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Structured logging pipeline for tracking frame processing, parsing, and storage.
/// Emits correlation IDs for tracing requests through the system.
/// </summary>
public interface ILoggingPipeline
{
    LogContext CreateContext(string frameId, string deviceId);
    void LogFrameReceived(LogContext ctx, GpsFrame frame);
    void LogParsingStarted(LogContext ctx, ProtocolType protocol);
    void LogParsingCompleted(LogContext ctx, LocationData location);
    void LogStorageStarted(LogContext ctx);
    void LogStorageCompleted(LogContext ctx, string storedId);
}

public class LoggingPipeline : ILoggingPipeline
{
    private readonly ILogger<LoggingPipeline> _logger;

    public LoggingPipeline(ILogger<LoggingPipeline> logger)
    {
        _logger = logger;
    }

    public LogContext CreateContext(string frameId, string deviceId)
    {
        return new LogContext
        {
            CorrelationId = Guid.NewGuid().ToString(),
            FrameId = frameId,
            DeviceId = deviceId,
            StartTime = DateTime.UtcNow
        };
    }

    public void LogFrameReceived(LogContext ctx, GpsFrame frame)
    {
        _logger.LogInformation(
            "[{CorrelationId}] Frame received | Device: {DeviceId}, Size: {Size}B, Protocol: {Protocol}",
            ctx.CorrelationId, ctx.DeviceId, frame.RawData.Length, frame.Protocol);
    }

    public void LogParsingStarted(LogContext ctx, ProtocolType protocol)
    {
        _logger.LogDebug(
            "[{CorrelationId}] Parsing started | Protocol: {Protocol}",
            ctx.CorrelationId, protocol);
    }

    public void LogParsingCompleted(LogContext ctx, LocationData location)
    {
        _logger.LogInformation(
            "[{CorrelationId}] Parsing completed | Lat: {Lat:F6}, Lon: {Lon:F6}, Speed: {Speed:F1}km/h",
            ctx.CorrelationId, location.Latitude, location.Longitude, location.Speed);
    }

    public void LogStorageStarted(LogContext ctx)
    {
        _logger.LogDebug("[{CorrelationId}] Storage started", ctx.CorrelationId);
    }

    public void LogStorageCompleted(LogContext ctx, string storedId)
    {
        var elapsed = DateTime.UtcNow - ctx.StartTime;
        _logger.LogInformation(
            "[{CorrelationId}] Storage completed | StoredId: {StoredId}, Elapsed: {Elapsed}ms",
            ctx.CorrelationId, storedId, elapsed.TotalMilliseconds);
    }
}

public class LogContext
{
    public string CorrelationId { get; set; }
    public string FrameId { get; set; }
    public string DeviceId { get; set; }
    public DateTime StartTime { get; set; }
}
