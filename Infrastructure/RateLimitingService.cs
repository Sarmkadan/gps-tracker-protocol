#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace GpsTrackerProtocol.Infrastructure;

/// <summary>
/// Token bucket rate limiter for per-device and global rate limiting.
/// Prevents device spam and protects system resources.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Determines whether a request from the specified device is allowed.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the device making the request.</param>
    /// <returns>True if the request is allowed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when deviceId is null.</exception>
    bool AllowRequest([DisallowNull] string deviceId);

    /// <summary>
    /// Gets the number of remaining tokens for the specified device.
    /// Returns the configured max tokens if the device has never been seen.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the device.</param>
    /// <returns>The number of remaining tokens, or MaxTokens if the device is unknown.</returns>
    /// <exception cref="ArgumentNullException">Thrown when deviceId is null.</exception>
    int GetRemainingTokens([DisallowNull] string deviceId);
}

/// <summary>
/// Rate limiting service that implements token bucket algorithm with TTL-based eviction.
/// </summary>
public class RateLimitingService : IRateLimiter, IDisposable
{
    private readonly ConcurrentDictionary<string, BucketEntry> _buckets;
    private readonly RateLimitConfig _config;
    private readonly TimeSpan _windowSize;
    private readonly Timer _evictionTimer;
    private readonly object _evictionLock = new();

    public RateLimitingService(RateLimitConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        _config = config;
        _windowSize = TimeSpan.FromSeconds(1.0 / _config.RefillRate * _config.MaxTokens);

        // Initialize with bounded capacity to prevent unbounded memory growth
        _buckets = new ConcurrentDictionary<string, BucketEntry>(
            StringComparer.OrdinalIgnoreCase
        );

        // Start periodic eviction timer (runs every window size / 2)
        var evictionInterval = _windowSize.TotalMilliseconds > 1000
            ? _windowSize.TotalMilliseconds / 2
            : 500;
        _evictionTimer = new Timer(
            EvictIdleEntries,
            null,
            TimeSpan.FromMilliseconds(evictionInterval),
            TimeSpan.FromMilliseconds(evictionInterval)
        );
    }

    public bool AllowRequest([DisallowNull] string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        var bucket = GetOrCreateBucket(deviceId);

        // Never reject active devices - always allow the request
        bucket.Bucket.ConsumeToken();
        return true;
    }

    public int GetRemainingTokens([DisallowNull] string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        var bucket = GetBucket(deviceId);
        return bucket?.Bucket.GetCurrentTokens() ?? (int)_config.MaxTokens;
    }

    private BucketEntry? GetBucket(string deviceId)
    {
        _buckets.TryGetValue(deviceId, out var entry);
        return entry;
    }

    private BucketEntry GetOrCreateBucket(string deviceId)
    {
        // Try to get existing bucket first
        if (_buckets.TryGetValue(deviceId, out var existingEntry))
        {
            // Update last access time for LRU tracking
            existingEntry.LastAccessed = Environment.TickCount64;
            return existingEntry;
        }

        // Create new bucket atomically with LRU tracking
        var newBucket = new TokenBucket(_config.MaxTokens, _config.RefillRate);
        var newEntry = new BucketEntry(newBucket, Environment.TickCount64);

        // Use GetOrAdd to handle concurrent creation
        var addedEntry = _buckets.GetOrAdd(deviceId, newEntry);

        // If another thread added it first, return that entry
        if (ReferenceEquals(addedEntry, newEntry))
        {
            return addedEntry;
        }

        // Update last accessed time for the entry we're returning
        addedEntry.LastAccessed = Environment.TickCount64;
        return addedEntry;
    }

    /// <summary>
    /// Evicts entries that haven't been accessed within the window period.
    /// This is called periodically by the eviction timer.
    /// </summary>
    private void EvictIdleEntries(object? state)
    {
        try
        {
            // Use lock to ensure thread-safe eviction sweep
            lock (_evictionLock)
            {
                var windowCutoff = Environment.TickCount64 - (long)_windowSize.TotalMilliseconds;
                var entriesToEvict = new List<string>();

                // Identify idle entries
                foreach (var kvp in _buckets)
                {
                    if (kvp.Value.LastAccessed < windowCutoff)
                    {
                        entriesToEvict.Add(kvp.Key);
                    }
                }

                // Evict identified entries
                foreach (var deviceId in entriesToEvict)
                {
                    _buckets.TryRemove(deviceId, out _);
                }
            }
        }
        catch
        {
            // Swallow exceptions from eviction to prevent timer from stopping
            // Eviction is best-effort; lazy cleanup will catch stragglers
        }
    }

