[MemoryDiagnoser]
public class ReplayOptionsBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Setup test data
        var testReplayOptions = new ReplayOptions();
        testReplayOptions.Option1 = "value1";
        testReplayOptions.Option2 = 123;
        // Benchmark code
        for (int i = 0; i < 1000; i++)
        {
            // Code to benchmark
            var result = testReplayOptions.MethodToBenchmark();
        }
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int inputSize)
    {
        // Setup test data
        var testReplayOptions = new ReplayOptions();
        testReplayOptions.Option1 = "value1";
        testReplayOptions.Option2 = 123;
        // Benchmark code
        for (int i = 0; i < inputSize; i++)
        {
            // Code to benchmark
            var result = testReplayOptions.MethodToBenchmark();
        }
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Setup test data
        var testReplayOptions = new ReplayOptions();
        testReplayOptions.Option1 = "value1";
        testReplayOptions.Option2 = 123;
        // Benchmark code
        for (int i = 0; i < 1000; i++)
        {
            // Code to benchmark
            var result = testReplayOptions.MethodToBenchmark();
        }
    }
}