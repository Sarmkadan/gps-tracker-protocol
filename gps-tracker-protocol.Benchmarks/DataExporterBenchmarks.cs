using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using GpsTrackerProtocol.Examples;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Benchmarks
{
    [MemoryDiagnoser]
    public class DataExporterBenchmarks
    {
        private DataExporter _exporter = null!;
        private List<LocationData> _data = null!;

        // Vary the number of items exported
        [Params(10, 100, 1000)]
        public int Size { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Initialize the exporter (assumes a parameterless constructor)
            _exporter = new DataExporter();

            // Prepare a list of dummy LocationData objects
            _data = new List<LocationData>(Size);
            for (int i = 0; i < Size; i++)
            {
                var loc = new LocationData
                {
                    // Populate with plausible dummy values; adjust property names as needed
                    // If LocationData has a parameterless constructor and no required properties,
                    // this block can stay empty.
                };
                _data.Add(loc);
            }
        }

        // Benchmark exporting to JSON (synchronous)
        [Benchmark]
        public string ExportToJson()
        {
            // Assuming DataExporter has a method: string ExportToJson(IEnumerable<LocationData> data)
            return _exporter.ExportToJson(_data);
        }

        // Benchmark exporting to CSV (synchronous)
        [Benchmark]
        public string ExportToCsv()
        {
            // Assuming DataExporter has a method: string ExportToCsv(IEnumerable<LocationData> data)
            return _exporter.ExportToCsv(_data);
        }

        // Benchmark asynchronous export (e.g., writing to a stream or file)
        [Benchmark]
        public async Task ExportAsync()
        {
            // Assuming DataExporter has a method: Task ExportAsync(IEnumerable<LocationData> data)
            await _exporter.ExportAsync(_data);
        }
    }
}
