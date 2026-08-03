[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class JourneyAnalyticsWorkerBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
    }

    [Params(10)]
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test method 1
    }

    [Params(100)]
    [Benchmark]
    public void BenchmarkMethod2()
    {
        // Test method 2
    }

    [Params(1000)]
    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test method 3
    }
}
