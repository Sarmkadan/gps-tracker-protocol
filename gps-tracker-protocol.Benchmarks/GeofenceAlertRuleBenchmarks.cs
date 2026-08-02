using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows;
using GpsTrackerProtocol.Domain.Models;
using System;
using System.Collections.Generic;

namespace GpsTrackerProtocol.Benchmarks
{
    /// <summary>
    /// Benchmarks for the public members of <see cref="GeofenceAlertRule"/>.
    /// The benchmarks focus on the methods that contain non‑trivial logic such as
    /// evaluating a location against a geofence and calculating distances.
    /// </summary>
    [MemoryDiagnoser]
    public class GeofenceAlertRuleBenchmarks
    {
        // Number of location points that will be processed in the benchmark methods.
        [Params(10, 100, 1000)]
        public int N;

        private GeofenceAlertRule _rule;
        private List<LocationData> _locations;

        /// <summary>
        /// Sets up a realistic geofence (a simple square) and a collection of location points.
        /// The points are generated randomly inside a bounding box that surrounds the geofence.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            // Define a square geofence centred at (50.0, 10.0) with a side length of 0.1 degrees.
            var geofenceVertices = new[]
            {
                new LocationData { Latitude = 49.95, Longitude = 9.95 },
                new LocationData { Latitude = 49.95, Longitude = 10.05 },
                new LocationData { Latitude = 50.05, Longitude = 10.05 },
                new LocationData { Latitude = 50.05, Longitude = 9.95 }
            };

            // Initialise the rule – the constructor signature is assumed to accept the vertices.
            _rule = new GeofenceAlertRule(geofenceVertices);

            // Prepare N random location points around the geofence.
            var rnd = new Random(42);
            _locations = new List<LocationData>(N);
            for (int i = 0; i < N; i++)
            {
                // Random latitude between 49.90 and 50.10
                double lat = 49.90 + rnd.NextDouble() * 0.20;
                // Random longitude between 9.90 and 10.10
                double lon = 9.90 + rnd.NextDouble() * 0.20;

                _locations.Add(new LocationData
                {
                    Latitude = lat,
                    Longitude = lon,
                    // The remaining fields are not required for the geofence logic.
                    DeviceId = $"device-{i}",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Benchmarks the evaluation of many locations against the geofence rule.
        /// </summary>
        [Benchmark]
        public int EvaluateMultipleLocations()
        {
            int triggeredCount = 0;
            foreach (var loc in _locations)
            {
                // The public method that determines whether the rule fires.
                // The exact method name is assumed to be IsAlertTriggered.
                if (_rule.IsAlertTriggered(loc))
                    triggeredCount++;
            }
            return triggeredCount;
        }

        /// <summary>
        /// Benchmarks the distance calculation from a location to the centre of the geofence.
        /// </summary>
        [Benchmark]
        public double CalculateDistanceToCenter()
        {
            double total = 0;
            foreach (var loc in _locations)
            {
                // The public method that returns the distance (in metres) to the geofence centre.
                // The exact method name is assumed to be DistanceToCenter.
                total += _rule.DistanceToCenter(loc);
            }
            return total;
        }

        /// <summary>
        /// Benchmarks the generation of the alert message for a location that triggers the rule.
        /// Only locations that actually trigger the rule are processed.
        /// </summary>
        [Benchmark]
        public string GenerateAlertMessages()
        {
            // Build a single concatenated string to avoid allocating many strings in the loop.
            var builder = new System.Text.StringBuilder();
            foreach (var loc in _locations)
            {
                if (_rule.IsAlertTriggered(loc))
                {
                    // The public method that creates a human‑readable alert message.
                    // The exact method name is assumed to be GetAlertMessage.
                    builder.AppendLine(_rule.GetAlertMessage(loc));
                }
            }
            return builder.ToString();
        }
    }
}
