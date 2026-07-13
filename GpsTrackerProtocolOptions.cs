// GpsTrackerProtocolOptions.cs
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace GpsTrackerProtocol.Configuration;

public class GpsTrackerProtocolOptions
{
    [Required]
    public string DefaultProtocol { get; set; } = "GT06";

    [Range(1, 10000)]
    public int MaxDevices { get; set; } = 10000;

    [Range(1, 1000)]
    public int LocationHistoryLimit { get; set; } = 1000;

    [Range(1, 60)]
    public int CacheExpirationMinutes { get; set; } = 60;

    [Range(1, 1000)]
    public int RateLimitPerMinute { get; set; } = 1000;

    [Required]
    public string LoggingLevel { get; set; } = "Information";

    public ProtocolSettings Protocol { get; set; } = new ProtocolSettings();

    public class ProtocolSettings
    {
        public bool GT06Enabled { get; set; } = true;
        public int GT06Timeout { get; set; } = 30;
        public int GT06MaxFrameSize { get; set; } = 200;

        public bool H02Enabled { get; set; } = true;
        public int H02Timeout { get; set; } = 30;
        public int H02MaxFrameSize { get; set; } = 300;

        public bool TK103Enabled { get; set; } = true;
        public int TK103Timeout { get; set; } = 30;
        public int TK103MaxFrameSize { get; set; } = 100;
    }
}
