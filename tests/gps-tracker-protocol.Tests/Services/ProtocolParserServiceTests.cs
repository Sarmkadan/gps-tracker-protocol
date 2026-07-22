#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for ProtocolParserService
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GpsTrackerProtocol.Tests.Services;

public class ProtocolParserServiceTests
{
    private readonly IProtocolParserService _service;

    public ProtocolParserServiceTests()
    {
        _service = new ProtocolParserService();
    }

    #region GT06 Protocol Tests

    [Fact]
    public async Task ValidateFrameAsync_GT06_ValidChecksum_ReturnsTrue()
    {
        // Arrange - Minimal valid GT06 frame
        byte[] validGt06RawData = { 0x78, 0x78, 0x01, 0x01, 0x02, 0x02, 0x0D, 0x0A };
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = validGt06RawData
        };

        // Act
        var result = await _service.ValidateFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateFrameAsync_GT06_InvalidChecksum_ReturnsFalse()
    {
        // Arrange - GT06 frame with invalid checksum
        byte[] invalidGt06RawData = { 0x78, 0x78, 0x01, 0x01, 0x02, 0x03, 0x0D, 0x0A };
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = invalidGt06RawData
        };

        // Act
        var result = await _service.ValidateFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateFrameAsync_GT06_TooShortFrame_ReturnsFalse()
    {
        // Arrange - Frame too short to be valid
        byte[] tooShortRawData = { 0x78, 0x78, 0x01, 0x01, 0x02, 0x02, 0x0D };
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = tooShortRawData
        };

        // Act
        var result = await _service.ValidateFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DetectProtocolAsync_GT06_StartMarker_ReturnsGT06()
    {
        // Arrange
        byte[] gt06Data = { 0x78, 0x78, 0x1E, 0x01 };

        // Act
        var protocol = await _service.DetectProtocolAsync(gt06Data).ConfigureAwait(false);

        // Assert
        protocol.Should().Be(ProtocolType.GT06);
    }

    [Fact]
    public async Task DetectProtocolAsync_GT06_ExtendedStartMarker_ReturnsGT06()
    {
        // Arrange
        byte[] gt06ExtendedData = { 0x79, 0x79, 0x1E, 0x01 };

        // Act
        var protocol = await _service.DetectProtocolAsync(gt06ExtendedData).ConfigureAwait(false);

        // Assert
        protocol.Should().Be(ProtocolType.GT06);
    }

    [Fact]
    public async Task ParseFrameAsync_GT06_ValidFrame_ParsesSuccessfully()
    {
        // Arrange - Valid GT06 frame with proper checksum
        byte[] gt06Frame = new byte[]
        {
            0x78, 0x78, // Start markers
            0x1E,       // Length
            0x01,       // Protocol number
            0x30, 0x31, 0x32, 0x33, 0x34, // Device ID "01234"
            0x14, 0x01, 0x02, 0x03, 0x04, 0x05, // Timestamp bytes
            0x00, 0x00, 0x01, // Latitude
            0x00, 0x00, 0x02, // Longitude
            0x04,          // Status byte
            0x00, 0x01,    // Speed
            0x00, 0x64,    // Bearing
            0x08,          // Satellite count
            0x00,          // Checksum (will be calculated)
            0x0D, 0x0A      // Stop bytes
        };

        // Calculate checksum
        byte checksum = 0;
        for (int i = 2; i <= 29; i++)
        {
            checksum ^= gt06Frame[i];
        }
        gt06Frame[30] = checksum;

        var frame = new GpsFrame
        {
            FrameId = "test-gt06-valid",
            Protocol = ProtocolType.GT06,
            RawData = gt06Frame,
            ReceivedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            SourceAddress = "127.0.0.1",
            SourcePort = 12345,
            IsValidChecksum = true
        };

        // Act
        var result = await _service.ParseFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.DeviceId.Should().Be("01234");
        result.Protocol.Should().Be(ProtocolType.GT06);
    }

    [Fact]
    public async Task ParseFrameAsync_GT06_InvalidChecksum_ThrowsParseException()
    {
        // Arrange - Frame with invalid checksum
        byte[] invalidFrame = new byte[]
        {
            0x78, 0x78, 0x1E, 0x01, 0x30, 0x31, 0x32, 0x33, 0x34,
            0x14, 0x01, 0x02, 0x03, 0x04, 0x05,
            0x00, 0x00, 0x01,
            0x00, 0x00, 0x02,
            0x04,
            0x00, 0x01,
            0x00, 0x64,
            0x08,
            0xFF, // Wrong checksum
            0x0D, 0x0A
        };

        var frame = new GpsFrame
        {
            FrameId = "test-gt06-invalid",
            Protocol = ProtocolType.GT06,
            RawData = invalidFrame,
            IsValidChecksum = false
        };

        // Act & Assert
        Func<Task> act = async () => await _service.ParseFrameAsync(frame).ConfigureAwait(false);
        await act.Should().ThrowAsync<ParseException>();
    }

    #endregion

    #region H02 Protocol Tests

    [Fact]
    public async Task ValidateFrameAsync_H02_AlwaysReturnsTrue()
    {
        // Arrange
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.H02,
            RawData = Encoding.ASCII.GetBytes("$GPRMC,010000.00,A,2824.237,N,08100.237,W,0.00,0.0,010100,0,E*68")
        };

