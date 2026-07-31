using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Tests;
using System;
using System.Collections.Generic;

namespace GpsTrackerProtocol.Benchmarks
{
    /// <summary>
    /// Benchmarks for the validation helpers defined in <c>DomainAndServiceTestsValidation</c>.
    /// The benchmarks exercise the most frequently used public methods:
    /// <c>Validate</c>, <c>IsValid</c> and <c>EnsureValid</c> for <c>LocationData</c> and <c>Device</c>.
    /// </summary>
    [MemoryDiagnoser]
    public class DomainAndServiceTestsValidationBenchmarks
    {
        // Input size – the number of items that will be validated in each benchmark run.
        [Params(10, 100, 1000)]
        public int N;

        private List<LocationData> _locationDataList = null!;
        private List<Device> _deviceList = null!;

        // -----------------------------------------------------------------
        // Global setup – creates realistic test data for the given N.
        // -----------------------------------------------------------------
        [GlobalSetup]
        public void Setup()
        {
            var rnd = new Random(42);

            _locationDataList = new List<LocationData>(N);
            for (int i = 0; i < N; i++)
            {
                // Create a valid LocationData instance.
                var loc = new LocationData
                {
                    DeviceId = $"dev-{i}",
                    Latitude = rnd.NextDouble() * 180.0 - 90.0,   // -90 .. +90
                    Longitude = rnd.NextDouble() * 360.0 - 180.0, // -180 .. +180
                    Speed = rnd.Next(0, 120),                     // km/h
                    Bearing = rnd.Next(0, 361),                  // 0 .. 360
                    SatelliteCount = rnd.Next(0, 20)
                };
                _locationDataList.Add(loc);
            }

            _deviceList = new List<Device>(N);
            for (int i = 0; i < N; i++)
            {
                // Create a valid Device instance.
                var dev = new Device
                {
                    Id = $"dev-{i}",
                    Imei = $"123456789012{(i % 10):D1}" // always 15 digits
                };
                _deviceList.Add(dev);
            }
        }

        // -----------------------------------------------------------------
        // Benchmark: Validate(LocationData) for a collection of N items.
        // -----------------------------------------------------------------
        [Benchmark]
        public void ValidateLocationData()
        {
            foreach (var loc in _locationDataList)
            {
                loc.Validate();
            }
        }

        // -----------------------------------------------------------------
        // Benchmark: Validate(Device) for a collection of N items.
        // -----------------------------------------------------------------
        [Benchmark]
        public void ValidateDevice()
        {
            foreach (var dev in _deviceList)
            {
                dev.Validate();
            }
        }

        // -----------------------------------------------------------------
        // Benchmark: IsValid(LocationData) for a collection of N items.
        // -----------------------------------------------------------------
        [Benchmark]
        public void IsValidLocationData()
        {
            foreach (var loc in _locationDataList)
            {
                _ = loc.IsValid();
            }
        }

        // -----------------------------------------------------------------
        // Benchmark: IsValid(Device) for a collection of N items.
        // -----------------------------------------------------------------
        [Benchmark]
        public void IsValidDevice()
        {
            foreach (var dev in _deviceList)
            {
                _ = dev.IsValid();
            }
        }

        // -----------------------------------------------------------------
        // Benchmark: EnsureValid(LocationData) for a collection of N items.
        // -----------------------------------------------------------------
        [Benchmark]
        public void EnsureValidLocationData()
        {
            foreach (var loc in _locationDataList)
            {
                loc.EnsureValid();
            }
        }

        // -----------------------------------------------------------------
        // Benchmark: EnsureValid(Device) for a collection of N items.
        // -----------------------------------------------------------------
        [Benchmark]
        public void EnsureValidDevice()
        {
            foreach (var dev in _deviceList)
            {
                dev.EnsureValid();
            }
        }
    }
}
