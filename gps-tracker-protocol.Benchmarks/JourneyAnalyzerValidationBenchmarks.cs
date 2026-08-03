[Benchmark]
[Benchmark(MinTimeQuery = 100, MaxTimeQuery = 5000)]
[MemoryDiagnoser]
public class JourneyAnalyzerValidationBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // Test method 1
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_Method2()
    {
        // Test method 2 with input size
    }

    [Benchmark]
    public void Benchmark_Method3()
    {
        // Test method 3
    }
}