        // Act
        var result = await _service.ValidateFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DetectProtocolAsync_H02_GPRMC_ReturnsH02()
    {
        // Arrange
        byte[] h02Data = Encoding.ASCII.GetBytes("$GPRMC,123456.00,A,5133.81,N,00042.25,W");

        // Act
        var protocol = await _service.DetectProtocolAsync(h02Data).ConfigureAwait(false);

        // Assert
        protocol.Should().Be(ProtocolType.H02);
    }

    [Fact]
    public async Task DetectProtocolAsync_H02_HQ_ReturnsH02()
    {
        // Arrange
        byte[] h02Data = Encoding.ASCII.GetBytes("*HQ,123456789012345,V1,123456");

        // Act
        var protocol = await _service.DetectProtocolAsync(h02Data).ConfigureAwait(false);

        // Assert
        protocol.Should().Be(ProtocolType.H02);
    }

    [Fact]
    public async Task ParseFrameAsync_H02_GPRMC_ValidFrame_ParsesSuccessfully()
    {
        // Arrange
        string nmeaFrame = "$GPRMC,235947.00,A,5133.81,N,00042.25,W,0.13,309.62,120598,,,A*6C";
        var frame = new GpsFrame
        {
            FrameId = "test-h02-gprmc",
            Protocol = ProtocolType.H02,
            RawData = Encoding.ASCII.GetBytes(nmeaFrame),
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = "127.0.0.1",
            SourcePort = 12345,
            IsValidChecksum = true
        };

        // Act
        var result = await _service.ParseFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Protocol.Should().Be(ProtocolType.H02);
        result.Latitude.Should().BeApproximately(51.5635, 0.0001);
        result.Longitude.Should().BeApproximately(-0.7041667, 0.00001);
    }

    [Fact]
    public async Task ParseFrameAsync_H02_HQ_ValidFrame_ParsesSuccessfully()
    {
        // Arrange
        string hqFrame = "*HQ,123456789012345,V1,123456,5133.81,N,00042.25,W,0.13,309.62,120598,,,A";
        var frame = new GpsFrame
        {
            FrameId = "test-h02-hq",
            Protocol = ProtocolType.H02,
            RawData = Encoding.ASCII.GetBytes(hqFrame),
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = "127.0.0.1",
            SourcePort = 12345,
            IsValidChecksum = true
        };

        // Act
        var result = await _service.ParseFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.DeviceId.Should().Be("123456789012345");
        result.Protocol.Should().Be(ProtocolType.H02);
    }

    #endregion

    #region TK103 Protocol Tests

    [Fact]
    public async Task ValidateFrameAsync_TK103_AlwaysReturnsTrue()
    {
        // Arrange
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.TK103,
            RawData = Encoding.ASCII.GetBytes("123456789012345,20240101123045,4812.3456,N,01623.4567,E,45.5,123.4")
        };

        // Act
        var result = await _service.ValidateFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DetectProtocolAsync_TK103_StartMarker_ReturnsTK103()
    {
        // Arrange
        byte[] tk103Data = { 0x28 };

        // Act
        var protocol = await _service.DetectProtocolAsync(tk103Data).ConfigureAwait(false);

        // Assert
        protocol.Should().Be(ProtocolType.TK103);
    }

    [Fact]
    public async Task ParseFrameAsync_TK103_ValidFrame_ParsesSuccessfully()
    {
        // Arrange
        string tk103Frame = "123456789012345,20240101123045,4812.3456,N,01623.4567,E,45.5,123.4";
        var frame = new GpsFrame
        {
            FrameId = "test-tk103-valid",
            Protocol = ProtocolType.TK103,
            RawData = Encoding.ASCII.GetBytes(tk103Frame),
            ReceivedAt = DateTime.UtcNow,
            SourceAddress = "127.0.0.1",
            SourcePort = 12345,
            IsValidChecksum = true
        };

        // Act
        var result = await _service.ParseFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.DeviceId.Should().Be("123456789012345");
        result.Protocol.Should().Be(ProtocolType.TK103);
        result.Latitude.Should().BeApproximately(48.20576, 0.00001);
        result.Longitude.Should().BeApproximately(16.390945, 0.00001);
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task ParseFrameAsync_UnsupportedProtocol_ThrowsParseException()
    {
        // Arrange
        byte[] unsupportedData = { 0xFF, 0xFF };
        var frame = new GpsFrame
        {
            FrameId = "test-unsupported",
            Protocol = ProtocolType.Unknown,
            RawData = unsupportedData,
            IsValidChecksum = true
        };

        // Act & Assert
        Func<Task> act = async () => await _service.ParseFrameAsync(frame).ConfigureAwait(false);
        await act.Should().ThrowAsync<ParseException>();
    }

    [Fact]
    public async Task DetectProtocolAsync_EmptyData_ReturnsUnknown()
    {
        // Arrange
        byte[] emptyData = Array.Empty<byte>();

        // Act
        var protocol = await _service.DetectProtocolAsync(emptyData).ConfigureAwait(false);

        // Assert
        protocol.Should().Be(ProtocolType.Unknown);
    }

    [Fact]
    public async Task ValidateFrameAsync_EmptyFrame_ReturnsFalse()
    {
        // Arrange
        var frame = new GpsFrame
        {
            RawData = Array.Empty<byte>()
        };

        // Act
        var result = await _service.ValidateFrameAsync(frame).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}