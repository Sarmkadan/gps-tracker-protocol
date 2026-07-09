#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Domain;

namespace GpsTrackerProtocol.Services;

/// <summary>
/// Parses ASCII NMEA 0183 sentences (GPRMC, GPGGA, GPGLL) into <see cref="LocationData"/>
/// </summary>
public class NmeaSentenceParser
{
    /// <summary>
    /// Returns true if the line starts with '$', has a '*' checksum section, and the XOR checksum of chars between '$' and '*' matches the two hex digits after '*'.
    /// </summary>
    public bool ValidateChecksum(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return false;

        if (!sentence.StartsWith('$'))
            return false;

        var asteriskIndex = sentence.IndexOf('*');
        if (asteriskIndex <= 0 || asteriskIndex >= sentence.Length - 3)
            return false;

        var checksumPart = sentence.Substring(asteriskIndex + 1);
        if (checksumPart.Length != 2)
            return false;

        var dataPart = sentence.Substring(1, asteriskIndex - 1);
        var calculatedChecksum = CalculateXorChecksum(dataPart);
        var expectedChecksum = checksumPart;

        return string.Equals(calculatedChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses one NMEA sentence for the given deviceId. Supports $GPRMC (position, speed in knots -> km/h, course, date+time), $GPGGA (position, altitude, satellite count, HDOP -> Accuracy), $GPGLL (position, time). Converts ddmm.mmmm/dddmm.mmmm with N/S/E/W signs to decimal degrees. Throws ChecksumException if checksum invalid, ParseException for unsupported/malformed sentences or 'V' (void) fix status.
    /// </summary>
    public LocationData ParseSentence(string sentence, string deviceId)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            throw new ParseException("Sentence cannot be null or empty", ProtocolType.Unknown);

        if (!ValidateChecksum(sentence))
            throw new ChecksumException("Invalid checksum", "", ProtocolType.Unknown);

        var parts = sentence.Split(',');
        if (parts.Length < 2)
            throw new ParseException("Sentence has insufficient parts", ProtocolType.Unknown);

        var sentenceType = parts[0];

        return sentenceType switch
        {
            "$GPRMC" => ParseGprmc(parts, deviceId),
            "$GPGGA" => ParseGpgga(parts, deviceId),
            "$GPGLL" => ParseGpgll(parts, deviceId),
            _ => throw new ParseException($"Unsupported sentence type: {sentenceType}", ProtocolType.Unknown)
        };
    }

    /// <summary>
    /// Splits a raw multi-line NMEA payload on CR/LF, parses every supported sentence, silently skips unsupported sentence types, and returns all successfully parsed fixes in order.
    /// </summary>
    public IReadOnlyList<LocationData> ParseBuffer(string rawText, string deviceId)
    {
        var results = new List<LocationData>();

        if (string.IsNullOrWhiteSpace(rawText))
            return results.AsReadOnly();

        var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            try
            {
                var location = ParseSentence(line.Trim(), deviceId);
                results.Add(location);
            }
            catch (GpsTrackerException)
            {
                // Silently skip unsupported or malformed sentences and checksum errors
                continue;
            }
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Converts an NMEA coordinate field (e.g. "4916.45" + hemisphere 'N') to signed decimal degrees. Public for reuse.
    /// </summary>
    public static double ConvertNmeaCoordinate(string value, char hemisphere)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));

        // Parse ddmm.mmmm format
        if (!double.TryParse(value, out var coordinateValue))
            throw new ParseException($"Invalid coordinate format: {value}");

        var degrees = Math.Floor(coordinateValue / 100.0);
        var minutes = coordinateValue % 100.0;
        var decimalDegrees = degrees + (minutes / 60.0);

