using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GpsTrackerProtocol;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Benchmarks
{
    [MemoryDiagnoser]
    public class DeviceBenchmarks
    {
        private Device _device;
        private List<LocationData> _locationDataList;

        [Params(10, 100, 1000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            _device = new Device();
            _locationDataList = new List<LocationData>();
            for (int i = 0; i < 1000; i++)
            {
                _locationDataList.Add(new LocationData());
            }
        }

        [Benchmark]
        public void BenchmarkDeviceConstructor()
        {
            for (int i = 0; i < N; i++)
            {
                new Device();
            }
        }

        [Benchmark]
        public void BenchmarkDeviceUpdateLocation()
        {
            for (int i = 0; i < N; i++)
            {
                _device.UpdateLocation(_locationDataList[i % _locationDataList.Count]);
            }
        }

        [Benchmark]
        public void BenchmarkDeviceGetLocationHistory()
        {
            for (int i = 0; i < N; i++)
            {
                _device.GetLocationHistory();
            }
        }

        [Benchmark]
        public void BenchmarkDeviceCalculateDistance()
        {
            for (int i = 0; i < N; i++)
            {
                _device.CalculateDistance(_locationDataList[i % _locationDataList.Count], _locationDataList[(i + 1) % _locationDataList.Count]);
            }
        }
    }
}
