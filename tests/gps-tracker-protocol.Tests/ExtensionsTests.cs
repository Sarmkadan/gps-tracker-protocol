#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using Xunit; // Added explicitly

namespace GpsTrackerProtocol.Tests;

/// <summary>
/// Contains unit tests for extension methods in the GpsTrackerProtocol.Utilities namespace.
/// Tests cover byte array conversions, string manipulations, and date/time operations.
/// </summary>
public class ExtensionsTests
{
    // ── ByteExtensions ────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="ByteExtensions.ToHexString(byte[])"/> converts a byte array to an uppercase hexadecimal string without dashes.
    /// </summary>
    [Fact]
    public void ToHexString_ByteArray_ReturnsUppercaseHexWithoutDashes()
    {
        var data = new byte[] { 0x78, 0x78, 0x0D };

        /// <summary>
        /// Tests that <see cref="ByteExtensions.ToHexString(byte[], bool)"/> with empty array returns empty string.
        /// </summary>
        data.ToHexString().Should().Be("78780D");
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.ToHexString(byte[], bool)"/> with an empty array returns an empty string.
    /// </summary>
    [Fact]
    public void ToHexString_EmptyArray_ReturnsEmptyString()
        /// <summary>
        /// Tests that <see cref="ByteExtensions.ToHexString(byte[], bool)"/> with addSpaces=true returns dash-separated hexadecimal string.
        /// </summary>
    {
        new byte[0].ToHexString().Should().Be(string.Empty);
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.ToHexString(byte[], bool)"/> with <c>addSpaces=true</c> returns a dash‑separated hexadecimal string.
    /// </summary>
    [Fact]
    public void ToHexString_WithSpaces_ReturnsDashSeparatedHex()
        /// <summary>
        /// Tests that <see cref="ByteExtensions.ToUInt16BigEndian(byte[], int)"/> reads a 16-bit unsigned integer in big-endian format from the specified byte array offset.
        /// </summary>
    {
        var data = new byte[] { 0x01, 0x02 };

        data.ToHexString(addSpaces: true).Should().Be("01-02");
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.ToUInt16BigEndian(byte[], int)"/> returns the correct big‑endian value when the offset is valid.
    /// </summary>
    [Fact]
    public void ToUInt16BigEndian_ValidOffset_ReturnsBigEndianValue()
    {
        // Bytes 0x01 0x02 → (0x01 << 8) | 0x02 = 258
        var data = new byte[] { 0x01, 0x02, 0x00, 0x00 };

        /// <summary>
        /// Tests that <see cref="ByteExtensions.CalculateXorChecksum(byte[], int, int)"/> calculates XOR checksum over specified byte range.
        /// </summary>
        data.ToUInt16BigEndian(0).Should().Be(258);
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.ToUInt16BigEndian(byte[], int)"/> throws <see cref="ArgumentException"/> when the offset is invalid.
    /// </summary>
    [Fact]
    public void ToUInt16BigEndian_InvalidOffset_ThrowsArgumentException()
    {
        /// <summary>
        /// Tests that <see cref="ByteExtensions.CalculateXorChecksum(byte[], int, int)"/> returns the same byte when calculating checksum over a single byte.
        /// </summary>
        var data = new byte[] { 0x01 };

        var act = () => data.ToUInt16BigEndian(0);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.CalculateXorChecksum(byte[], int, int)"/> returns the expected XOR result for a known byte sequence.
    /// </summary>
    [Fact]
    public void CalculateXorChecksum_KnownBytes_ReturnsExpectedXorResult()
    {
        // 0x01 ^ 0x02 ^ 0x03 = 0x00
        var data = new byte[] { 0x01, 0x02, 0x03 };
        /// <summary>
        /// Tests that <see cref="ByteExtensions.StartsWithMarker(byte[], byte, byte)"/> returns false when first byte does not match the expected marker.
        /// </summary>

        data.CalculateXorChecksum(0, 3).Should().Be(0x00);
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.CalculateXorChecksum(byte[], int, int)"/> returns the same byte when the range consists of a single byte.
    /// </summary>
    [Fact]
    public void CalculateXorChecksum_SingleByte_ReturnsSameByte()
        /// <summary>
        /// Tests that <see cref="ByteExtensions.IndexOfSequence(byte[], byte[])"/> finds the starting index of a byte sequence in an array.
        /// </summary>
    {
        var data = new byte[] { 0xAB };

        data.CalculateXorChecksum(0, 1).Should().Be(0xAB);
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.StartsWithMarker(byte[], byte, byte)"/> returns <c>true</c> when the byte array starts with the specified marker bytes.
    /// </summary>
    [Fact]
    public void StartsWithMarker_MatchingPrefix_ReturnsTrue()
    {
        var data = new byte[] { 0x78, 0x78, 0x01, 0x02 };

        data.StartsWithMarker(0x78, 0x78).Should().BeTrue();
        /// <summary>
        /// Tests that <see cref="ByteExtensions.CopyRange(byte[], int, int)"/> extracts a sub-range of bytes from the specified array.
        /// </summary>
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.StartsWithMarker(byte[], byte, byte)"/> returns <c>false</c> when the first byte does not match the expected marker.
    /// </summary>
    [Fact]
    public void StartsWithMarker_NonMatchingFirstByte_ReturnsFalse()
    {
        var data = new byte[] { 0x28, 0x78 };
        /// <summary>
        /// Tests that <see cref="ByteExtensions.ToAsciiString(byte[], int, int)"/> converts a byte range to an ASCII string.
        /// </summary>

        data.StartsWithMarker(0x78, 0x78).Should().BeFalse();
        /// <summary>
        /// Tests for <see cref="StringExtensions.IsValidImei(string)"/> extension methods.
        /// </summary>
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.IndexOfSequence(byte[], byte[])"/> returns the start index when the sequence is present.
    /// </summary>
    [Fact]
    public void IndexOfSequence_SequencePresent_ReturnsStartIndex()
    {
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        data.IndexOfSequence(new byte[] { 0x02, 0x03 }).Should().Be(1);
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.IndexOfSequence(byte[], byte[])"/> returns <c>-1</c> when the sequence is absent.
    /// </summary>
    [Fact]
    public void IndexOfSequence_SequenceAbsent_ReturnsMinusOne()
    {
        var data = new byte[] { 0x01, 0x02, 0x03 };

        /// <summary>
        /// Tests that <see cref="StringExtensions.IsValidImei(string)"/> returns false when IMEI contains non-digit characters.
        /// </summary>
        data.IndexOfSequence(new byte[] { 0x04, 0x05 }).Should().Be(-1);
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.CopyRange(byte[], int, int)"/> returns the correct subset for a valid range.
    /// </summary>
    [Fact]
    public void CopyRange_ValidRange_ReturnsCorrectSubset()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.IsValidDeviceId(string)"/> validates device ID strings (alphanumeric with dashes and underscores).
        /// </summary>
        var data = new byte[] { 0x10, 0x20, 0x30, 0x40 };

        data.CopyRange(1, 2).Should().Equal(new byte[] { 0x20, 0x30 });
    }

    /// <summary>
    /// Tests that <see cref="ByteExtensions.ToAsciiString(byte[], int, int)"/> correctly decodes a valid ASCII byte range.
    /// </summary>
    [Fact]
    public void ToAsciiString_ValidBytes_ReturnsDecodedString()
    {
        var data = System.Text.Encoding.ASCII.GetBytes("GT06");

        data.ToAsciiString(0, 4).Should().Be("GT06");
    }

    // ── StringExtensions ──────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidImei(string)"/> returns <c>true</c> for a 15‑digit IMEI.
    /// </summary>
    [Fact]
    public void IsValidImei_FifteenDigits_ReturnsTrue()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.SanitizeDeviceId(string)"/> returns "unknown" when input string is empty.
        /// </summary>
        "123456789012345".IsValidImei().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidImei(string)"/> returns <c>false</c> for a string shorter than 15 digits.
    /// </summary>
    [Fact]
    public void IsValidImei_TooShortString_ReturnsFalse()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.SanitizeDeviceId(string)"/> returns "unknown" when all characters are invalid.
        /// </summary>
        "12345".IsValidImei().Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidImei(string)"/> returns <c>false</c> when the IMEI contains non‑digit characters.
    /// </summary>
    [Fact]
    public void IsValidImei_ContainsNonDigit_ReturnsFalse()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.GetNmeaChecksum(string)"/> extracts the checksum part from an NMEA sentence.
        /// </summary>
        "12345678901234A".IsValidImei().Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidDeviceId(string)"/> returns <c>true</c> for an alphanumeric ID containing dashes and underscores.
    /// </summary>
    [Fact]
    public void IsValidDeviceId_AlphanumericWithDashUnderscore_ReturnsTrue()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.RemoveNmeaChecksum(string)"/> removes the star and checksum from an NMEA sentence.
        /// </summary>
        "device-001_test".IsValidDeviceId().Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.IsValidDeviceId(string)"/> returns <c>false</c> when the ID contains an '@' symbol.
    /// </summary>
    [Fact]
    public void IsValidDeviceId_ContainsAtSymbol_ReturnsFalse()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.SplitNmea(string)"/> splits an NMEA sentence by commas and trims whitespace from each field.
        /// </summary>
        "device@001".IsValidDeviceId().Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.SanitizeDeviceId(string)"/> removes invalid characters from a device ID.
    /// </summary>
    [Fact]
    public void SanitizeDeviceId_InvalidChars_StripsThemOut()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.HexToByteArray(string)"/> converts a valid hexadecimal string to a byte array.
        /// </summary>
        "device@123!".SanitizeDeviceId().Should().Be("device123");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.SanitizeDeviceId(string)"/> returns "unknown" when the input string is empty.
    /// </summary>
    [Fact]
    public void SanitizeDeviceId_EmptyString_ReturnsUnknown()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.HexToByteArray(string)"/> strips dash delimiters before parsing hexadecimal strings.
        /// </summary>
        string.Empty.SanitizeDeviceId().Should().Be("unknown");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.SanitizeDeviceId(string)"/> returns "unknown" when all characters are invalid.
    /// </summary>
    [Fact]
    public void SanitizeDeviceId_AllInvalidChars_ReturnsUnknown()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.HexToByteArray(string)"/> returns empty array when hexadecimal string has odd length.
        /// </summary>
        "@@@!!!".SanitizeDeviceId().Should().Be("unknown");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.GetNmeaChecksum(string)"/> extracts the checksum part from a NMEA sentence that includes a checksum.
    /// </summary>
    [Fact]
    public void GetNmeaChecksum_SentenceWithChecksum_ReturnsChecksumPart()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.Truncate(string, int)"/> truncates strings longer than max length and appends ellipsis.
        /// </summary>
        "$GPRMC,123456,A*4A".GetNmeaChecksum().Should().Be("4A");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.RemoveNmeaChecksum(string)"/> removes the star and checksum from a NMEA sentence.
    /// </summary>
    [Fact]
    public void RemoveNmeaChecksum_SentenceWithChecksum_RemovesStarAndBeyond()
    {
        /// <summary>
        /// Tests that <see cref="StringExtensions.Truncate(string, int)"/> returns the original string when it is shorter than max length.
        /// </summary>
        "$GPRMC,123456*4A".RemoveNmeaChecksum().Should().Be("$GPRMC,123456");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.SplitNmea(string)"/> splits a comma‑separated NMEA sentence and trims each field.
    /// </summary>
    [Fact]
    public void SplitNmea_CommaSeparatedSentence_ReturnsTrimmedFields()
    {
        /// <summary>
        /// Tests for <see cref="DateTimeExtensions"/> extension methods including Unix timestamp conversions and date/time rounding.
        /// </summary>
        var fields = "$GPRMC, 123456 ,A".SplitNmea();

        fields.Should().Equal("$GPRMC", "123456", "A");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.HexToByteArray(string)"/> correctly parses a valid hexadecimal string without delimiters.
    /// </summary>
    [Fact]
    public void HexToByteArray_ValidHexString_ReturnsExpectedBytes()
    {
        "7878".HexToByteArray().Should().Equal(new byte[] { 0x78, 0x78 });
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.HexToByteArray(string)"/> strips dash delimiters before parsing.
    /// </summary>
    [Fact]
    public void HexToByteArray_HexWithDashes_StripsDelimitersBeforeParsing()
    {
        "78-78".HexToByteArray().Should().Equal(new byte[] { 0x78, 0x78 });
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.HexToByteArray(string)"/> returns an empty array when the hex string has an odd length.
    /// </summary>
    [Fact]
    public void HexToByteArray_OddLengthHex_ReturnsEmpty()
    {
        "ABC".HexToByteArray().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.Truncate(string, int)"/> appends the suffix when the string is longer than the maximum length.
    /// </summary>
    [Fact]
    public void Truncate_StringLongerThanMax_AppendsSuffix()
    {
        "HelloWorld".Truncate(7).Should().Be("Hell...");
    }

    /// <summary>
    /// Tests that <see cref="StringExtensions.Truncate(string, int)"/> returns the original string when it is shorter than the maximum length.
    /// </summary>
    [Fact]
    public void Truncate_StringShorterThanMax_ReturnsOriginal()
    {
        "Hi".Truncate(10).Should().Be("Hi");
    }

    // ── DateTimeExtensions ────────────────────────────────────────────────────
    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.ToUnixTimestamp(DateTime)"/> returns zero for the Unix epoch.
    /// </summary>
    [Fact]
    public void ToUnixTimestamp_Epoch_ReturnsZero()
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Tests that <see cref="DateTimeExtensions.GetStartOfDay(DateTime)"/> returns midnight of the same day.
        /// </summary>
        epoch.ToUnixTimestamp().Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.FromUnixTimestamp(long)"/> converts zero back to the Unix epoch.
    /// </summary>
    [Fact]
    public void FromUnixTimestamp_Zero_ReturnsUnixEpoch()
    {
        var expected = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        DateTimeExtensions.FromUnixTimestamp(0).Should().Be(expected);
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.RoundDown(DateTime, TimeSpan)"/> floors a DateTime to the nearest 5‑minute boundary.
    /// </summary>
    [Fact]
    public void RoundDown_ToFiveMinutes_FloorsToBoundary()
    {
        var dt = new DateTime(2024, 6, 15, 12, 37, 45, DateTimeKind.Utc);

        dt.RoundDown(TimeSpan.FromMinutes(5))
          .Should().Be(new DateTime(2024, 6, 15, 12, 35, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.RoundUp(DateTime, TimeSpan)"/> ceils a DateTime to the nearest 5‑minute boundary.
    /// </summary>
    [Fact]
    public void RoundUp_ToFiveMinutes_CeilsToBoundary()
    {
        var dt = new DateTime(2024, 6, 15, 12, 32, 1, DateTimeKind.Utc);

        dt.RoundUp(TimeSpan.FromMinutes(5))
          .Should().Be(new DateTime(2024, 6, 15, 12, 35, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.IsSameDay(DateTime, DateTime)"/> returns <c>true</c> for two times on the same date.
    /// </summary>
    [Fact]
    public void IsSameDay_TwoTimesOnSameDate_ReturnsTrue()
    {
        var morning = new DateTime(2024, 6, 15, 8, 0, 0);
        var evening = new DateTime(2024, 6, 15, 22, 30, 0);

        morning.IsSameDay(evening).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.IsSameDay(DateTime, DateTime)"/> returns <c>false</c> for times on different dates.
    /// </summary>
    [Fact]
    public void IsSameDay_TwoTimesOnDifferentDates_ReturnsFalse()
    {
        var day1 = new DateTime(2024, 6, 15, 23, 59, 59);
        var day2 = new DateTime(2024, 6, 16, 0, 0, 0);

        day1.IsSameDay(day2).Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.GetStartOfDay(DateTime)"/> returns midnight for any given time.
    /// </summary>
    [Fact]
    public void GetStartOfDay_AnyTime_ReturnsMidnight()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);

        dt.GetStartOfDay().Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.GetStartOfMonth(DateTime)"/> returns the first day of the month at midnight.
    /// </summary>
    [Fact]
    public void GetStartOfMonth_MidMonth_ReturnsFirstDayMidnight()
    {
        var dt = new DateTime(2024, 6, 15);

        dt.GetStartOfMonth().Should().Be(new DateTime(2024, 6, 1));
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.ToRelativeTime(DateTime)"/> returns a human‑readable string for a time five minutes ago.
    /// </summary>
    [Fact]
    public void ToRelativeTime_FiveMinutesAgo_ReturnsMinutesAgoString()
    {
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

        fiveMinutesAgo.ToRelativeTime().Should().Be("5 minutes ago");
    }

    /// <summary>
    /// Tests that <see cref="DateTimeExtensions.ToRelativeTime(DateTime)"/> returns "just now" for a time less than a minute ago.
    /// </summary>
    [Fact]
    public void ToRelativeTime_ThirtySecondsAgo_ReturnsJustNow()
    {
        var thirtySecondsAgo = DateTime.UtcNow.AddSeconds(-30);

        thirtySecondsAgo.ToRelativeTime().Should().Be("just now");
    }
}
