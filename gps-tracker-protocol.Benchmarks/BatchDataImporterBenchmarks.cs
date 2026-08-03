[MemoryDiagnoser]
public class BatchDataImporterBenchmarks
{
    [Benchmark]
    public void BenchmarkImport_10()
    {
        // Setup test data
        var importer = new BatchDataImporter();
        var data = new List<string>() { "a", "b", "c" };
        // Benchmark
        importer.Import(data);
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkImport_Params(int n)
    {
        // Setup test data
        var importer = new BatchDataImporter();
        var data = new List<string>();
        for (int i = 0; i < n; i++)
        {
            data.Add("item" + i);
        }
        // Benchmark
        importer.Import(data);
    }

    [Benchmark]
    public void BenchmarkImport_Large()
    {
        // Setup test data
        var importer = new BatchDataImporter();
        var data = new List<string>() { "a", "b", "c" };
        // Benchmark
        for (int i = 0; i < 1000; i++)
        {
            data.Add("item" + i);
        }
        importer.Import(data);
    }
}