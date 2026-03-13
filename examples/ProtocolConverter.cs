// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain; // Added for ProtocolType
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Configuration;

/// <summary>
/// Converts GPS tracker frames between different protocols (GT06, H02, TK103).
/// Useful for protocol migration, interoperability testing, and multi-protocol systems.
/// </summary>
public class ProtocolConverter
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<ProtocolConverter> _logger;
    private readonly IProtocolParserService _parserService;

    public ProtocolConverter()
    {
        var services = new ServiceCollection();
        services.AddGpsTrackerServices();
        services.AddLogging(builder => builder.AddConsole());
        _provider = services.BuildServiceProvider();

        _logger = _provider.GetRequiredService<ILogger<ProtocolConverter>>();
        _parserService = _provider.GetRequiredService<IProtocolParserService>();
    }

    /// <summary>Converts a frame from one protocol to another</summary>
    public async Task<byte[]?> ConvertFrameAsync(byte[] sourceData,
        ProtocolType sourceProtocol, ProtocolType targetProtocol)
    {
        // Parse source frame
        var sourceFrame = new GpsFrame
        {
            RawData = sourceData,
            Protocol = sourceProtocol,
            ReceivedAt = DateTime.UtcNow
        };

        bool isValid = await _parserService.ValidateFrameAsync(sourceFrame);
        if (!isValid)
        {
            _logger.LogWarning("Source frame validation failed");
            return null;
        }

        var location = await _parserService.ExtractLocationDataAsync(sourceFrame);
        if (location == null)
        {
            _logger.LogWarning("Could not extract location from source frame");
            return null;
        }

        _logger.LogInformation("Extracted location: Lat={0:F4}, Lng={1:F4}, Speed={2:F1}",
            location.Latitude, location.Longitude, location.Speed);

        // Generate target frame
        var targetData = GenerateTargetFrame(location, targetProtocol);
        _logger.LogInformation("Generated {0} frame ({1} bytes)", targetProtocol, targetData?.Length ?? 0);

        return targetData;
    }

    /// <summary>Generates a frame in the target protocol format</summary>
    private byte[]? GenerateTargetFrame(LocationData location, ProtocolType targetProtocol)
    {
        return targetProtocol switch
        {
            ProtocolType.GT06 => GenerateGT06Frame(location),
            ProtocolType.H02 => GenerateH02Frame(location),
            ProtocolType.TK103 => GenerateTK103Frame(location),
            _ => null
        };
    }

    /// <summary>Generates a GT06 protocol frame</summary>
    private byte[] GenerateGT06Frame(LocationData location)
    {
        var frame = new List<byte>
        {
            0x78, 0x78,  // Start marker
            0x1F,        // Frame length (31 bytes)
            0x12         // Position reporting
        };

        // Add time (4 bytes)
        var now = DateTime.UtcNow;
        frame.Add((byte)((now.Year - 2000) & 0xFF));
        frame.Add((byte)now.Month);
        frame.Add((byte)now.Day);
        frame.Add((byte)now.Hour);

        // Add latitude (4 bytes)
        uint lat = (uint)(Math.Abs(location.Latitude) * 30000000);
        frame.AddRange(BitConverter.GetBytes(lat).Take(4));

        // Add longitude (4 bytes)
        uint lng = (uint)(Math.Abs(location.Longitude) * 30000000);
        frame.AddRange(BitConverter.GetBytes(lng).Take(4));

        // Add speed (2 bytes)
        ushort speed = (ushort)(location.Speed * 10);
        frame.AddRange(BitConverter.GetBytes(speed));

        // Add bearing (2 bytes)
        ushort bearing = (ushort)location.Bearing;
        frame.AddRange(BitConverter.GetBytes(bearing));

        // Add satellite count and other fields
        frame.Add((byte)location.SatelliteCount);
        frame.AddRange(new byte[4]); // Padding

        // Calculate checksum
        byte checksum = 0;
        for (int i = 2; i < frame.Count; i++)
            checksum ^= frame[i];

        frame.Add(checksum);
        frame.Add(0x0D);
        frame.Add(0x0A);

        return frame.ToArray();
    }

    /// <summary>Generates an H02 protocol frame (ASCII-based)</summary>
    private byte[] GenerateH02Frame(LocationData location)
    {
        var ns = location.Latitude >= 0 ? 'N' : 'S';
        var ew = location.Longitude >= 0 ? 'E' : 'W';

        var lat = Math.Abs(location.Latitude);
        var lng = Math.Abs(location.Longitude);

        int latDegrees = (int)lat;
        double latMinutes = (lat - latDegrees) * 60;

        int lngDegrees = (int)lng;
        double lngMinutes = (lng - lngDegrees) * 60;

        var timestamp = DateTime.UtcNow;

        var sentence = $"$HQ,{timestamp:yyMMddHHmmss}," +
            $"{latDegrees:D2}{latMinutes:F4},{ns}," +
            $"{lngDegrees:D3}{lngMinutes:F4},{ew}," +
            $"{location.Speed:F1},{location.Bearing:F1},1,12,5.0*\r\n";

        return Encoding.ASCII.GetBytes(sentence);
    }

    /// <summary>Generates a TK103 protocol frame (fixed 32-byte format)</summary>
    private byte[] GenerateTK103Frame(LocationData location)
    {
        var frame = new byte[32];

        frame[0] = 0x78;  // Start marker
        frame[1] = 0x78;

        var timestamp = DateTime.UtcNow;
        frame[2] = (byte)((timestamp.Year - 2000) & 0xFF);
        frame[3] = (byte)timestamp.Month;
        frame[4] = (byte)timestamp.Day;
        frame[5] = (byte)timestamp.Hour;
        frame[6] = (byte)timestamp.Minute;
        frame[7] = (byte)timestamp.Second;

        // Latitude
        uint lat = (uint)(Math.Abs(location.Latitude) * 1000000);
        Array.Copy(BitConverter.GetBytes(lat), 0, frame, 8, 4);

        // Longitude
        uint lng = (uint)(Math.Abs(location.Longitude) * 1000000);
        Array.Copy(BitConverter.GetBytes(lng), 0, frame, 12, 4);

        // Speed
        frame[16] = (byte)(location.Speed & 0xFF);

        // Bearing
        frame[17] = (byte)((location.Bearing / 2) & 0xFF);

        // Status and satellite count
        frame[18] = (byte)location.SatelliteCount;

        // Checksum
        byte checksum = 0;
        for (int i = 0; i < 30; i++)
            checksum += frame[i];

        frame[30] = checksum;
        frame[31] = 0x0A;

        return frame;
    }

    /// <summary>Converts a file from one protocol to another</summary>
    public async Task ConvertFileAsync(string inputPath, ProtocolType sourceProtocol,
        ProtocolType targetProtocol, string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            _logger.LogError("Input file not found: {0}", inputPath);
            return;
        }

        var inputData = await File.ReadAllBytesAsync(inputPath);
        _logger.LogInformation("Converting {0} bytes from {1} to {2}",
            inputData.Length, sourceProtocol, targetProtocol);

        var convertedData = await ConvertFrameAsync(inputData, sourceProtocol, targetProtocol);

        if (convertedData == null)
        {
            _logger.LogError("Conversion failed");
            return;
        }

        await File.WriteAllBytesAsync(outputPath, convertedData);
        _logger.LogInformation("Converted frame written to: {0}", outputPath);
    }

    public static async Task Main(string[] args)
    {
        var converter = new ProtocolConverter();

        if (args.Length < 3)
        {
            Console.WriteLine("Usage: ProtocolConverter <from> <to> <input> [output]");
            Console.WriteLine("Protocols: GT06, H02, TK103");
            Console.WriteLine("Examples:");
            Console.WriteLine("  ProtocolConverter GT06 H02 data.bin data.h02");
            Console.WriteLine("  ProtocolConverter H02 TK103 data.txt data.tk103");
            return;
        }

        if (!Enum.TryParse<ProtocolType>(args[0], out var sourceProtocol) ||
            !Enum.TryParse<ProtocolType>(args[1], out var targetProtocol))
        {
            Console.WriteLine("Invalid protocol specified");
            return;
        }

        var inputPath = args[2];
        var outputPath = args.Length > 3 ? args[3] : $"output_{targetProtocol}.bin";

        await converter.ConvertFileAsync(inputPath, sourceProtocol, targetProtocol, outputPath);
    }
}
