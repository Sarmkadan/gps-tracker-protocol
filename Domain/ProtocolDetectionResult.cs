#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Protocol detection result type for handling ambiguous frame prefixes
// =============================================================================

namespace GpsTrackerProtocol.Domain;

/// <summary>
/// Represents the result of protocol detection with different confidence levels.
/// </summary>
public enum ProtocolDetectionResult
{
    /// <summary>
    /// Detection is conclusive - the protocol is definitely identified.
    /// </summary>
    Detected = 0,

    /// <summary>
    /// Detection is ambiguous - multiple protocols could match the available data.
    /// More bytes are needed to make a definitive decision.
    /// </summary>
    Ambiguous = 1,

    /// <summary>
    /// No protocol signature detected in the available data.
    /// </summary>
    Unknown = 2,

    /// <summary>
    /// Insufficient data to make any determination (data too short).
    /// </summary>
    NeedMoreData = 3
}

/// <summary>
/// Extended protocol detection result that includes the detected protocol type
/// when detection is conclusive, or a list of possible protocols when ambiguous.
/// </summary>
public readonly record struct ProtocolDetection
{
    /// <summary>
    /// The detection result status.
    /// </summary>
    public ProtocolDetectionResult Result { get; init; }

    /// <summary>
    /// The detected protocol type (only valid when Result is Detected).
    /// </summary>
    public ProtocolType DetectedProtocol { get; init; }

    /// <summary>
    /// List of possible protocols when Result is Ambiguous.
    /// </summary>
    public IReadOnlyList<ProtocolType>? PossibleProtocols { get; init; }

    /// <summary>
    /// The number of bytes that were inspected during detection.
    /// </summary>
    public int BytesInspected { get; init; }

    /// <summary>
    /// Creates a conclusive detection result.
    /// </summary>
    /// <param name="protocol">The detected protocol type.</param>
    /// <param name="bytesInspected">Number of bytes inspected.</param>
    /// <returns>A ProtocolDetection with Detected result.</returns>
    public static ProtocolDetection Detected(ProtocolType protocol, int bytesInspected)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bytesInspected, 1);
        return new ProtocolDetection
        {
            Result = ProtocolDetectionResult.Detected,
            DetectedProtocol = protocol,
            BytesInspected = bytesInspected
        };
    }

    /// <summary>
    /// Creates an ambiguous detection result.
    /// </summary>
    /// <param name="possibleProtocols">List of possible protocols.</param>
    /// <param name="bytesInspected">Number of bytes inspected.</param>
    /// <returns>A ProtocolDetection with Ambiguous result.</returns>
    public static ProtocolDetection Ambiguous(IReadOnlyList<ProtocolType> possibleProtocols, int bytesInspected)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bytesInspected, 1);
        ArgumentNullException.ThrowIfNull(possibleProtocols);
        if (possibleProtocols.Count == 0)
            throw new ArgumentException("At least one possible protocol must be provided", nameof(possibleProtocols));

        return new ProtocolDetection
        {
            Result = ProtocolDetectionResult.Ambiguous,
            PossibleProtocols = possibleProtocols,
            BytesInspected = bytesInspected
        };
    }

    /// <summary>
    /// Creates an unknown detection result.
    /// </summary>
    /// <param name="bytesInspected">Number of bytes inspected.</param>
    /// <returns>A ProtocolDetection with Unknown result.</returns>
    public static ProtocolDetection Unknown(int bytesInspected)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bytesInspected, 0);
        return new ProtocolDetection
        {
            Result = ProtocolDetectionResult.Unknown,
            BytesInspected = bytesInspected
        };
    }

    /// <summary>
    /// Creates a "need more data" detection result.
    /// </summary>
    /// <param name="bytesAvailable">Total bytes available.</param>
    /// <param name="bytesRequired">Minimum bytes required for detection.</param>
    /// <returns>A ProtocolDetection with NeedMoreData result.</returns>
    public static ProtocolDetection NeedMoreData(int bytesAvailable, int bytesRequired)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bytesAvailable);
        ArgumentOutOfRangeException.ThrowIfNegative(bytesRequired);
        return new ProtocolDetection
        {
            Result = ProtocolDetectionResult.NeedMoreData,
            BytesInspected = bytesAvailable,
            PossibleProtocols = bytesRequired > 0 ?
                [ProtocolType.GT06, ProtocolType.H02, ProtocolType.TK103] :
                null
        };
    }

    /// <summary>
    /// Gets the protocol type if detection is conclusive.
    /// </summary>
    /// <param name="protocol">The protocol type output parameter.</param>
    /// <returns>True if Result is Detected, false otherwise.</returns>
    public bool TryGetProtocol(out ProtocolType protocol)
    {
        if (Result == ProtocolDetectionResult.Detected)
        {
            protocol = DetectedProtocol;
            return true;
        }

        protocol = ProtocolType.Unknown;
        return false;
    }
}
