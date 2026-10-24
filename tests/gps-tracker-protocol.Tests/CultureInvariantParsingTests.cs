#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Regression tests: GPS protocol parsing must not depend on the thread culture
// =============================================================================

using System.Globalization;
using System.Text;
using FluentAssertions;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Parsers;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Utilities;
using Xunit;

namespace GpsTrackerProtocol.Tests;

/// <summary>
/// Runs parsing under a comma-decimal culture (de-DE) to prove NMEA, TK103 and H02 fields
/// are read with '.' as the decimal separator regardless of the host locale.
/// </summary>
public class CultureInvariantParsingTests
{
    private const string GermanCultureName = "de-DE";
    private const double CoordinateTolerance = 0.0001;

    // Latitude 49 deg 16.45 min N, longitude 123 deg 11.12 min W, speed 12.5, course 90.0.
    // Checksums are XOR over the data part, as expected by the TK103 and H02 frame validators.
    private const string Tk103Frame = "(123456789012345,20250101123456,4916.45,N,12311.12,W,12.5,90.0,*34";
    private const string H02Frame = "$GPRMC,123456,A,4916.45,N,12311.12,W,12.5,90.0,010125,,*08";
    private const string GprmcSentence = "$GPRMC,235947.00,A,5133.81,N,00042.25,W,0.13,309.62,120598,,,A*45";

    private const double ExpectedLatitude = 49 + 16.45 / 60;
    private const double ExpectedLongitude = -(123 + 11.12 / 60);
    private const double ExpectedGprmcLatitude = 51 + 33.81 / 60;
    private const double ExpectedGprmcLongitude = -(42.25 / 60);

    /// <summary>
    /// Sets the current thread culture to de-DE for the lifetime of the scope and restores it on dispose.
    /// </summary>
    private sealed class GermanCultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _previousUiCulture = CultureInfo.CurrentUICulture;

        /// <summary>
        /// Switches the current thread to de-DE and verifies that the culture really uses ',' as decimal separator.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the de-DE culture data is not available on this host.</exception>
        public GermanCultureScope()
        {
            var german = new CultureInfo(GermanCultureName);
            if (german.NumberFormat.NumberDecimalSeparator != ",")
                throw new InvalidOperationException($"Culture {GermanCultureName} does not use ',' as decimal separator on this host.");

            CultureInfo.CurrentCulture = german;
            CultureInfo.CurrentUICulture = german;
        }

