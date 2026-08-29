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
/// Handles parsing and routing for a specific GPS tracker protocol.
/// Implement this interface to support a new protocol type.
/// </summary>
public interface IProtocolHandler
{
    /// <summary>The protocol type this handler is responsible for.</summary>
    ProtocolType Protocol { get; }

    /// <summary>
    /// Returns true when the supplied preamble bytes match this protocol's signature.
    /// Implementations should only inspect the first few bytes.
    /// </summary>
    /// <param name="preamble">The leading bytes of the incoming data to inspect.</param>
    /// <returns><c>true</c> when the preamble matches this protocol's signature; otherwise, <c>false</c>.</returns>
    bool CanHandle(byte[] preamble);

    /// <summary>Creates a <see cref="GpsFrame"/> from raw connection data.</summary>
    /// <param name="data">The raw frame bytes received from the device.</param>
    /// <param name="sourceAddress">The network address of the device that sent the data.</param>
    /// <returns>A task that resolves to the constructed <see cref="GpsFrame"/>.</returns>
    Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress);
}

/// <summary>
/// Detects the GPS tracker protocol from the first bytes of a new TCP/UDP connection
/// and routes the data to the matching <see cref="IProtocolHandler"/>.
/// </summary>
public interface IProtocolAutoDetector
{
    /// <summary>
    /// Detects the protocol from the provided data and returns a detailed detection result.
    /// </summary>
    /// <param name="data">The preamble bytes to analyze.</param>
    /// <returns>A <see cref="ProtocolDetection"/> result indicating detection status.</returns>
    ProtocolDetection Detect(byte[] data);

    /// <summary>
    /// Returns the first handler whose signature matches <paramref name="data"/>,
    /// or <c>null</c> when no handler matches.
    /// </summary>
    /// <param name="data">The data to check for protocol compatibility.</param>
    /// <returns>The matching handler or null if no match.</returns>
    IProtocolHandler? GetHandler(byte[] data);

    /// <summary>
    /// Gets the minimum number of bytes required for reliable protocol detection.
    /// </summary>
    int MinimumDetectionBytesRequired { get; }

    /// <summary>
    /// Gets the minimum number of bytes required for each protocol type.
    /// </summary>
    /// <param name="protocol">The protocol type to check.</param>
    /// <returns>The minimum bytes required, or 0 if unknown.</returns>
    int GetMinimumBytesRequired(ProtocolType protocol);
}

/// <summary>
/// Auto-detector that inspects the leading bytes of incoming data and selects the
/// appropriate <see cref="IProtocolHandler"/> based on known protocol signatures:
/// <list type="bullet">
/// <item>GT06 – starts with <c>0x78 0x78</c> or <c>0x79 0x79</c></item>
/// <item>H02 – starts with <c>*HQ</c> or <c>$GPRMC</c></item>
/// <item>TK103 – starts with <c>(</c> (0x28)</item>
/// </list>
/// Uses minimum byte requirements to prevent ambiguous detections from short buffers.
/// Falls back to a configurable default protocol (or logs and returns
/// <see cref="ProtocolType.Unknown"/> when no default is set).
/// </summary>
public partial class ProtocolAutoDetector : IProtocolAutoDetector
{
    private readonly IReadOnlyList<IProtocolHandler> _handlers;
    private readonly ILogger<ProtocolAutoDetector> _logger;
    private readonly ProtocolType _defaultProtocol;
    private readonly Dictionary<ProtocolType, int> _minimumBytesRequired = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolAutoDetector"/> class.
    /// </summary>
    /// <param name="handlers">The protocol handlers available for signature matching.</param>
    /// <param name="logger">The logger used to record detection warnings.</param>
    /// <param name="defaultProtocol">
    /// The protocol to fall back to when no handler signature matches. Pass
    /// <see cref="ProtocolType.Unknown"/> (the default) to disable the fallback.
    /// </param>
    public ProtocolAutoDetector(
        IEnumerable<IProtocolHandler> handlers,
        ILogger<ProtocolAutoDetector> logger,
        ProtocolType defaultProtocol = ProtocolType.Unknown)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(logger);
        _handlers = handlers.ToList();
        _logger = logger;
        _defaultProtocol = defaultProtocol;

