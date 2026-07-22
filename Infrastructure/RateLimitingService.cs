#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Threading;

namespace GpsTrackerProtocol.Infrastructure;

/// <summary>
/// Token bucket rate limiter for per-device and global rate limiting.
/// Prevents device spam and protects system resources.
/// </summary>
public interface IRateLimiter
{
    bool AllowRequest(string deviceId);
    int GetRemainingTokens(string deviceId);
}

public class RateLimitingService : IRateLimiter
{
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly RateLimitConfig _config;
    private readonly TimeSpan _windowSize;

    public RateLimitingService(RateLimitConfig config)
    {
        _config = config;
        _windowSize = TimeSpan.FromSeconds(1.0 / _config.RefillRate * _config.MaxTokens);
    }

    public bool AllowRequest(string deviceId)
    {
        var bucket = GetOrCreateBucket(deviceId);
        return bucket.TryConsumeToken();
    }

    public int GetRemainingTokens(string deviceId)
    {
        var bucket = GetBucket(deviceId);
        return bucket?.GetCurrentTokens() ?? (int)_config.MaxTokens;
    }

    private TokenBucket? GetBucket(string deviceId)
    {
        _buckets.TryGetValue(deviceId, out var bucket);
        return bucket;
    }

    private TokenBucket GetOrCreateBucket(string deviceId)
    {
        // Try to get existing bucket first
        if (_buckets.TryGetValue(deviceId, out var existingBucket))
        {
            return existingBucket;
        }

        // Create new bucket atomically
        var newBucket = new TokenBucket(_config.MaxTokens, _config.RefillRate);
        return _buckets.GetOrAdd(deviceId, newBucket);
    }
}

public class TokenBucket
{
    private double _tokens;
    private readonly double _maxTokens;
    private readonly double _refillRate;
    private long _lastRefillTicks; // Using long for interlocked operations

    public TokenBucket(double maxTokens, double refillRate)
    {
        _maxTokens = maxTokens;
        _tokens = maxTokens;
        _refillRate = refillRate;
        _lastRefillTicks = DateTime.UtcNow.Ticks;
    }

    public bool TryConsumeToken()
    {
        Refill();
        double currentTokens = Volatile.Read(ref _tokens);

        // Try to consume atomically
        while (currentTokens >= 1.0)
        {
            double newTokens = currentTokens - 1.0;
            double originalTokens = Interlocked.CompareExchange(ref _tokens, newTokens, currentTokens);

            if (originalTokens == currentTokens)
            {
                return true;
            }

            currentTokens = originalTokens;
        }

        return false;
    }

    public int GetCurrentTokens()
    {
        Refill();
        return (int)Math.Floor(Volatile.Read(ref _tokens));
    }

    private void Refill()
    {
        long lastRefillTicks = Volatile.Read(ref _lastRefillTicks);
        long nowTicks = DateTime.UtcNow.Ticks;
        long elapsedTicks = nowTicks - lastRefillTicks;
        double elapsedSeconds = (double)elapsedTicks / TimeSpan.TicksPerSecond;

        if (elapsedSeconds > 0)
        {
            double tokensToAdd = elapsedSeconds * _refillRate;
            double currentTokens = Volatile.Read(ref _tokens);
            double newTokens = Math.Min(_maxTokens, currentTokens + tokensToAdd);

            // Atomically update both tokens and lastRefillTicks
            Interlocked.Exchange(ref _tokens, newTokens);
            Interlocked.Exchange(ref _lastRefillTicks, nowTicks);
        }
    }
}

public class RateLimitConfig
{
    public double MaxTokens { get; set; } = 100;
    public double RefillRate { get; set; } = 10; // tokens per second
}