        /// <summary>
        /// Restores the thread culture that was active before the scope was created.
        /// </summary>
        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }

    /// <summary>
    /// Verifies that an NMEA GPRMC sentence keeps '.' decimal values when the thread culture is de-DE.
    /// </summary>
    [Fact]
    public void ParseSentence_ShouldKeepDotDecimalValues_WhenThreadCultureIsGerman()
    {
        using var culture = new GermanCultureScope();
        var parser = new NmeaSentenceParser();

        var result = parser.ParseSentence(GprmcSentence, "device-1");

        result.Latitude.Should().BeApproximately(ExpectedGprmcLatitude, CoordinateTolerance);
        result.Longitude.Should().BeApproximately(ExpectedGprmcLongitude, CoordinateTolerance);
        result.Speed.Should().BeApproximately(0.13 * 1.852, CoordinateTolerance);
        result.Bearing.Should().BeApproximately(309.62, CoordinateTolerance);
    }

    /// <summary>
    /// Verifies that a TK103 frame parsed through <see cref="ProtocolParserService"/> keeps '.' decimal values under de-DE.
    /// </summary>
    [Fact]
    public async Task ParseFrameAsync_Tk103_ShouldKeepDotDecimalValues_WhenThreadCultureIsGerman()
    {
        using var culture = new GermanCultureScope();
        var service = new ProtocolParserService();
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.TK103,
            RawData = Encoding.ASCII.GetBytes(Tk103Frame)
        };

        var location = await service.ParseFrameAsync(frame);

        location.Latitude.Should().BeApproximately(ExpectedLatitude, CoordinateTolerance);
        location.Longitude.Should().BeApproximately(ExpectedLongitude, CoordinateTolerance);
        location.Speed.Should().BeApproximately(12.5, CoordinateTolerance);
        location.Bearing.Should().BeApproximately(90.0, CoordinateTolerance);
        location.Timestamp.Should().Be(new DateTime(2025, 1, 1, 12, 34, 56));
    }

    /// <summary>
    /// Verifies that an H02 frame parsed through <see cref="ProtocolParserService"/> keeps '.' decimal values
    /// and reads its ddmmyy date under de-DE.
    /// </summary>
    [Fact]
    public async Task ParseFrameAsync_H02_ShouldKeepDotDecimalValues_WhenThreadCultureIsGerman()
    {
        using var culture = new GermanCultureScope();
        var service = new ProtocolParserService();
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.H02,
            RawData = Encoding.ASCII.GetBytes(H02Frame)
        };

        var location = await service.ParseFrameAsync(frame);

        location.Latitude.Should().BeApproximately(ExpectedLatitude, CoordinateTolerance);
        location.Longitude.Should().BeApproximately(ExpectedLongitude, CoordinateTolerance);
        location.Speed.Should().BeApproximately(12.5, CoordinateTolerance);
        location.Bearing.Should().BeApproximately(90.0, CoordinateTolerance);
        location.Timestamp.Should().Be(new DateTime(2025, 1, 1, 12, 34, 56));
    }

    /// <summary>
    /// Verifies that the standalone TK103 parser keeps '.' decimal values under de-DE.
    /// </summary>
    [Fact]
    public void Tk103ProtocolParser_Parse_ShouldKeepDotDecimalValues_WhenThreadCultureIsGerman()
    {
        using var culture = new GermanCultureScope();
        var parser = new Tk103ProtocolParser();

        var result = parser.Parse(Encoding.ASCII.GetBytes(Tk103Frame));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Speed.Should().BeApproximately(12.5, CoordinateTolerance);
        result.Value.Bearing.Should().BeApproximately(90.0, CoordinateTolerance);
        result.Value.Latitude.Should().BeApproximately(ExpectedLatitude, CoordinateTolerance);
        result.Value.Longitude.Should().BeApproximately(ExpectedLongitude, CoordinateTolerance);
    }

    /// <summary>
    /// Verifies that the standalone H02 parser keeps '.' decimal values under de-DE.
    /// </summary>
    [Fact]
    public void H02ProtocolParser_Parse_ShouldKeepDotDecimalValues_WhenThreadCultureIsGerman()
    {
        using var culture = new GermanCultureScope();
        var parser = new H02ProtocolParser();

        var result = parser.Parse(Encoding.ASCII.GetBytes(H02Frame));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Speed.Should().BeApproximately(12.5, CoordinateTolerance);
        result.Value.Bearing.Should().BeApproximately(90.0, CoordinateTolerance);
        result.Value.Latitude.Should().BeApproximately(ExpectedLatitude, CoordinateTolerance);
        result.Value.Longitude.Should().BeApproximately(ExpectedLongitude, CoordinateTolerance);
    }

    /// <summary>
    /// Verifies that the generic string helper reads '.' decimals under de-DE.
    /// </summary>
    [Fact]
    public void ToDoubleOrDefault_ShouldReadDotDecimal_WhenThreadCultureIsGerman()
    {
        using var culture = new GermanCultureScope();

        "12.5".ToDoubleOrDefault().Should().Be(12.5);
    }

    /// <summary>
    /// Verifies that <see cref="LocationData.ToString"/> formats coordinates with '.' under de-DE.
    /// </summary>
    [Fact]
    public void LocationDataToString_ShouldUseDotDecimalSeparator_WhenThreadCultureIsGerman()
    {
        using var culture = new GermanCultureScope();
        var location = new LocationData
        {
            DeviceId = "device-1",
            Latitude = 49.5,
            Longitude = 11.25,
            Speed = 12.5
        };

        var text = location.ToString();

        text.Should().Contain("49.500000");
        text.Should().Contain("11.250000");
        text.Should().Contain("12.50km/h");
    }
}
