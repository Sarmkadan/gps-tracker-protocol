using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GpsTrackerProtocol.BackgroundWorkers;
using GpsTrackerProtocol.Caching;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.Logging;

namespace GpsTrackerProtocol.Benchmarks
{
    public class LocationAggregationWorkerBenchmarks
    {
        private LocationAggregationWorker _worker;
        private MethodInfo _methodInfo;
        private List<LocationData> _locations;

        [Params(100, 1000, 10000)]
        public int Size { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Create worker with null dependencies (safe because CalculateTotalDistance doesn't use them)
            _worker = new LocationAggregationWorker(null, null, null, new NullLogger());
            _methodInfo = typeof(LocationAggregationWorker).GetMethod("CalculateTotalDistance", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            // Generate locations with increasing latitude/longitude to simulate movement
            _locations = Enumerable.Range(0, Size)
                .Select(i => new LocationData
                {
                    Latitude = 50.0 + i * 0.0001,   // ~11m increment in latitude
                    Longitude = 10.0 + i * 0.0001,  // ~8m increment in longitude
                    Speed = 50.0,                   // constant speed for simplicity
                    Timestamp = DateTime.UtcNow.AddSeconds(i)
                })
                .ToList();
        }

        [Benchmark]
        public double CalculateTotalDistance()
        {
            // Invoke the private method via reflection
            return (double)_methodInfo.Invoke(_worker, new object[] { _locations });
        }

        private class NullLogger : ILogger<LocationAggregationWorker>
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
            private class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();
                public void Dispose() { }
            }
        }
    }
}