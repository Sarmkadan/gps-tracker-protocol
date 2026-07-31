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
using GpsTrackerProtocol.Benchmarks.GpsUtilitiesTestsValidation;

namespace GpsTrackerProtocol.Benchmarks
{
    [MemoryDiagnoser]
    public class GpsUtilitiesTestsValidationBenchmarks
    {
        [Params(10, 100, 1000)]
        public int N

        [GlobalSetup]
        public void Setup()
        {
            // TODO: set up test data
        }

        [Benchmark]
        public void BenchmarkMethod1()
        {
            // TODO: implement benchmark
        }

        [Benchmark]
        public void BenchmarkMethod2()
        {
            // TODO: implement benchmark
        }

        [Benchmark]
        public void BenchmarkMethod3()
        {
            // TODO: implement benchmark
        }
    }
}
