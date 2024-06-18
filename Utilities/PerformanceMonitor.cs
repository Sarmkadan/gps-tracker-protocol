#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// Performance monitoring utility for tracking operation latency.
/// Useful for identifying bottlenecks and measuring system performance.
/// </summary>
public interface IPerformanceMonitor
{
    IDisposable MeasureOperation(string operationName);
    void RecordOperation(string operationName, TimeSpan duration);
    OperationMetrics GetMetrics(string operationName);
    void PrintReport();
}

public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly Dictionary<string, List<long>> _operationTimes = new();
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly object _lock = new();

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger;
    }

    public IDisposable MeasureOperation(string operationName)
    {
        return new OperationTimer(this, operationName);
    }

    public void RecordOperation(string operationName, TimeSpan duration)
    {
        lock (_lock)
        {
            if (!_operationTimes.ContainsKey(operationName))
                _operationTimes[operationName] = new List<long>();

            _operationTimes[operationName].Add(duration.Ticks);
        }
    }

    public OperationMetrics GetMetrics(string operationName)
    {
        lock (_lock)
        {
            if (!_operationTimes.TryGetValue(operationName, out var times) || times.Count == 0)
                return new OperationMetrics { OperationName = operationName };

            var ticks = times.OrderBy(t => t).ToList();
            var duration = new TimeSpan(ticks.Sum());

            return new OperationMetrics
            {
                OperationName = operationName,
                Count = times.Count,
                TotalDuration = duration,
                AverageDuration = TimeSpan.FromTicks((long)times.Average()),
                MinDuration = TimeSpan.FromTicks(ticks.First()),
                MaxDuration = TimeSpan.FromTicks(ticks.Last()),
                MedianDuration = TimeSpan.FromTicks(ticks[ticks.Count / 2])
            };
        }
    }

    public void PrintReport()
    {
        lock (_lock)
        {
            _logger.LogInformation("=== Performance Report ===");
            foreach (var operation in _operationTimes.Keys)
            {
                var metrics = GetMetrics(operation);
                _logger.LogInformation("Operation: {Name}", metrics.OperationName);
                _logger.LogInformation("  Count: {Count}", metrics.Count);
                _logger.LogInformation("  Total: {Total}ms", metrics.TotalDuration.TotalMilliseconds);
                _logger.LogInformation("  Average: {Avg:F2}ms", metrics.AverageDuration.TotalMilliseconds);
                _logger.LogInformation("  Min: {Min:F2}ms, Max: {Max:F2}ms, Median: {Med:F2}ms",
                    metrics.MinDuration.TotalMilliseconds,
                    metrics.MaxDuration.TotalMilliseconds,
                    metrics.MedianDuration.TotalMilliseconds);
            }
        }
    }

    private class OperationTimer : IDisposable
    {
        private readonly PerformanceMonitor _monitor;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        public OperationTimer(PerformanceMonitor monitor, string operationName)
        {
            _monitor = monitor;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed);
        }
    }
}

public class OperationMetrics
{
    public string OperationName { get; set; }
    public int Count { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan MedianDuration { get; set; }
}
