using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GpsTrackerProtocol;
using GpsTrackerProtocol.Benchmarks;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Benchmarks
{
    [MemoryDiagnoser]
    public class DeviceDiagnosticsReportBenchmarks
    {
        private DeviceDiagnosticsReport _report;
        private List<LocationData> _locationData;

        [GlobalSetup]
        public void Setup()
        {
            _locationData = new List<LocationData>();
            for (int i = 0; i < 1000; i++)
            {
                _locationData.Add(new LocationData
                {
                    Latitude = 48.1173,
                    Longitude = 11.5166667,
                    Altitude = 545.4,
                    SatelliteCount = 8,
                    Accuracy = 0.9
                });
            }
            _report = new DeviceDiagnosticsReport(_locationData);
        }

        [Benchmark]
        public void BenchmarkGenerateReport()
        {
            _report.GenerateReport();
        }

        [Params(10, 100, 1000)]
        public int N;

        [Benchmark]
        public void BenchmarkGetLocationData()
        {
            for (int i = 0; i < N; i++)
            {
                _report.GetLocationData();
            }
        }

        [Benchmark]
        public void BenchmarkGetDeviceStatistics()
        {
            _report.GetDeviceStatistics();
        }
    }
}
