#nullable enable

using System;
using System.Collections.Generic;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Xunit;

namespace GpsTrackerProtocol.Tests
{
    public class NmeaSentenceParserTests
    {
        private readonly NmeaSentenceParser _parser = new();

        [Fact]
        public void ParseGpgga_ValidSentence_ReturnsExpectedLocationData()
        {
            // Example from NMEA documentation – checksum verified.
            const string sentence = "$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47";

            var result = _parser.ParseSentence(sentence, "device-1");

            Assert.NotNull(result);
            Assert.Equal("device-1", result.DeviceId);
            // Latitude 48°07.038' => 48 + 7.038/60 = 48.1173
            Assert.Equal(48.1173, Math.Round(result.Latitude, 4));
            // Longitude 11°31.000' => 11 + 31.000/60 = 11.5166667
            Assert.Equal(11.5166667, Math.Round(result.Longitude, 7));
            Assert.Equal(545.4, result.Altitude);
            Assert.Equal(8, result.SatelliteCount);
            Assert.Equal(0.9, result.Accuracy);
            Assert.Equal("GPGGA", result.ExtendedData["SentenceType"]);
        }

        [Fact]
        public void ParseGprmc_ValidSentence_ReturnsExpectedLocationData()
        {
            // Example from NMEA documentation – checksum verified.
            const string sentence = "$GPRMC,235947.00,A,5133.81,N,00042.25,W,0.13,309.62,120598,,,A*6C";

            var result = _parser.ParseSentence(sentence, "device-2");

            Assert.NotNull(result);
            Assert.Equal("device-2", result.DeviceId);
            // Latitude 51°33.81' => 51 + 33.81/60 = 51.5635
            Assert.Equal(51.5635, Math.Round(result.Latitude, 4));
            // Longitude 0°42.25' West => -(0 + 42.25/60) = -0.7041667
            Assert.Equal(-0.7041667, Math.Round(result.Longitude, 7));
            // Speed 0.13 knots => 0.13 * 1.852 km/h = 0.24076
            Assert.Equal(0.24076, Math.Round(result.Speed, 5));
            Assert.Equal(309.62, result.Bearing);
            Assert.Equal("GPRMC", result.ExtendedData["SentenceType"]);
        }

        [Fact]
        public void ConvertNmeaCoordinate_SouthernAndWesternHemisphere_ReturnsNegativeValues()
        {
            // Latitude 12°34.567' South => -(12 + 34.567/60)
            double lat = NmeaSentenceParser.ConvertNmeaCoordinate("1234.567", 'S');
            // Longitude 098°12.345' West => -(98 + 12.345/60)
            double lon = NmeaSentenceParser.ConvertNmeaCoordinate("09812.345", 'W');

            Assert.Equal(-(12 + 34.567 / 60), Math.Round(lat, 6));
            Assert.Equal(-(98 + 12.345 / 60), Math.Round(lon, 6));
        }

        [Fact]
        public void ParseSentence_MalformedChecksum_ThrowsParseException()
        {
            // Intentionally corrupted checksum (*00)
            const string badChecksum = "$GPRMC,235947.00,A,5133.81,N,00042.25,W,0.13,309.62,120598,,,A*00";

            var ex = Assert.Throws<ParseException>(() => _parser.ParseSentence(badChecksum, "device-3"));
            Assert.Contains("Invalid checksum", ex.Message);
        }

        [Fact]
        public void ParseSentence_EmptyString_ThrowsParseException()
        {
            var ex = Assert.Throws<ParseException>(() => _parser.ParseSentence(string.Empty, "device-4"));
            Assert.Contains("cannot be null or empty", ex.Message);
        }
    }
}
