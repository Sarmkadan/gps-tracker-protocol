// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using Xunit; // Added explicitly

namespace GpsTrackerProtocol.Tests;

public class ExtensionsTests
{
    // ── ByteExtensions ────────────────────────────────────────────────────────

    [Fact]
    public void ToHexString_ByteArray_ReturnsUppercaseHexWithoutDashes()
    {
        var data = new byte[] { 0x78, 0x78, 0x0D };

        data.ToHexString().Should().Be("78780D");
    }

    [Fact]
    public void ToHexString_EmptyArray_ReturnsEmptyString()
    {
        Array.Empty<byte>().ToHexString().Should().Be(string.Empty);
    }

    [Fact]
    public void ToHexString_WithSpaces_ReturnsDashSeparatedHex()
    {
        var data = new byte[] { 0x01, 0x02 };

        data.ToHexString(addSpaces: true).Should().Be("01-02");
    }

    [Fact]
    public void ToUInt16BigEndian_ValidOffset_ReturnsBigEndianValue()
    {
        // Bytes 0x01 0x02 → (0x01 << 8) | 0x02 = 258
        var data = new byte[] { 0x01, 0x02, 0x00, 0x00 };

        data.ToUInt16BigEndian(0).Should().Be(258);
    }

    [Fact]
    public void ToUInt16BigEndian_InvalidOffset_ThrowsArgumentException()
    {
        var data = new byte[] { 0x01 };

        var act = () => data.ToUInt16BigEndian(0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateXorChecksum_KnownBytes_ReturnsExpectedXorResult()
    {
        // 0x01 ^ 0x02 ^ 0x03 = 0x00
        var data = new byte[] { 0x01, 0x02, 0x03 };

        data.CalculateXorChecksum(0, 3).Should().Be(0x00);
    }

    [Fact]
    public void CalculateXorChecksum_SingleByte_ReturnsSameByte()
    {
        var data = new byte[] { 0xAB };

        data.CalculateXorChecksum(0, 1).Should().Be(0xAB);
    }

    [Fact]
    public void StartsWithMarker_MatchingPrefix_ReturnsTrue()
    {
        var data = new byte[] { 0x78, 0x78, 0x01, 0x02 };

        data.StartsWithMarker(0x78, 0x78).Should().BeTrue();
    }

    [Fact]
    public void StartsWithMarker_NonMatchingFirstByte_ReturnsFalse()
    {
        var data = new byte[] { 0x28, 0x78 };

        data.StartsWithMarker(0x78, 0x78).Should().BeFalse();
    }

    [Fact]
    public void IndexOfSequence_SequencePresent_ReturnsStartIndex()
    {
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        data.IndexOfSequence(new byte[] { 0x02, 0x03 }).Should().Be(1);
    }

    [Fact]
    public void IndexOfSequence_SequenceAbsent_ReturnsMinusOne()
    {
        var data = new byte[] { 0x01, 0x02, 0x03 };

        data.IndexOfSequence(new byte[] { 0x04, 0x05 }).Should().Be(-1);
    }

    [Fact]
    public void CopyRange_ValidRange_ReturnsCorrectSubset()
    {
        var data = new byte[] { 0x10, 0x20, 0x30, 0x40 };

        data.CopyRange(1, 2).Should().Equal(new byte[] { 0x20, 0x30 });
    }

    [Fact]
    public void ToAsciiString_ValidBytes_ReturnsDecodedString()
    {
        var data = System.Text.Encoding.ASCII.GetBytes("GT06");

        data.ToAsciiString(0, 4).Should().Be("GT06");
    }

    // ── StringExtensions ──────────────────────────────────────────────────────

    [Fact]
    public void IsValidImei_FifteenDigits_ReturnsTrue()
    {
        "123456789012345".IsValidImei().Should().BeTrue();
    }

    [Fact]
    public void IsValidImei_TooShortString_ReturnsFalse()
    {
        "12345".IsValidImei().Should().BeFalse();
    }

    [Fact]
    public void IsValidImei_ContainsNonDigit_ReturnsFalse()
    {
        "12345678901234A".IsValidImei().Should().BeFalse();
    }

    [Fact]
    public void IsValidDeviceId_AlphanumericWithDashUnderscore_ReturnsTrue()
    {
        "device-001_test".IsValidDeviceId().Should().BeTrue();
    }

    [Fact]
    public void IsValidDeviceId_ContainsAtSymbol_ReturnsFalse()
    {
        "device@001".IsValidDeviceId().Should().BeFalse();
    }

    [Fact]
    public void SanitizeDeviceId_InvalidChars_StripsThemOut()
    {
        "device@123!".SanitizeDeviceId().Should().Be("device123");
    }

    [Fact]
    public void SanitizeDeviceId_EmptyString_ReturnsUnknown()
    {
        string.Empty.SanitizeDeviceId().Should().Be("unknown");
    }

    [Fact]
    public void SanitizeDeviceId_AllInvalidChars_ReturnsUnknown()
    {
        "@@@!!!".SanitizeDeviceId().Should().Be("unknown");
    }

    [Fact]
    public void GetNmeaChecksum_SentenceWithChecksum_ReturnsChecksumPart()
    {
        "$GPRMC,123456,A*4A".GetNmeaChecksum().Should().Be("4A");
    }

    [Fact]
    public void RemoveNmeaChecksum_SentenceWithChecksum_RemovesStarAndBeyond()
    {
        "$GPRMC,123456*4A".RemoveNmeaChecksum().Should().Be("$GPRMC,123456");
    }

    [Fact]
    public void SplitNmea_CommaSeparatedSentence_ReturnsTrimmedFields()
    {
        var fields = "$GPRMC, 123456 ,A".SplitNmea();

        fields.Should().Equal("$GPRMC", "123456", "A");
    }

    [Fact]
    public void HexToByteArray_ValidHexString_ReturnsExpectedBytes()
    {
        "7878".HexToByteArray().Should().Equal(new byte[] { 0x78, 0x78 });
    }

    [Fact]
    public void HexToByteArray_HexWithDashes_StripsDelimitersBeforeParsing()
    {
        "78-78".HexToByteArray().Should().Equal(new byte[] { 0x78, 0x78 });
    }

    [Fact]
    public void HexToByteArray_OddLengthHex_ReturnsEmpty()
    {
        "ABC".HexToByteArray().Should().BeEmpty();
    }

    [Fact]
    public void Truncate_StringLongerThanMax_AppendsSuffix()
    {
        "HelloWorld".Truncate(7).Should().Be("Hell...");
    }

    [Fact]
    public void Truncate_StringShorterThanMax_ReturnsOriginal()
    {
        "Hi".Truncate(10).Should().Be("Hi");
    }

    // ── DateTimeExtensions ────────────────────────────────────────────────────

    [Fact]
    public void ToUnixTimestamp_Epoch_ReturnsZero()
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        epoch.ToUnixTimestamp().Should().Be(0);
    }

    [Fact]
    public void FromUnixTimestamp_Zero_ReturnsUnixEpoch()
    {
        var expected = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        DateTimeExtensions.FromUnixTimestamp(0).Should().Be(expected);
    }

    [Fact]
    public void RoundDown_ToFiveMinutes_FloorsToBoundary()
    {
        var dt = new DateTime(2024, 6, 15, 12, 37, 45, DateTimeKind.Utc);

        dt.RoundDown(TimeSpan.FromMinutes(5))
          .Should().Be(new DateTime(2024, 6, 15, 12, 35, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void RoundUp_ToFiveMinutes_CeilsToBoundary()
    {
        var dt = new DateTime(2024, 6, 15, 12, 32, 1, DateTimeKind.Utc);

        dt.RoundUp(TimeSpan.FromMinutes(5))
          .Should().Be(new DateTime(2024, 6, 15, 12, 35, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void IsSameDay_TwoTimesOnSameDate_ReturnsTrue()
    {
        var morning = new DateTime(2024, 6, 15, 8, 0, 0);
        var evening = new DateTime(2024, 6, 15, 22, 30, 0);

        morning.IsSameDay(evening).Should().BeTrue();
    }

    [Fact]
    public void IsSameDay_TwoTimesOnDifferentDates_ReturnsFalse()
    {
        var day1 = new DateTime(2024, 6, 15, 23, 59, 59);
        var day2 = new DateTime(2024, 6, 16, 0, 0, 0);

        day1.IsSameDay(day2).Should().BeFalse();
    }

    [Fact]
    public void GetStartOfDay_AnyTime_ReturnsMidnight()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);

        dt.GetStartOfDay().Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));
    }

    [Fact]
    public void GetStartOfMonth_MidMonth_ReturnsFirstDayMidnight()
    {
        var dt = new DateTime(2024, 6, 15);

        dt.GetStartOfMonth().Should().Be(new DateTime(2024, 6, 1));
    }

    [Fact]
    public void ToRelativeTime_FiveMinutesAgo_ReturnsMinutesAgoString()
    {
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

        fiveMinutesAgo.ToRelativeTime().Should().Be("5 minutes ago");
    }

    [Fact]
    public void ToRelativeTime_ThirtySecondsAgo_ReturnsJustNow()
    {
        var thirtySecondsAgo = DateTime.UtcNow.AddSeconds(-30);

        thirtySecondsAgo.ToRelativeTime().Should().Be("just now");
    }
}
