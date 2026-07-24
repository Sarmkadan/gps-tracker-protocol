#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// GT06 Response Builder unit tests
// =====================================================================

namespace GpsTrackerProtocol.Tests.Parsers;

using Xunit;

using GpsTrackerProtocol.Parsers;
using GpsTrackerProtocol.Domain;

/// <summary>
/// Unit tests for Gt06ResponseBuilder.
/// Tests acknowledgment frame generation and validation.
/// </summary>
public class Gt06ResponseBuilderTests
{
    [Fact]
    public void CreateLoginAck_WithValidParameters_ReturnsCorrectFrame()
    {
        // Arrange
        string deviceId = "ABCDE";
        ushort serialNumber = 0x1234;
        bool useExtendedFormat = false;

        // Act
        byte[] frame = Gt06ResponseBuilder.CreateLoginAck(deviceId, serialNumber, useExtendedFormat);

        // Assert
        Assert.NotNull(frame);
        Assert.Equal(15, frame.Length); // 2 start + 1 length + 1 protocol + 1 msgType + 2 serial + 5 deviceId + 1 crc + 2 end

        // Verify start markers
        Assert.Equal(ProtocolConstants.GT06_START_MARKER, frame[0]);
        Assert.Equal(ProtocolConstants.GT06_START_MARKER, frame[1]);

        // Verify packet length
        Assert.Equal(0x0B, frame[2]);

        // Verify protocol number (0x91 for ACK)
        Assert.Equal(0x91, frame[3]);

        // Verify message type (0x01 for login ACK)
        Assert.Equal(0x01, frame[4]);

        // Verify serial number (big-endian)
        Assert.Equal(0x12, frame[5]);
        Assert.Equal(0x34, frame[6]);

        // Verify device ID
        Assert.Equal((byte)'A', frame[7]);
        Assert.Equal((byte)'B', frame[8]);
        Assert.Equal((byte)'C', frame[9]);
        Assert.Equal((byte)'D', frame[10]);
        Assert.Equal((byte)'E', frame[11]);

        // Verify end markers
        Assert.Equal(ProtocolConstants.GT06_END_MARKER, frame[13]);
        Assert.Equal(0x0A, frame[14]);
    }

    [Fact]
    public void CreateHeartbeatAck_WithValidParameters_ReturnsCorrectFrame()
    {
        // Arrange
        string deviceId = "TEST1";
        ushort serialNumber = 0x5678;
        bool useExtendedFormat = false;

        // Act
        byte[] frame = Gt06ResponseBuilder.CreateHeartbeatAck(deviceId, serialNumber, useExtendedFormat);

        // Assert
        Assert.NotNull(frame);
        Assert.Equal(15, frame.Length);

        // Verify message type (0x02 for heartbeat ACK)
        Assert.Equal(0x02, frame[4]);

        // Verify serial number
        Assert.Equal(0x56, frame[5]);
        Assert.Equal(0x78, frame[6]);

        // Verify device ID
        Assert.Equal((byte)'T', frame[7]);
        Assert.Equal((byte)'E', frame[8]);
        Assert.Equal((byte)'S', frame[9]);
        Assert.Equal((byte)'T', frame[10]);
        Assert.Equal((byte)'1', frame[11]);
    }

    [Fact]
    public void CreateAck_WithExtendedFormat_ReturnsCorrectFrame()
    {
        // Arrange
        string deviceId = "EXTEN";
        ushort serialNumber = 0xABCD;
        byte messageType = 0x05;
        bool useExtendedFormat = true;

        // Act
        byte[] frame = Gt06ResponseBuilder.CreateAck(deviceId, serialNumber, messageType, useExtendedFormat);

        // Assert
        Assert.NotNull(frame);
        Assert.Equal(15, frame.Length);

        // Verify extended start markers
        Assert.Equal(ProtocolConstants.GT06_EXTENDED_START_MARKER, frame[0]);
        Assert.Equal(ProtocolConstants.GT06_EXTENDED_START_MARKER, frame[1]);

        // Verify message type is echoed
        Assert.Equal(messageType, frame[4]);

        // Verify serial number
        Assert.Equal(0xAB, frame[5]);
        Assert.Equal(0xCD, frame[6]);
    }

