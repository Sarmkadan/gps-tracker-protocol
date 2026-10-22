#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Configuration;

/// <summary>
/// Configuration for the TCP/UDP listener in <c>RealTimeGpsServer</c>.
/// Values are checked by <see cref="GpsListenerOptionsValidator"/> at startup.
/// </summary>
public class GpsListenerOptions
{
    /// <summary>
    /// Gets or sets the TCP port that tracker connections are accepted on.
    /// Range: 1 to 65535.
    /// </summary>
    public int TcpPort { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the UDP port that tracker datagrams are received on.
    /// Range: 1 to 65535.
    /// </summary>
    public int UdpPort { get; set; } = 5001;

    /// <summary>
    /// Gets or sets the maximum number of simultaneous TCP client connections.
    /// Connections beyond this limit are closed immediately. Must be greater than 0.
    /// </summary>
    public int MaxConnections { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the size in bytes of the per-connection receive buffer.
    /// Range: 64 to 65536 bytes.
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 1024;

    /// <summary>
    /// Gets or sets how long a TCP connection may stay silent before it is closed.
    /// Must be greater than zero and no more than about 24 days.
    /// </summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Returns a string summary of the listener settings.
    /// </summary>
    public override string ToString() =>
        $"GpsListenerOptions {{ TcpPort = {TcpPort}, UdpPort = {UdpPort}, MaxConnections = {MaxConnections}, " +
        $"ReceiveBufferSize = {ReceiveBufferSize}, IdleTimeout = {IdleTimeout} }}";
}
