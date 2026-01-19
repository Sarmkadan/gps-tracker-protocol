// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    private readonly RateLimitConfig _config;
    private readonly object _lock = new();

    public RateLimitingService(RateLimitConfig config)
    {
        _config = config;
    }

    public bool AllowRequest(string deviceId)
    {
        lock (_lock)
        {
            if (!_buckets.ContainsKey(deviceId))
                _buckets[deviceId] = new TokenBucket(_config.MaxTokens, _config.RefillRate);

            var bucket = _buckets[deviceId];
            return bucket.TryConsumeToken();
        }
    }

    public int GetRemainingTokens(string deviceId)
    {
        lock (_lock)
        {
            return _buckets.ContainsKey(deviceId)
                ? _buckets[deviceId].GetCurrentTokens()
                : (int)_config.MaxTokens;
        }
    }
}

public class TokenBucket
{
    private double _tokens;
    private readonly double _maxTokens;
    private readonly double _refillRate;
    private DateTime _lastRefill;

    public TokenBucket(double maxTokens, double refillRate)
    {
        _maxTokens = maxTokens;
        _tokens = maxTokens;
        _refillRate = refillRate;
        _lastRefill = DateTime.UtcNow;
    }

    public bool TryConsumeToken()
    {
        Refill();
        if (_tokens >= 1.0)
        {
            _tokens -= 1.0;
            return true;
        }
        return false;
    }

    public int GetCurrentTokens()
    {
        Refill();
        return (int)Math.Floor(_tokens);
    }

    private void Refill()
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastRefill).TotalSeconds;
        var tokensToAdd = elapsed * _refillRate;
        _tokens = Math.Min(_maxTokens, _tokens + tokensToAdd);
        _lastRefill = now;
    }
}

public class RateLimitConfig
{
    public double MaxTokens { get; set; } = 100;
    public double RefillRate { get; set; } = 10; // tokens per second
}