        // Define minimum bytes required for reliable detection of each protocol
        _minimumBytesRequired[ProtocolType.GT06] = 2;  // Needs at least 2 bytes for 0x78 0x78 signature
        _minimumBytesRequired[ProtocolType.H02] = 3;   // Needs at least 3 bytes for "*HQ" or "$GP"
        _minimumBytesRequired[ProtocolType.TK103] = 1; // Needs at least 1 byte for 0x28 '('
    }

    /// <summary>
    /// Gets the minimum number of bytes required for reliable protocol detection.
    /// This is the largest of the per-protocol minimum byte requirements.
    /// </summary>
    public int MinimumDetectionBytesRequired => _minimumBytesRequired.Values.DefaultIfEmpty(0).Max();

    /// <summary>
    /// Gets the minimum number of bytes required for each protocol type.
    /// </summary>
    /// <param name="protocol">The protocol type to check.</param>
    /// <returns>The minimum bytes required, or 0 if unknown.</returns>
    public int GetMinimumBytesRequired(ProtocolType protocol)
    {
        return _minimumBytesRequired.TryGetValue(protocol, out var bytes) ? bytes : 0;
    }

    /// <summary>
    /// Detects the protocol from the provided data and returns a detailed detection result.
    /// </summary>
    /// <param name="data">The preamble bytes to analyze.</param>
    /// <returns>A <see cref="ProtocolDetection"/> result indicating detection status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <c>null</c>.</exception>
    public ProtocolDetection Detect(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Check if we have enough data for any protocol detection
        if (data.Length == 0)
        {
            return ProtocolDetection.NeedMoreData(0, MinimumDetectionBytesRequired);
        }

        if (data.Length < MinimumDetectionBytesRequired)
        {
            return ProtocolDetection.NeedMoreData(data.Length, MinimumDetectionBytesRequired);
        }

        // Check for ambiguous matches - multiple handlers could potentially match
        var matchingHandlers = _handlers
            .Where(h => h.CanHandle(data))
            .ToList();

        if (matchingHandlers.Count == 1)
        {
            // Clear conclusive detection
            var protocol = matchingHandlers[0].Protocol;
            LogDetectionSucceeded(_logger, protocol, data.Length);
            return ProtocolDetection.Detected(protocol, data.Length);
        }

        if (matchingHandlers.Count > 1)
        {
            // Multiple protocols match - this is ambiguous
            var possibleProtocols = matchingHandlers.Select(h => h.Protocol).ToList();
            _logger.LogWarning(
                "Ambiguous protocol signature in {Length}-byte preamble: possible protocols {Protocols}",
                data.Length,
                string.Join(", ", possibleProtocols));
            return ProtocolDetection.Ambiguous(possibleProtocols, data.Length);
        }

        // No handlers match - check if we should use default or return unknown
        if (_defaultProtocol != ProtocolType.Unknown)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                LogUnknownPreamble(_logger, GetPreambleHex(data));
            }

            LogDetectionSucceeded(_logger, _defaultProtocol, data.Length);

            if (!_handlers.Any(handler => handler.Protocol == _defaultProtocol))
            {
                LogNoHandlerRegistered(_logger, _defaultProtocol);
            }

            return ProtocolDetection.Detected(_defaultProtocol, data.Length);
        }

        if (_logger.IsEnabled(LogLevel.Warning))
        {
            LogUnknownPreamble(_logger, GetPreambleHex(data));
        }

        return ProtocolDetection.Unknown(data.Length);
    }

    /// <summary>
    /// Returns the first handler whose signature matches <paramref name="data"/>,
    /// or <c>null</c> when no handler matches.
    /// </summary>
    /// <param name="data">The data to check for protocol compatibility.</param>
    /// <returns>The matching handler or null if no match.</returns>
    public IProtocolHandler? GetHandler(byte[] data)
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(data));
        if (handler is not null)
        {
            LogHandlerRouted(_logger, handler.Protocol);
        }

        return handler;
    }

    private static string GetPreambleHex(byte[] data) =>
        Convert.ToHexString(data.AsSpan(0, Math.Min(data.Length, 8)));

    [LoggerMessage(1000, LogLevel.Debug, "Detected protocol {Protocol} from a {PreambleLength}-byte preamble")]
    private static partial void LogDetectionSucceeded(
        ILogger logger,
        ProtocolType protocol,
        int preambleLength);

    [LoggerMessage(1001, LogLevel.Warning, "Protocol detection failed for preamble {PreambleHex}")]
    private static partial void LogUnknownPreamble(ILogger logger, string preambleHex);

    [LoggerMessage(1002, LogLevel.Debug, "Routed protocol {Protocol} to its registered handler")]
    private static partial void LogHandlerRouted(ILogger logger, ProtocolType protocol);

    [LoggerMessage(1003, LogLevel.Warning, "No handler registered for detected protocol {Protocol}")]
    private static partial void LogNoHandlerRegistered(ILogger logger, ProtocolType protocol);
}

/// <summary>
/// Protocol handler for the GT06 binary protocol.
/// Signature: first two bytes are <c>0x78 0x78</c> (standard) or <c>0x79 0x79</c> (extended).
/// </summary>
public class GT06ProtocolHandler : IProtocolHandler
{
    /// <summary>The protocol type this handler is responsible for.</summary>
    public ProtocolType Protocol => ProtocolType.GT06;

