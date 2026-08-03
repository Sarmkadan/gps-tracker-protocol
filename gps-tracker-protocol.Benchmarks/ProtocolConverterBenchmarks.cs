using BenchmarkDotNet.Attributes;
using GpsTrackerProtocol.Domain;
using System.Threading.Tasks;

namespace GpsTrackerProtocol.Benchmarks
{
    [MemoryDiagnoser]
    public class ProtocolConverterBenchmarks
    {
        private ProtocolConverter _converter = null!;
        private byte[] _smallData = null!;
        private byte[] _largeData = null!;

        [GlobalSetup]
        public void Setup()
        {
            _converter = new ProtocolConverter();
            _smallData = new byte[100]; // 100 bytes
            _largeData = new byte[1000]; // 1000 bytes
            // Fill with some dummy data to make it realistic
            new System.Random(42).NextBytes(_smallData);
            new System.Random(42).NextBytes(_largeData);
        }

        [Benchmark]
        public async Task<byte[]?> ConvertSmallFrameGt06ToH02()
        {
            return await _converter.ConvertFrameAsync(_smallData, ProtocolType.GT06, ProtocolType.H02);
        }

        [Benchmark]
        public async Task<byte[]?> ConvertLargeFrameGt06ToH02()
        {
            return await _converter.ConvertFrameAsync(_largeData, ProtocolType.GT06, ProtocolType.H02);
        }

        [Benchmark]
        public async Task<byte[]?> ConvertGt06ToTk103()
        {
            return await _converter.ConvertFrameAsync(_smallData, ProtocolType.GT06, ProtocolType.TK103);
        }

        [Benchmark]
        public async Task<byte[]?> ConvertH02ToGt06()
        {
            return await _converter.ConvertFrameAsync(_smallData, ProtocolType.H02, ProtocolType.GT06);
        }

        [Benchmark]
        public async Task<byte[]?> ConvertTk103ToH02()
        {
            return await _converter.ConvertFrameAsync(_smallData, ProtocolType.TK103, ProtocolType.H02);
        }
    }
}