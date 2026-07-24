#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// GT06 protocol response frame builder implementation
// =====================================================================

namespace GpsTrackerProtocol.Parsers;

using System.Buffers.Binary;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Builds GT06 protocol acknowledgment response frames.
/// GT06 devices require the server to echo an ACK frame for login and heartbeat packets.
/// Without acknowledgment, the device disconnects and retries in a loop.
/// </summary>
public static class Gt06ResponseBuilder
{
    /// <summary>
    /// Creates a login acknowledgment response frame.
    /// </summary>
    /// <param name="deviceId">Device identifier (5 ASCII characters)</param>
    /// <param name="serialNumber">Message serial number to echo (2 bytes)</param>
    /// <param name="useExtendedFormat">Whether to use extended format (0x79) or standard (0x78)</param>
    /// <returns>Complete GT06 ACK frame as byte array</returns>
    /// <exception cref="ArgumentNullException">Thrown when deviceId is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when deviceId length is not 5 characters</exception>
    public static byte[] CreateLoginAck(string deviceId, ushort serialNumber, bool useExtendedFormat = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(deviceId);
        if (deviceId.Length != 5)
        {
            throw new ArgumentException("Device ID must be exactly 5 characters", nameof(deviceId));
        }

        return CreateAck(deviceId, serialNumber, 0x01, useExtendedFormat);
    }

    /// <summary>
    /// Creates a heartbeat acknowledgment response frame.
    /// </summary>
    /// <param name="deviceId">Device identifier (5 ASCII characters)</param>
    /// <param name="serialNumber">Message serial number to echo (2 bytes)</param>
    /// <param name="useExtendedFormat">Whether to use extended format (0x79) or standard (0x78)</param>
    /// <returns>Complete GT06 ACK frame as byte array</returns>
    /// <exception cref="ArgumentNullException">Thrown when deviceId is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when deviceId length is not 5 characters</exception>
    public static byte[] CreateHeartbeatAck(string deviceId, ushort serialNumber, bool useExtendedFormat = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(deviceId);
        if (deviceId.Length != 5)
        {
            throw new ArgumentException("Device ID must be exactly 5 characters", nameof(deviceId));
        }

        return CreateAck(deviceId, serialNumber, 0x02, useExtendedFormat);
    }

    /// <summary>
    /// Creates a general acknowledgment response frame for any message type.
    /// </summary>
    /// <param name="deviceId">Device identifier (5 ASCII characters)</param>
    /// <param name="serialNumber">Message serial number to echo (2 bytes)</param>
    /// <param name="messageType">Original message type being acknowledged</param>
    /// <param name="useExtendedFormat">Whether to use extended format (0x79) or standard (0x78)</param>
    /// <returns>Complete GT06 ACK frame as byte array</returns>
    /// <exception cref="ArgumentNullException">Thrown when deviceId is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when deviceId length is not 5 characters</exception>
    public static byte[] CreateAck(string deviceId, ushort serialNumber, byte messageType, bool useExtendedFormat = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(deviceId);
        if (deviceId.Length != 5)
        {
            throw new ArgumentException("Device ID must be exactly 5 characters", nameof(deviceId));
        }

        // GT06 ACK frame structure:
        // 0-1:  Start markers (0x78 0x78 or 0x79 0x79)
        // 2:    Packet length (total bytes from index 3 to before checksum)
        // 3:    Protocol number (0x91 for ACK responses)
        // 4:    Message type being acknowledged (echoed from original message)
        // 5-6:  Serial number (echoed from original message, 2 bytes big-endian)
        // 7-11: Device ID (ASCII encoded, 5 characters)
        // 12:   Checksum (CRC-ITU)
        // 13:   End marker (0x0D)
        // 14:   End marker (0x0A)

        const int frameSize = 15;
        var frame = new byte[frameSize];

        // Start markers
        frame[0] = useExtendedFormat ? ProtocolConstants.GT06_EXTENDED_START_MARKER : ProtocolConstants.GT06_START_MARKER;
        frame[1] = frame[0];

        // Packet length (bytes from index 3 to before checksum = 11 bytes: protocol(1) + msgType(1) + serial(2) + deviceId(5))
        frame[2] = 0x0B;

        // Protocol number (0x91 for ACK responses)
        frame[3] = 0x91;

        // Message type being acknowledged
        frame[4] = messageType;

        // Serial number (2 bytes, big-endian)
        frame[5] = (byte)((serialNumber >> 8) & 0xFF);
        frame[6] = (byte)(serialNumber & 0xFF);

        // Device ID (5 ASCII characters)
        for (int i = 0; i < 5; i++)
        {
            frame[7 + i] = (byte)deviceId[i];
        }

        // Calculate XOR checksum (same as GT06 protocol parser validation)
        // Calculate from packet length (index 2) through device ID (index 11)
        byte checksum = CalculateXorChecksum(frame.AsSpan(2, 10)); // 10 bytes: length(1) + protocol(1) + msgType(1) + serial(2) + deviceId(5)
        frame[12] = checksum;

        // End markers
        frame[13] = ProtocolConstants.GT06_END_MARKER;
        frame[14] = 0x0A;

        return frame;
    }

