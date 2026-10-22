#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Real-time TCP/UDP GPS server that listens for incoming tracker connections,
/// parses frames, and stores location data in real-time.
/// </summary>
public class RealTimeGpsServer
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<RealTimeGpsServer> _logger;
    private readonly IDeviceService _deviceService;
    private readonly ILocationDataService _locationService;
    private readonly IProtocolParserService _parserService;
    private readonly GpsListenerOptions _options;
    private readonly SemaphoreSlim _connectionSlots;
    private TcpListener? _tcpListener;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Creates a server with default <see cref="GpsListenerOptions"/>.
    /// </summary>
    public RealTimeGpsServer()
        : this(new GpsListenerOptions())
    {
    }

    /// <summary>
    /// Creates a server with the given listener options.
    /// </summary>
    /// <param name="options">Listener settings; validated before anything is bound.</param>
    /// <exception cref="OptionsValidationException">
    /// Thrown when one or more listener options are out of range. The exception lists every violation.
    /// </exception>
    public RealTimeGpsServer(GpsListenerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var validation = new GpsListenerOptionsValidator().Validate(null, options);
        if (validation.Failed)
        {
            throw new OptionsValidationException(
                nameof(GpsListenerOptions), typeof(GpsListenerOptions), validation.Failures);
        }

        _options = options;
        _connectionSlots = new SemaphoreSlim(options.MaxConnections, options.MaxConnections);

        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<RealTimeGpsServer>>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();
        _parserService = _provider.GetRequiredService<IProtocolParserService>();
    }

    /// <summary>Starts listening on the ports from the constructor's <see cref="GpsListenerOptions"/></summary>
    public Task StartAsync() => StartAsync(_options.TcpPort, _options.UdpPort);

    /// <summary>Starts listening for GPS data on TCP and UDP ports</summary>
    public async Task StartAsync(int tcpPort = 5000, int udpPort = 5001)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _logger.LogInformation("Starting GPS Tracker Server on TCP:{0} UDP:{1}", tcpPort, udpPort);

        var tcpTask = ListenTcpAsync(tcpPort, _cancellationTokenSource.Token);
        var udpTask = ListenUdpAsync(udpPort, _cancellationTokenSource.Token);

        await Task.WhenAll(tcpTask, udpTask).ConfigureAwait(false);
    }

    /// <summary>Listens for TCP connections and processes GPS frames</summary>
    private async Task ListenTcpAsync(int port, CancellationToken cancellationToken)
    {
        _tcpListener = new TcpListener(IPAddress.Any, port);
        _tcpListener.Start();
        _logger.LogInformation("TCP listener started on port {0}", port);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                if (!_connectionSlots.Wait(0))
                {
                    _logger.LogWarning("Connection limit of {0} reached; rejecting client", _options.MaxConnections);
                    client.Dispose();
                    continue;
                }

                _ = ProcessTcpClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("TCP listener stopped");
        }
    }

    /// <summary>Processes a single TCP client connection</summary>
    private async Task ProcessTcpClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var remoteEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
                _logger.LogInformation("Client connected from {0}:{1}", remoteEndpoint?.Address, remoteEndpoint?.Port);

                try
                {
                    var buffer = new byte[_options.ReceiveBufferSize];
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        using var idleCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        idleCts.CancelAfter(_options.IdleTimeout);

                        int bytesRead;
                        try
                        {
                            bytesRead = await stream.ReadAsync(buffer.AsMemory(), idleCts.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Closing idle client after {0}", _options.IdleTimeout);
                            break;
                        }

                        if (bytesRead == 0) break;

                        var frameData = buffer[..bytesRead];
                        await ProcessFrameAsync(frameData, remoteEndpoint?.Address?.ToString() ?? "Unknown").ConfigureAwait(false);
                    }
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error processing TCP client");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Error processing TCP client");
                }

                _logger.LogInformation("Client disconnected");
            }
        }
        finally
        {
            _connectionSlots.Release();
        }
    }

    /// <summary>Listens for UDP packets with GPS frames</summary>
    private async Task ListenUdpAsync(int port, CancellationToken cancellationToken)
    {
        _udpClient = new UdpClient(port);
        _logger.LogInformation("UDP listener started on port {0}", port);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                await ProcessFrameAsync(result.Buffer, result.RemoteEndPoint.Address.ToString()).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("UDP listener stopped");
        }
    }

    /// <summary>Parses GPS frame and stores location data</summary>
    private async Task ProcessFrameAsync(byte[] data, string sourceAddress)
    {
        try
        {
            var detection = await _parserService.DetectProtocolAsync(data).ConfigureAwait(false);
            if (detection.Result != ProtocolDetectionResult.Detected)
            {
                _logger.LogWarning("Unknown protocol from {0}", sourceAddress);
                return;
            }

            var frame = new GpsFrame
            {
                RawData = data,
                Protocol = detection.DetectedProtocol,
                ReceivedAt = DateTime.UtcNow,
                SourceAddress = sourceAddress
            };

            bool isValid = await _parserService.ValidateFrameAsync(frame).ConfigureAwait(false);
            if (!isValid)
            {
                _logger.LogWarning("Invalid frame from {0}", sourceAddress);
                return;
            }

            var location = await _parserService.ParseFrameAsync(frame).ConfigureAwait(false);
            if (location is null) return;

            var stored = await _locationService.StoreLocationAsync(location).ConfigureAwait(false);
            _logger.LogInformation("Location stored: Device={0} Lat={1:F4} Lng={2:F4} Speed={3:F1}",
                stored.DeviceId, stored.Latitude, stored.Longitude, stored.Speed);
        }
        catch (GpsTrackerProtocol.Domain.ParseException ex)
        {
            _logger.LogError(ex, "Error processing frame from {0}", sourceAddress);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error processing frame from {0}", sourceAddress);
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _tcpListener?.Stop();
        _udpClient?.Dispose();
        _logger.LogInformation("GPS Tracker Server stopped");
    }

    public static async Task Main(string[] args)
    {
        var tcpPort = args.Length > 0 && int.TryParse(args[0], out var tcp) ? tcp : 5000;
        var udpPort = args.Length > 1 && int.TryParse(args[1], out var udp) ? udp : 5001;

        // Throws OptionsValidationException listing every bad value before any port is bound.
        var server = new RealTimeGpsServer(new GpsListenerOptions { TcpPort = tcpPort, UdpPort = udpPort });

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            var serverTask = server.StartAsync();
            await Task.Delay(Timeout.Infinite, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            server.Stop();
        }
    }
}
