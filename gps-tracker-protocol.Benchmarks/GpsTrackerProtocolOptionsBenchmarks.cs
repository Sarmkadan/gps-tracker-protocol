using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using GpsTrackerProtocol.Configuration;
using System;
using System.Collections.Generic;

namespace gps_tracker_protocol.Benchmarks
{
    [MemoryDiagnoser]
    public class GpsTrackerProtocolOptionsBenchmarks
    {
        private GpsTrackerProtocolOptions _options;
        private List<string> _protocols;

        [GlobalSetup]
        public void Setup()
        {
            _options = new GpsTrackerProtocolOptions();
            _protocols = new List<string> { "GT06", "H02", "TK103" };
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void BenchmarkProtocolSettings(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _options.Protocol.GT06Enabled = true;
                _options.Protocol.GT06Timeout = 30;
                _options.Protocol.GT06MaxFrameSize = 200;
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void BenchmarkLocationHistoryLimit(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _options.LocationHistoryLimit = 1000;
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void BenchmarkCacheExpirationMinutes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _options.CacheExpirationMinutes = 60;
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void BenchmarkRateLimitPerMinute(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _options.RateLimitPerMinute = 1000;
            }
        }
    }
}