    /// <summary>
    /// Calculates XOR checksum for GT06 protocol (same as Gt06ProtocolParser.ValidateChecksum).
    /// </summary>
    /// <param name="data">Data to calculate checksum for</param>
    /// <returns>XOR checksum byte</returns>
    private static byte CalculateXorChecksum(ReadOnlySpan<byte> data)
    {
        byte checksum = 0;
        foreach (byte b in data)
        {
            checksum ^= b;
        }
        return checksum;
    }

    /// <summary>
    /// Validates that a GT06 frame is a valid acknowledgment frame.
    /// </summary>
    /// <param name="frameData">Frame data to validate</param>
    /// <returns>True if frame is a valid ACK frame, false otherwise</returns>
    public static bool IsAckFrame(ReadOnlySpan<byte> frameData)
    {
        try
        {
            // Validate minimum frame size (15 bytes)
            if (frameData.Length < 15)
            {
                return false;
            }

            // Validate start markers
            if (frameData[0] != ProtocolConstants.GT06_START_MARKER &&
                frameData[0] != ProtocolConstants.GT06_EXTENDED_START_MARKER)
            {
                return false;
            }

            if (frameData[1] != frameData[0])
            {
                return false;
            }

            // Validate stop markers
            if (frameData[13] != ProtocolConstants.GT06_END_MARKER || frameData[14] != 0x0A)
            {
                return false;
            }

            // Validate packet length
            if (frameData[2] != 0x0C)
            {
                return false;
            }

            // Validate protocol number (must be 0x91 for ACK)
            if (frameData[3] != 0x91)
            {
                return false;
            }

            // Validate checksum (XOR checksum)
            byte expectedChecksum = frameData[12];
            byte calculatedChecksum = CalculateXorChecksum(frameData.Slice(2, 10));

            return calculatedChecksum == expectedChecksum;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts serial number from an ACK frame.
    /// </summary>
    /// <param name="frameData">ACK frame data</param>
    /// <returns>Extracted serial number, or 0 if frame is invalid</returns>
    public static ushort ExtractSerialNumber(ReadOnlySpan<byte> frameData)
    {
        try
        {
            if (!IsAckFrame(frameData))
            {
                return 0;
            }

            // Serial number is at bytes 5-6 (2 bytes, big-endian)
            return (ushort)((frameData[5] << 8) | frameData[6]);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Extracts device ID from an ACK frame.
    /// </summary>
    /// <param name="frameData">ACK frame data</param>
    /// <returns>Extracted device ID as string, or empty string if frame is invalid</returns>
    public static string ExtractDeviceId(ReadOnlySpan<byte> frameData)
    {
        try
        {
            if (!IsAckFrame(frameData))
            {
                return string.Empty;
            }

            // Device ID is 5 ASCII characters at bytes 7-11
            Span<char> deviceId = stackalloc char[5];
            for (int i = 0; i < 5; i++)
            {
                deviceId[i] = (char)frameData[7 + i];
            }

            return new string(deviceId);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Extracts the acknowledged message type from an ACK frame.
    /// </summary>
    /// <param name="frameData">ACK frame data</param>
    /// <returns>Acknowledged message type, or 0 if frame is invalid</returns>
    public static byte ExtractAcknowledgedMessageType(ReadOnlySpan<byte> frameData)
    {
        try
        {
            if (!IsAckFrame(frameData))
            {
                return 0;
            }

            // Message type is at byte 4
            return frameData[4];
        }
        catch
        {
            return 0;
        }
    }
}