    /// <summary>
    /// Determines whether the preamble starts with the GT06 signature
    /// (<c>0x78 0x78</c> or <c>0x79 0x79</c>).
    /// </summary>
    /// <param name="preamble">The leading bytes of the incoming data to inspect.</param>
    /// <returns><c>true</c> when the first two bytes match a GT06 signature; otherwise, <c>false</c>.</returns>
    public bool CanHandle(byte[] preamble)
    {
        ArgumentNullException.ThrowIfNull(preamble);
        return preamble.Length >= 2 &&
               ((preamble[0] == 0x78 && preamble[1] == 0x78) ||
                (preamble[0] == 0x79 && preamble[1] == 0x79));
    }

    /// <summary>
    /// Creates a <see cref="GpsFrame"/> for the GT06 protocol from raw connection data.
    /// </summary>
    /// <param name="data">The raw frame bytes received from the device.</param>
    /// <param name="sourceAddress">The network address of the device that sent the data.</param>
    /// <returns>A task that resolves to the constructed GT06 <see cref="GpsFrame"/>.</returns>
    public Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(sourceAddress);
        return Task.FromResult(new GpsFrame
        {
            RawData = data,
            Protocol = ProtocolType.GT06,
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = sourceAddress
        });
    }
}

/// <summary>
/// Protocol handler for the H02 text protocol.
/// Signature: frame starts with <c>*HQ</c> (proprietary) or <c>$GPRMC</c> (NMEA).
/// </summary>
public class H02ProtocolHandler : IProtocolHandler
{
    /// <summary>The protocol type this handler is responsible for.</summary>
    public ProtocolType Protocol => ProtocolType.H02;

    /// <summary>
    /// Determines whether the preamble starts with the H02 signature
    /// (<c>*HQ</c> or <c>$GPRMC</c>).
    /// </summary>
    /// <param name="preamble">The leading bytes of the incoming data to inspect.</param>
    /// <returns><c>true</c> when the preamble starts with an H02 marker; otherwise, <c>false</c>.</returns>
    public bool CanHandle(byte[] preamble)
    {
        ArgumentNullException.ThrowIfNull(preamble);
        if (preamble.Length < 3)
            return false;

        var header = System.Text.Encoding.ASCII.GetString(preamble, 0, Math.Min(preamble.Length, 6));
        return header.StartsWith(ProtocolConstants.H02_HQ_START_MARKER, StringComparison.Ordinal) ||
               header.StartsWith(ProtocolConstants.H02_START_MARKER, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a <see cref="GpsFrame"/> for the H02 protocol from raw connection data.
    /// </summary>
    /// <param name="data">The raw frame bytes received from the device.</param>
    /// <param name="sourceAddress">The network address of the device that sent the data.</param>
    /// <returns>A task that resolves to the constructed H02 <see cref="GpsFrame"/>.</returns>
    public Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(sourceAddress);
        return Task.FromResult(new GpsFrame
        {
            RawData = data,
            Protocol = ProtocolType.H02,
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = sourceAddress
        });
    }
}

/// <summary>
/// Protocol handler for the TK103 text protocol.
/// Signature: frame starts with a parenthesis (<c>(</c>, byte value 0x28).
/// </summary>
public class TK103ProtocolHandler : IProtocolHandler
{
    /// <summary>The protocol type this handler is responsible for.</summary>
    public ProtocolType Protocol => ProtocolType.TK103;

    /// <summary>
    /// Determines whether the preamble starts with the TK103 signature
    /// (a parenthesis, <c>(</c>, byte value 0x28).
    /// </summary>
    /// <param name="preamble">The leading bytes of the incoming data to inspect.</param>
    /// <returns><c>true</c> when the first byte is the TK103 start marker; otherwise, <c>false</c>.</returns>
    public bool CanHandle(byte[] preamble)
    {
        ArgumentNullException.ThrowIfNull(preamble);
        return preamble.Length >= 1 && preamble[0] == ProtocolConstants.TK103_START_MARKER;
    }

    /// <summary>
    /// Creates a <see cref="GpsFrame"/> for the TK103 protocol from raw connection data.
    /// </summary>
    /// <param name="data">The raw frame bytes received from the device.</param>
    /// <param name="sourceAddress">The network address of the device that sent the data.</param>
    /// <returns>A task that resolves to the constructed TK103 <see cref="GpsFrame"/>.</returns>
    public Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(sourceAddress);
        return Task.FromResult(new GpsFrame
        {
            RawData = data,
            Protocol = ProtocolType.TK103,
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = sourceAddress
        });
    }
}
