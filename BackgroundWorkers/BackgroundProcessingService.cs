#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.BackgroundWorkers;

using Microsoft.Extensions.Logging;

/// <summary>
/// Background task scheduler for recurring and one-time jobs.
/// Manages worker lifecycle and error handling.
/// </summary>
public interface IBackgroundWorker
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
    string WorkerName { get; }
}

public interface IBackgroundProcessingService
{
    void RegisterWorker(IBackgroundWorker worker);
    Task StartAllWorkersAsync(CancellationToken cancellationToken);
    Task StopAllWorkersAsync();
}

public class BackgroundProcessingService : IBackgroundProcessingService
{
    private readonly List<IBackgroundWorker> _workers = new();
    private readonly ILogger<BackgroundProcessingService> _logger;
    private readonly List<Task> _runningTasks = new();

    public BackgroundProcessingService(ILogger<BackgroundProcessingService> logger)
    {
        _logger = logger;
    }

    public void RegisterWorker(IBackgroundWorker worker)
    {
        _workers.Add(worker);
        _logger.LogInformation("Worker registered: {WorkerName}", worker.WorkerName);
    }

    public async Task StartAllWorkersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {Count} background workers", _workers.Count);

        foreach (var worker in _workers)
        {
            var task = worker.StartAsync(cancellationToken);
            _runningTasks.Add(task);
            _logger.LogInformation("Worker started: {WorkerName}", worker.WorkerName);
        }

        await Task.WhenAll(_runningTasks).ConfigureAwait(false);
    }

    public async Task StopAllWorkersAsync()
    {
        _logger.LogInformation("Stopping all background workers");

        foreach (var worker in _workers)
        {
            try
            {
                await worker.StopAsync().ConfigureAwait(false);
                _logger.LogInformation("Worker stopped: {WorkerName}", worker.WorkerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping worker: {WorkerName}", worker.WorkerName);
            }
        }
    }
}

/// <summary>
/// Base class for recurring background workers with retry logic.
/// </summary>
public abstract class RecurringBackgroundWorker : IBackgroundWorker
{
    protected readonly ILogger _logger;
    protected TimeSpan _interval = TimeSpan.FromMinutes(5);
    protected int _maxFailures = 3;
    private int _consecutiveFailures = 0;

    public abstract string WorkerName { get; }

    protected RecurringBackgroundWorker(ILogger logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{WorkerName}] Started", WorkerName);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("[{WorkerName}] Executing...", WorkerName);
                    await ExecuteAsync().ConfigureAwait(false);
                    _consecutiveFailures = 0;
                }
                catch (Exception ex)
                {
                    _consecutiveFailures++;
                    _logger.LogError(ex, "[{WorkerName}] Execution failed ({Failures}/{Max})",
                        WorkerName, _consecutiveFailures, _maxFailures);

                    if (_consecutiveFailures >= _maxFailures)
                    {
                        _logger.LogCritical("[{WorkerName}] Max failures reached, stopping worker", WorkerName);
                        break;
                    }
                }

                await Task.Delay(_interval, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[{WorkerName}] Cancelled", WorkerName);
        }
    }

    public Task StopAsync()
    {
        _logger.LogInformation("[{WorkerName}] Stopped", WorkerName);
        return Task.CompletedTask;
    }

    protected abstract Task ExecuteAsync();
}

/// <summary>
/// Worker pool for parallel task execution with concurrency control.
/// </summary>
public class ParallelWorkerPool
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger _logger;

    public ParallelWorkerPool(int maxConcurrency, ILogger logger)
    {
        _semaphore = new SemaphoreSlim(maxConcurrency);
        _logger = logger;
    }

    public async Task ExecuteAsync(Func<Task> work)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await work().ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ExecuteAsync<T>(IEnumerable<T> items, Func<T, Task> work)
    {
        var tasks = items.Select(item => ExecuteAsync(() => work(item)));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
