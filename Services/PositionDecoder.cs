#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Utilities;

/// <summary>
/// Decodes raw binary position data from GPS frames into structured location records.
/// Supports GT06, H02, and TK103 coordinate encoding formats.
/// </summary>
public class PositionDecoder
{
    private readonly ILogger<PositionDecoder> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PositionDecoder"/>.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public PositionDecoder(ILogger<PositionDecoder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Decodes latitude and longitude from a GT06 binary frame payload.
    /// GT06 stores coordinates as 4-byte big-endian integers scaled by 1,800,000.
    /// </summary>
    /// <param name="payload">Raw payload bytes starting at the coordinate offset.</param>
    /// <param name="offset">Byte offset where latitude begins.</param>
    /// <returns>A parsed <see cref="LocationData"/> or null if the payload is too short.</returns>
    public LocationData? DecodeGt06Position(byte[] payload, int offset = 0)
    {
        if (payload.Length < offset + 8)
        {
            _logger.LogWarning("GT06 payload too short: {Length} bytes at offset {Offset}", payload.Length, offset);
            return null;
        }

        int rawLat = (payload[offset] << 24) | (payload[offset + 1] << 16)
                   | (payload[offset + 2] << 8) | payload[offset + 3];
        int rawLon = (payload[offset + 4] << 24) | (payload[offset + 5] << 16)
                   | (payload[offset + 6] << 8) | payload[offset + 7];

        double latitude = rawLat / 1_800_000.0;
        double longitude = rawLon / 1_800_000.0;

        if (!GpsUtilities.IsValidCoordinate(latitude, longitude))
        {
            _logger.LogWarning("Decoded invalid coordinates: {Lat}, {Lon}", latitude, longitude);
            return null;
        }

        return new LocationData
        {
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Decodes an H02 ASCII position string in NMEA-like format (DDMM.MMMM).
    /// </summary>
    /// <param name="latStr">Latitude string, e.g. "4807.038".</param>
    /// <param name="latDir">Hemisphere indicator: 'N' or 'S'.</param>
    /// <param name="lonStr">Longitude string, e.g. "01131.000".</param>
    /// <param name="lonDir">Hemisphere indicator: 'E' or 'W'.</param>
    /// <returns>Parsed <see cref="LocationData"/> or null on invalid input.</returns>
    public LocationData? DecodeH02Position(string latStr, char latDir, string lonStr, char lonDir)
    {
        if (!TryParseNmeaCoordinate(latStr, out double latitude)
            || !TryParseNmeaCoordinate(lonStr, out double longitude))
        {
            _logger.LogWarning("Failed to parse H02 NMEA coordinates: {Lat} {Lon}", latStr, lonStr);
            return null;
        }

        if (latDir == 'S') latitude = -latitude;
        if (lonDir == 'W') longitude = -longitude;

        if (!GpsUtilities.IsValidCoordinate(latitude, longitude))
            return null;

        return new LocationData
        {
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Decodes a TK103 comma-separated position segment.
    /// Expected format: "lat,N/S,lon,E/W".
    /// </summary>
    /// <param name="segment">Comma-separated coordinate segment.</param>
    /// <returns>Parsed <see cref="LocationData"/> or null on failure.</returns>
    public LocationData? DecodeTk103Position(string segment)
    {
        var parts = segment.Split(',');
        if (parts.Length < 4)
        {
            _logger.LogWarning("TK103 segment has fewer than 4 parts: {Segment}", segment);
            return null;
        }

        return DecodeH02Position(parts[0], parts[1][0], parts[2], parts[3][0]);
    }

    /// <summary>
    /// Determines the appropriate decoding strategy based on protocol type and invokes it.
    /// </summary>
    /// <param name="frame">The GPS frame to decode.</param>
    /// <returns>Decoded location data or null if the protocol is unsupported or data is invalid.</returns>
    public LocationData? Decode(GpsFrame frame)
    {
        return frame.Protocol switch
        {
            ProtocolType.GT06 => DecodeGt06Position(frame.RawData),
            ProtocolType.H02 => DecodeH02Position(
                System.Text.Encoding.ASCII.GetString(frame.RawData), 'N',
                string.Empty, 'E'),
            ProtocolType.TK103 => DecodeTk103Position(
                System.Text.Encoding.ASCII.GetString(frame.RawData)),
            _ => LogAndReturnNull(frame.Protocol)
        };
    }

    private LocationData? LogAndReturnNull(ProtocolType protocol)
    {
        _logger.LogWarning("Unsupported protocol for position decoding: {Protocol}", protocol);
        return null;
    }

    private static bool TryParseNmeaCoordinate(string raw, out double decimalDegrees)
    {
        decimalDegrees = 0;
        if (string.IsNullOrWhiteSpace(raw) || raw.Length < 4)
            return false;

        int dotIndex = raw.IndexOf('.');
        if (dotIndex < 2)
            return false;

        string degPart = raw[..(dotIndex - 2)];
        string minPart = raw[(dotIndex - 2)..];

        if (!double.TryParse(degPart, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double degrees)
            || !double.TryParse(minPart, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double minutes))
            return false;

        decimalDegrees = degrees + minutes / 60.0;
        return true;
    }
}
