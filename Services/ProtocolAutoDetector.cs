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
    bool CanHandle(byte[] preamble);

    /// <summary>Creates a <see cref="GpsFrame"/> from raw connection data.</summary>
    Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress);
}

/// <summary>
/// Detects the GPS tracker protocol from the first bytes of a new TCP/UDP connection
/// and routes the data to the matching <see cref="IProtocolHandler"/>.
/// </summary>
public interface IProtocolAutoDetector
{
    /// <summary>
    /// Returns the <see cref="ProtocolType"/> matched by the registered handlers,
    /// the configured default protocol, or <see cref="ProtocolType.Unknown"/> when
    /// no match is found and no default is configured.
    /// </summary>
    ProtocolType Detect(byte[] data);

    /// <summary>
    /// Returns the first handler whose signature matches <paramref name="data"/>,
    /// or <c>null</c> when no handler matches.
    /// </summary>
    IProtocolHandler? GetHandler(byte[] data);
}

/// <summary>
/// Auto-detector that inspects the leading bytes of incoming data and selects the
/// appropriate <see cref="IProtocolHandler"/> based on known protocol signatures:
/// <list type="bullet">
///   <item>GT06  – starts with <c>0x78 0x78</c> or <c>0x79 0x79</c></item>
///   <item>H02   – starts with <c>*HQ</c></item>
///   <item>TK103 – starts with <c>(</c> (0x28)</item>
/// </list>
/// Falls back to a configurable default protocol (or logs and returns
/// <see cref="ProtocolType.Unknown"/> when no default is set).
/// </summary>
public class ProtocolAutoDetector : IProtocolAutoDetector
{
    private readonly IReadOnlyList<IProtocolHandler> _handlers;
    private readonly ILogger<ProtocolAutoDetector> _logger;
    private readonly ProtocolType _defaultProtocol;

    public ProtocolAutoDetector(
        IEnumerable<IProtocolHandler> handlers,
        ILogger<ProtocolAutoDetector> logger,
        ProtocolType defaultProtocol = ProtocolType.Unknown)
    {
        _handlers = handlers.ToList();
        _logger = logger;
        _defaultProtocol = defaultProtocol;
    }

    public ProtocolType Detect(byte[] data)
    {
        var handler = GetHandler(data);
        if (handler is not null)
            return handler.Protocol;

        if (_defaultProtocol != ProtocolType.Unknown)
        {
            _logger.LogWarning(
                "Unknown protocol signature in {Length}-byte preamble, falling back to default protocol {Default}",
                data.Length, _defaultProtocol);
            return _defaultProtocol;
        }

        _logger.LogWarning(
            "Unknown protocol signature in {Length}-byte preamble; no default configured, disconnecting",
            data.Length);
        return ProtocolType.Unknown;
    }

    public IProtocolHandler? GetHandler(byte[] data) =>
        _handlers.FirstOrDefault(h => h.CanHandle(data));
}

/// <summary>
/// Protocol handler for the GT06 binary protocol.
/// Signature: first two bytes are <c>0x78 0x78</c> (standard) or <c>0x79 0x79</c> (extended).
/// </summary>
public class GT06ProtocolHandler : IProtocolHandler
{
    public ProtocolType Protocol => ProtocolType.GT06;

    public bool CanHandle(byte[] preamble) =>
        preamble.Length >= 2 &&
        ((preamble[0] == 0x78 && preamble[1] == 0x78) ||
         (preamble[0] == 0x79 && preamble[1] == 0x79));

    public Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress) =>
        Task.FromResult(new GpsFrame
        {
            RawData = data,
            Protocol = ProtocolType.GT06,
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = sourceAddress
        });
}

/// <summary>
/// Protocol handler for the H02 text protocol.
/// Signature: frame starts with <c>*HQ</c> (proprietary) or <c>$GPRMC</c> (NMEA).
/// </summary>
public class H02ProtocolHandler : IProtocolHandler
{
    public ProtocolType Protocol => ProtocolType.H02;

    public bool CanHandle(byte[] preamble)
    {
        if (preamble.Length < 3)
            return false;

        var header = System.Text.Encoding.ASCII.GetString(preamble, 0, Math.Min(preamble.Length, 6));
        return header.StartsWith(ProtocolConstants.H02_HQ_START_MARKER, StringComparison.Ordinal) ||
               header.StartsWith(ProtocolConstants.H02_START_MARKER, StringComparison.Ordinal);
    }

    public Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress) =>
        Task.FromResult(new GpsFrame
        {
            RawData = data,
            Protocol = ProtocolType.H02,
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = sourceAddress
        });
}

/// <summary>
/// Protocol handler for the TK103 text protocol.
/// Signature: frame starts with a parenthesis (<c>(</c>, byte value 0x28).
/// </summary>
public class TK103ProtocolHandler : IProtocolHandler
{
    public ProtocolType Protocol => ProtocolType.TK103;

    public bool CanHandle(byte[] preamble) =>
        preamble.Length >= 1 && preamble[0] == ProtocolConstants.TK103_START_MARKER;

    public Task<GpsFrame> CreateFrameAsync(byte[] data, string sourceAddress) =>
        Task.FromResult(new GpsFrame
        {
            RawData = data,
            Protocol = ProtocolType.TK103,
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = sourceAddress
        });
}
