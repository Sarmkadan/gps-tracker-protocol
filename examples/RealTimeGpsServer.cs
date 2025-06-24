// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private TcpListener? _tcpListener;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cancellationTokenSource;

    public RealTimeGpsServer()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<RealTimeGpsServer>>();
        _deviceService = _provider.GetRequiredService<IDeviceService>();
        _locationService = _provider.GetRequiredService<ILocationDataService>();
        _parserService = _provider.GetRequiredService<IProtocolParserService>();
    }

    /// <summary>Starts listening for GPS data on TCP and UDP ports</summary>
    public async Task StartAsync(int tcpPort = 5000, int udpPort = 5001)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _logger.LogInformation("Starting GPS Tracker Server on TCP:{0} UDP:{1}", tcpPort, udpPort);

        var tcpTask = ListenTcpAsync(tcpPort, _cancellationTokenSource.Token);
        var udpTask = ListenUdpAsync(udpPort, _cancellationTokenSource.Token);

        await Task.WhenAll(tcpTask, udpTask);
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
                var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
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
        using (client)
        using (var stream = client.GetStream())
        {
            var remoteEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
            _logger.LogInformation("Client connected from {0}:{1}", remoteEndpoint?.Address, remoteEndpoint?.Port);

            try
            {
                var buffer = new byte[1024];
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    var frameData = buffer[..bytesRead];
                    await ProcessFrameAsync(frameData, remoteEndpoint?.Address?.ToString() ?? "Unknown");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TCP client");
            }

            _logger.LogInformation("Client disconnected");
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
                var result = await _udpClient.ReceiveAsync(cancellationToken);
                await ProcessFrameAsync(result.Buffer, result.RemoteEndPoint.Address.ToString());
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
            var protocol = await _parserService.DetectProtocolAsync(data);
            if (protocol == ProtocolType.Unknown)
            {
                _logger.LogWarning("Unknown protocol from {0}", sourceAddress);
                return;
            }

            var frame = new GpsFrame
            {
                RawData = data,
                Protocol = protocol,
                ReceivedAt = DateTime.UtcNow,
                SourceAddress = sourceAddress
            };

            bool isValid = await _parserService.ValidateFrameAsync(frame);
            if (!isValid)
            {
                _logger.LogWarning("Invalid frame from {0}", sourceAddress);
                return;
            }

            var location = await _parserService.ExtractLocationDataAsync(frame);
            if (location == null) return;

            var stored = await _locationService.StoreLocationAsync(location);
            _logger.LogInformation("Location stored: Device={0} Lat={1:F4} Lng={2:F4} Speed={3:F1}",
                stored.DeviceId, stored.Latitude, stored.Longitude, stored.Speed);
        }
        catch (Exception ex)
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
        var server = new RealTimeGpsServer();
        var tcpPort = args.Length > 0 && int.TryParse(args[0], out var tcp) ? tcp : 5000;
        var udpPort = args.Length > 1 && int.TryParse(args[1], out var udp) ? udp : 5001;

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            var serverTask = server.StartAsync(tcpPort, udpPort);
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException)
        {
            server.Stop();
        }
    }
}