    [Fact]
    public void CreateLoginAck_WithNullDeviceId_ThrowsArgumentNullException()
    {
        // Arrange
        string? deviceId = null;
        ushort serialNumber = 0x1234;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Gt06ResponseBuilder.CreateLoginAck(deviceId!, serialNumber));
    }

    [Fact]
    public void CreateLoginAck_WithEmptyDeviceId_ThrowsArgumentException()
    {
        // Arrange
        string deviceId = string.Empty;
        ushort serialNumber = 0x1234;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Gt06ResponseBuilder.CreateLoginAck(deviceId, serialNumber));
    }

    [Fact]
    public void CreateLoginAck_WithInvalidDeviceIdLength_ThrowsArgumentException()
    {
        // Arrange
        string deviceId = "ABC"; // Too short
        ushort serialNumber = 0x1234;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Gt06ResponseBuilder.CreateLoginAck(deviceId, serialNumber));

        // Arrange
        deviceId = "ABCDEFGHIJ"; // Too long

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Gt06ResponseBuilder.CreateLoginAck(deviceId, serialNumber));
    }

    [Fact]
    public void IsAckFrame_WithValidAckFrame_ReturnsTrue()
    {
        // Arrange
        string deviceId = "VALID";
        ushort serialNumber = 0x1234;
        byte[] frame = Gt06ResponseBuilder.CreateLoginAck(deviceId, serialNumber);

        // Act
        bool isValid = Gt06ResponseBuilder.IsAckFrame(frame);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsAckFrame_WithInvalidFrame_ReturnsFalse()
    {
        // Arrange - invalid start marker
        byte[] invalidFrame1 = [0x78, 0x79, 0x0B, 0x91, 0x01, 0x12, 0x34, 0x56, 0x49, 0x44, 0x00, 0x00, 0x00, 0x0D, 0x0A];

        // Act
        bool isValid1 = Gt06ResponseBuilder.IsAckFrame(invalidFrame1);

        // Assert
        Assert.False(isValid1);

        // Arrange - mismatched start markers
        byte[] invalidFrame2 = [0x78, 0x79, 0x0B, 0x91, 0x01, 0x12, 0x34, 0x56, 0x49, 0x44, 0x00, 0x00, 0x00, 0x0D, 0x0A];

        // Act
        bool isValid2 = Gt06ResponseBuilder.IsAckFrame(invalidFrame2);

        // Assert
        Assert.False(isValid2);

        // Arrange - invalid end markers
        byte[] invalidFrame3 = [0x78, 0x78, 0x0B, 0x91, 0x01, 0x12, 0x34, 0x56, 0x49, 0x44, 0x00, 0x00, 0x00, 0x0D, 0x0B];

        // Act
        bool isValid3 = Gt06ResponseBuilder.IsAckFrame(invalidFrame3);

        // Assert
        Assert.False(isValid3);
    }

    [Fact]
    public void IsAckFrame_WithShortFrame_ReturnsFalse()
    {
        // Arrange
        byte[] shortFrame = [0x78, 0x78];

        // Act
        bool isValid = Gt06ResponseBuilder.IsAckFrame(shortFrame);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ExtractSerialNumber_WithValidAckFrame_ReturnsCorrectValue()
    {
        // Arrange
        string deviceId = "ABCDE";
        ushort expectedSerial = 0x9ABC;
        byte[] frame = Gt06ResponseBuilder.CreateLoginAck(deviceId, expectedSerial);

        // Act
        ushort actualSerial = Gt06ResponseBuilder.ExtractSerialNumber(frame);

        // Assert
        Assert.Equal(expectedSerial, actualSerial);
    }

    [Fact]
    public void ExtractSerialNumber_WithInvalidFrame_ReturnsZero()
    {
        // Arrange
        byte[] invalidFrame = [0x78, 0x78, 0x0B, 0x90, 0x01, 0x12, 0x34, 0x56, 0x49, 0x44, 0x00, 0x00, 0x00, 0x0D, 0x0A];

        // Act
        ushort serial = Gt06ResponseBuilder.ExtractSerialNumber(invalidFrame);

        // Assert
        Assert.Equal(0, serial);
    }

    [Fact]
    public void ExtractDeviceId_WithValidAckFrame_ReturnsCorrectValue()
    {
        // Arrange
        string expectedDeviceId = "TEST1";
        ushort serialNumber = 0x1234;
        byte[] frame = Gt06ResponseBuilder.CreateHeartbeatAck(expectedDeviceId, serialNumber);

        // Act
        string actualDeviceId = Gt06ResponseBuilder.ExtractDeviceId(frame);

        // Assert
        Assert.Equal(expectedDeviceId, actualDeviceId);
    }

    [Fact]
    public void ExtractDeviceId_WithInvalidFrame_ReturnsEmptyString()
    {
        // Arrange
        byte[] invalidFrame = [0x78, 0x78, 0x0B, 0x90, 0x01, 0x12, 0x34, 0x56, 0x49, 0x44, 0x00, 0x00, 0x00, 0x0D, 0x0A];

        // Act
        string deviceId = Gt06ResponseBuilder.ExtractDeviceId(invalidFrame);

        // Assert
        Assert.Empty(deviceId);
    }

    [Fact]
    public void ExtractAcknowledgedMessageType_WithValidAckFrame_ReturnsCorrectValue()
    {
        // Arrange
        string deviceId = "ABCDE";
        ushort serialNumber = 0x1234;
        byte expectedMessageType = 0x02; // Heartbeat ACK
        byte[] frame = Gt06ResponseBuilder.CreateAck(deviceId, serialNumber, expectedMessageType);

        // Act
        byte actualMessageType = Gt06ResponseBuilder.ExtractAcknowledgedMessageType(frame);

        // Assert
        Assert.Equal(expectedMessageType, actualMessageType);
    }

    [Fact]
    public void RoundTrip_BuildResponseThenValidate_ReturnsTrue()
    {
        // Arrange
        string deviceId = "GT061";
        ushort serialNumber = 0x4321;

        // Act 1: Build response
        byte[] responseFrame = Gt06ResponseBuilder.CreateLoginAck(deviceId, serialNumber);

        // Act 2: Validate the response
        bool isValid = Gt06ResponseBuilder.IsAckFrame(responseFrame);

        // Assert
        Assert.True(isValid);
        Assert.Equal(15, responseFrame.Length);
    }

    [Fact]
    public void RoundTrip_BuildResponseThenExtractSerial_ReturnsOriginalSerial()
    {
        // Arrange
        string deviceId = "SRL01";
        ushort originalSerial = 0x8765;

        // Act 1: Build response
        byte[] responseFrame = Gt06ResponseBuilder.CreateHeartbeatAck(deviceId, originalSerial);

        // Act 2: Extract serial from response
        ushort extractedSerial = Gt06ResponseBuilder.ExtractSerialNumber(responseFrame);

        // Assert
        Assert.Equal(originalSerial, extractedSerial);
    }

    [Fact]
    public void RoundTrip_BuildResponseThenExtractDeviceId_ReturnsOriginalDeviceId()
    {
        // Arrange
        string originalDeviceId = "DEV01";
        ushort serialNumber = 0x1111;

        // Act 1: Build response
        byte[] responseFrame = Gt06ResponseBuilder.CreateLoginAck(originalDeviceId, serialNumber);

        // Act 2: Extract device ID from response
        string extractedDeviceId = Gt06ResponseBuilder.ExtractDeviceId(responseFrame);

        // Assert
        Assert.Equal(originalDeviceId, extractedDeviceId);
    }


    [Fact]
    public void CreateAck_WithDifferentMessageTypes_CreatesDistinctFrames()
    {
        // Arrange
        string deviceId = "TEST1";
        ushort serialNumber = 0x1234;

        // Act
        byte[] frame1 = Gt06ResponseBuilder.CreateAck(deviceId, serialNumber, 0x01); // Login
        byte[] frame2 = Gt06ResponseBuilder.CreateAck(deviceId, serialNumber, 0x02); // Heartbeat
        byte[] frame3 = Gt06ResponseBuilder.CreateAck(deviceId, serialNumber, 0x05); // General

        // Assert
        Assert.Equal(0x01, frame1[4]);
        Assert.Equal(0x02, frame2[4]);
        Assert.Equal(0x05, frame3[4]);

        Assert.True(Gt06ResponseBuilder.IsAckFrame(frame1));
        Assert.True(Gt06ResponseBuilder.IsAckFrame(frame2));
        Assert.True(Gt06ResponseBuilder.IsAckFrame(frame3));
    }
}