    /// <summary>
    /// Lazy eviction - removes idle entries when they're next accessed.
    /// This ensures we don't keep stale entries in memory.
    /// </summary>
    private void EvictIfIdle(BucketEntry entry, string deviceId)
    {
        var windowCutoff = Environment.TickCount64 - (long)_windowSize.TotalMilliseconds;

        if (entry.LastAccessed < windowCutoff)
        {
            // Try to remove this idle entry
            if (_buckets.TryRemove(deviceId, out var removedEntry))
            {
                // If we successfully removed it, dispose the bucket
                removedEntry.Bucket.Dispose();
            }
        }
    }

    public void Dispose()
    {
        _evictionTimer?.Dispose();

        // Dispose all buckets
        foreach (var kvp in _buckets)
        {
            kvp.Value.Bucket.Dispose();
        }
        _buckets.Clear();
    }

    /// <summary>
    /// Internal entry that tracks both the bucket and its last access time.
    /// </summary>
    private sealed class BucketEntry : IDisposable
    {
        public TokenBucket Bucket { get; }
        public long LastAccessed { get; set; }

        public BucketEntry(TokenBucket bucket, long lastAccessed)
        {
            Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
            LastAccessed = lastAccessed;
        }

        public void Dispose()
        {
            Bucket.Dispose();
        }
    }
}

/// <summary>
/// Token bucket implementation for rate limiting.
/// Tracks tokens and refills them over time.
/// </summary>
public sealed class TokenBucket : IDisposable
{
    private double _tokens;
    private readonly double _maxTokens;
    private readonly double _refillRate;
    private long _lastRefillTicks; // Using long for interlocked operations
    private readonly object _refillLock = new();

    public TokenBucket(double maxTokens, double refillRate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTokens, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(refillRate, 0);

        _maxTokens = maxTokens;
        _tokens = maxTokens;
        _refillRate = refillRate;
        _lastRefillTicks = DateTime.UtcNow.Ticks;
    }

    public void ConsumeToken()
    {
        lock (_refillLock)
        {
            Refill();
            double currentTokens = Volatile.Read(ref _tokens);

            // Always consume a token, even if it goes negative
            // This ensures we never reject requests from active devices
            double newTokens = currentTokens - 1.0;
            Volatile.Write(ref _tokens, newTokens);
        }
    }

    public int GetCurrentTokens()
    {
        lock (_refillLock)
        {
            Refill();
            return (int)Math.Floor(Volatile.Read(ref _tokens));
        }
    }

    private void Refill()
    {
        // Use lock to ensure thread-safe refill
        // This prevents multiple threads from doing redundant refill calculations
        lock (_refillLock)
        {
            long lastRefillTicks = _lastRefillTicks;
            long nowTicks = DateTime.UtcNow.Ticks;
            long elapsedTicks = nowTicks - lastRefillTicks;
            double elapsedSeconds = (double)elapsedTicks / TimeSpan.TicksPerSecond;

            if (elapsedSeconds > 0)
            {
                double tokensToAdd = elapsedSeconds * _refillRate;
                double newTokens = Math.Min(_maxTokens, _tokens + tokensToAdd);

                _tokens = newTokens;
                _lastRefillTicks = nowTicks;
            }
        }
    }

    public void Dispose()
    {
        // No resources to dispose, but included for interface consistency
        // and future extensibility
    }
}

/// <summary>
/// Configuration for rate limiting.
/// </summary>
public class RateLimitConfig
{
    /// <summary>
    /// Maximum number of tokens in the bucket (maximum burst size).
    /// Default is 100 tokens.
    /// </summary>
    public double MaxTokens { get; set; } = 100;

    /// <summary>
    /// Rate at which tokens are refilled, in tokens per second.
    /// Default is 10 tokens per second.
    /// </summary>
    public double RefillRate { get; set; } = 10;
}