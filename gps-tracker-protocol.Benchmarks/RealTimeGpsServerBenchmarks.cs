[Benchmark]
[MemoryDiagnoser]
public class RealTimeGpsServerBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // prepare test data
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // test code
    }

    [Benchmark]
    [Params(10)]
    public void BenchmarkMethod2(int inputSize)
    {
        // test code
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // test code
    }
}