        // Apply hemisphere sign
        return hemisphere switch
        {
            'S' or 'W' => -decimalDegrees,
            'N' or 'E' => decimalDegrees,
            _ => throw new ParseException($"Invalid hemisphere: {hemisphere}")
        };
    }

    private LocationData ParseGprmc(string[] parts, string deviceId)
    {
        // $GPRMC,hhmmss.ss,A,ddmm.mmmm,N,dddmm.mmmm,E,ssss.s,ddd,ddmmyy,dd,mmmm,M,,*hh
        // 0      1      2 3            4 5            6 7        8 9      10 11
        if (parts.Length < 12)
            throw new ParseException("GPRMC sentence has insufficient parts", ProtocolType.Unknown);

        var timePart = parts[1];
        var status = parts[2];
        var latitudePart = parts[3];
        var latitudeHemi = parts[4];
        var longitudePart = parts[5];
        var longitudeHemi = parts[6];
        var speedKnots = parts[7];
        var bearing = parts[8];
        var datePart = parts[9];

        // Validate fix status
        if (status != "A")
            throw new ParseException("Invalid fix status (not A)", ProtocolType.Unknown);

        var locationData = new LocationData
        {
            DeviceId = deviceId,
            Protocol = ProtocolType.Unknown,
            ExtendedData = new Dictionary<string, object>()
        };

        // Parse coordinates
        locationData.Latitude = ConvertNmeaCoordinate(latitudePart, latitudeHemi[0]);
        locationData.Longitude = ConvertNmeaCoordinate(longitudePart, longitudeHemi[0]);

        // Parse time and date
        if (timePart.Length >= 6)
        {
            var hours = int.Parse(timePart.Substring(0, 2));
            var minutes = int.Parse(timePart.Substring(2, 2));
            var seconds = int.Parse(timePart.Substring(4, Math.Min(2, timePart.Length - 4)));

            if (datePart.Length >= 6)
            {
                var day = int.Parse(datePart.Substring(0, 2));
                var month = int.Parse(datePart.Substring(2, 2));
                var year = int.Parse(datePart.Substring(4, 2));

                // Convert 2-digit year to 4-digit (assuming 20xx)
                var fullYear = 2000 + year;

                locationData.Timestamp = new DateTime(fullYear, month, day, hours, minutes, seconds, DateTimeKind.Utc);
            }
        }

        // Parse speed (knots to km/h)
        if (double.TryParse(speedKnots, out var speedKnotsValue))
        {
            locationData.Speed = speedKnotsValue * 1.852; // 1 knot = 1.852 km/h
        }

        // Parse bearing
        if (double.TryParse(bearing, out var bearingValue))
        {
            locationData.Bearing = bearingValue;
        }

        // Extended data
        locationData.ExtendedData["SentenceType"] = "GPRMC";
        locationData.ExtendedData["NmeaStatus"] = status;

        return locationData;
    }

    private LocationData ParseGpgga(string[] parts, string deviceId)
    {
        // $GPGGA,hhmmss.ss,ddmm.mmmm,N,dddmm.mmmm,E,x,xx,x.x,x.x,M,x.x,M,x.x,xxxx*hh
        // 0      1      2 3            4 5            6 7 8  9  10 11 12 13
        if (parts.Length < 15)
            throw new ParseException("GPGGA sentence has insufficient parts", ProtocolType.Unknown);

        var timePart = parts[1];
        var latitudePart = parts[2];
        var latitudeHemi = parts[3];
        var longitudePart = parts[4];
        var longitudeHemi = parts[5];
        var fixQuality = parts[6];
        var satelliteCount = parts[7];
        var hdop = parts[8];
        var altitude = parts[9];
        var altitudeUnits = parts[10];
        var geoidalSeparation = parts[11];

        var locationData = new LocationData
        {
            DeviceId = deviceId,
            Protocol = ProtocolType.Unknown,
            ExtendedData = new Dictionary<string, object>()
        };

        // Parse coordinates
        locationData.Latitude = ConvertNmeaCoordinate(latitudePart, latitudeHemi[0]);
        locationData.Longitude = ConvertNmeaCoordinate(longitudePart, longitudeHemi[0]);

        // Parse time
        if (timePart.Length >= 6)
        {
            var hours = int.Parse(timePart.Substring(0, 2));
            var minutes = int.Parse(timePart.Substring(2, 2));
            var seconds = int.Parse(timePart.Substring(4, Math.Min(2, timePart.Length - 4)));
            locationData.Timestamp = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hours, minutes, seconds, DateTimeKind.Utc);
        }

        // Parse fix quality
        if (int.TryParse(fixQuality, out var quality) && quality == 0)
            throw new ParseException("Invalid fix quality (no fix)", ProtocolType.Unknown);

        // Parse satellite count
        if (int.TryParse(satelliteCount, out var satCount))
        {
            locationData.SatelliteCount = satCount;
        }

        // Parse HDOP (Horizontal Dilution of Precision)
        if (double.TryParse(hdop, out var hdopValue))
        {
            locationData.Accuracy = hdopValue;
        }

        // Parse altitude
        if (double.TryParse(altitude, out var altValue))
        {
            locationData.Altitude = altValue;
        }

        // Extended data
        locationData.ExtendedData["SentenceType"] = "GPGGA";
        locationData.ExtendedData["FixQuality"] = fixQuality;
        locationData.ExtendedData["GeoidalSeparation"] = geoidalSeparation;

        return locationData;
    }

    private LocationData ParseGpgll(string[] parts, string deviceId)
    {
        // $GPGLL,ddmm.mmmm,N,dddmm.mmmm,E,hhmmss.ss,A*hh
        // 0      1 2            3 4          5        6
        if (parts.Length < 7)
            throw new ParseException("GPGLL sentence has insufficient parts", ProtocolType.Unknown);

        var latitudePart = parts[1];
        var latitudeHemi = parts[2];
        var longitudePart = parts[3];
        var longitudeHemi = parts[4];
        var timePart = parts[5];
        var status = parts[6];

        // Validate status
        if (status != "A")
            throw new ParseException("Invalid status (not A)", ProtocolType.Unknown);

        var locationData = new LocationData
        {
            DeviceId = deviceId,
            Protocol = ProtocolType.Unknown,
            ExtendedData = new Dictionary<string, object>()
        };

        // Parse coordinates
        locationData.Latitude = ConvertNmeaCoordinate(latitudePart, latitudeHemi[0]);
        locationData.Longitude = ConvertNmeaCoordinate(longitudePart, longitudeHemi[0]);

        // Parse time
        if (timePart.Length >= 6)
        {
            var hours = int.Parse(timePart.Substring(0, 2));
            var minutes = int.Parse(timePart.Substring(2, 2));
            var seconds = int.Parse(timePart.Substring(4, Math.Min(2, timePart.Length - 4)));
            locationData.Timestamp = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hours, minutes, seconds, DateTimeKind.Utc);
        }

        // Extended data
        locationData.ExtendedData["SentenceType"] = "GPGLL";
        locationData.ExtendedData["NmeaStatus"] = status;

        return locationData;
    }

    private string CalculateXorChecksum(string data)
    {
        var checksum = 0;

        foreach (var c in data)
        {
            checksum ^= c;
        }

        return checksum.ToString("X2");
    }
}
