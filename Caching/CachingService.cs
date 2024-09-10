#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Caching;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// In-memory caching layer for location data and device information.
/// Reduces database queries and improves response times.
/// </summary>
public interface ICachingService
{
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    bool TryGet<T>(string key, out T value);
    void Remove(string key);
    void Clear();
    IEnumerable<string> GetAllKeys();
}

public class CachingService : ICachingService
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<CachingService> _logger;
    private readonly object _lock = new();

    public CachingService(ILogger<CachingService> logger)
    {
        _logger = logger;
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        lock (_lock)
        {
            var expiresAt = ttl.HasValue ? (DateTime?)DateTime.UtcNow.Add(ttl.Value) : null;

            _cache[key] = new CacheEntry
            {
                Value = value,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogDebug("Cache entry set: {Key}, TTL: {Ttl}s", key, ttl?.TotalSeconds ?? 0);
        }
    }

    public bool TryGet<T>(string key, out T value)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt is null || entry.ExpiresAt > DateTime.UtcNow)
                {
                    value = (T)entry.Value;
                    _logger.LogDebug("Cache hit: {Key}", key);
                    return true;
                }
                else
                {
                    _cache.Remove(key);
                    _logger.LogDebug("Cache entry expired: {Key}", key);
                }
            }

            value = default;
            return false;
        }
    }

    public void Remove(string key)
    {
        lock (_lock)
        {
            if (_cache.Remove(key))
                _logger.LogDebug("Cache entry removed: {Key}", key);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            var count = _cache.Count;
            _cache.Clear();
            _logger.LogInformation("Cache cleared: {Count} entries removed", count);
        }
    }

    public IEnumerable<string> GetAllKeys()
    {
        lock (_lock)
        {
            return _cache.Keys.ToList();
        }
    }

    private class CacheEntry
    {
        public object Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

/// <summary>
/// Cache key generator for consistent key formatting.
/// </summary>
public static class CacheKeyGenerator
{
    public static string GetDeviceKey(string deviceId) => $"device:{deviceId}";
    public static string GetDeviceListKey() => "devices:all";
    public static string GetLocationKey(string locationId) => $"location:{locationId}";
    public static string GetLatestLocationKey(string deviceId) => $"location:latest:{deviceId}";
    public static string GetLocationHistoryKey(string deviceId, int count) => $"location:history:{deviceId}:{count}";
    public static string GetJourneyKey(string journeyId) => $"journey:{journeyId}";
    public static string GetJourneyHistoryKey(string deviceId) => $"journey:history:{deviceId}";
    public static string GetCommandKey(string commandId) => $"command:{commandId}";
    public static string GetDeviceCommandsKey(string deviceId) => $"commands:{deviceId}";
}

/// <summary>
/// Distributed cache adapter for external caching systems (Redis, Memcached).
/// </summary>
public interface IDistributedCache
{
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task<T> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}

public class InMemoryDistributedCache : IDistributedCache
{
    private readonly ICachingService _cache;

    public InMemoryDistributedCache(ICachingService cache)
    {
        _cache = cache;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        _cache.Set(key, value, ttl);
        return Task.CompletedTask;
    }

    public Task<T> GetAsync<T>(string key)
    {
        _cache.TryGet(key, out T value);
        return Task.FromResult(value);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
