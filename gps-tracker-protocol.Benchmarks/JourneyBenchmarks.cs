[MemoryDiagnoser]
public class JourneyBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Setup test data
        var testData = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            testData.Add("testData" + i);
        }
        // Benchmark code
        Journey journey = new Journey();
        journey.Method1(testData);
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(100)] int inputSize)
    {
        // Setup test data
        var testData = new List<string>();
        for (int i = 0; i < inputSize; i++)
        {
            testData.Add("testData" + i);
        }
        // Benchmark code
        Journey journey = new Journey();
        journey.Method2(testData);
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Setup test data
        var testData = new Dictionary<string, string>();
        for (int i = 0; i < 10; i++)
        {
            testData.Add("testData" + i, "testValue" + i);
        }
        // Benchmark code
        Journey journey = new Journey();
        journey.Method3(testData);
    }
}