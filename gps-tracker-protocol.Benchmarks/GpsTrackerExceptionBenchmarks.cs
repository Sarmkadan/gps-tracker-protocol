using BenchmarkDotNet.Attributes;
using GpsTrackerProtocol.Domain;

namespace GpsTrackerProtocol.Benchmarks
{
    /// <summary>
    /// Benchmarks for the <see cref="GpsTrackerException"/> class.
    /// </summary>
    [MemoryDiagnoser]
    public class GpsTrackerExceptionBenchmarks
    {
        /// <summary>
        /// Number of exception instances to create and benchmark.
        /// </summary>
        [Params(10, 100, 1000)]
        public int N;

        /// <summary>
        /// Array of pre‑created exceptions used for instance methods.
        /// </summary>
        private GpsTrackerException[] _exceptions;

        /// <summary>
        /// Array of a second set of exceptions used for equality comparisons.
        /// </summary>
        private GpsTrackerException[] _otherExceptions;

        /// <summary>
        /// Prepare test data before each benchmark run.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _exceptions = new GpsTrackerException[N];
            _otherExceptions = new GpsTrackerException[N];

            for (int i = 0; i < N; i++)
            {
                _exceptions[i] = new GpsTrackerException($"Error {i}");
                _otherExceptions[i] = new GpsTrackerException($"Other {i}");
            }
        }

        /// <summary>
        /// Benchmarks the construction of a single <see cref="GpsTrackerException"/>.
        /// </summary>
        [Benchmark]
        public void BenchmarkCreateException()
        {
            var ex = new GpsTrackerException("Benchmark");
        }

        /// <summary>
        /// Benchmarks calling <see cref="GpsTrackerException.ToString"/> on a collection of exceptions.
        /// </summary>
        [Benchmark]
        public void BenchmarkToString()
        {
            foreach (var ex in _exceptions)
            {
                var s = ex.ToString();
                // Prevent the compiler from optimizing away the call
                if (s == null) System.Diagnostics.Debug.WriteLine("unreachable");
            }
        }

        /// <summary>
        /// Benchmarks the <see cref="GpsTrackerException.Equals(object)"/> method.
        /// </summary>
        [Benchmark]
        public void BenchmarkEquals()
        {
            for (int i = 0; i < _exceptions.Length; i++)
            {
                var ex = _exceptions[i];
                var other = _otherExceptions[(i + 1) % _otherExceptions.Length];
                var eq = ex.Equals(other);
                // Prevent optimization
                if (!eq) System.Diagnostics.Debug.WriteLine("unreachable");
            }
        }

        /// <summary>
        /// Benchmarks the <see cref="GpsTrackerException.GetHashCode"/> method.
        /// </summary>
        [Benchmark]
        public void BenchmarkGetHashCode()
        {
            foreach (var ex in _exceptions)
            {
                var h = ex.GetHashCode();
                // Prevent optimization
                if (h == 0) System.Diagnostics.Debug.WriteLine("unreachable");
            }
        }
    }
}
