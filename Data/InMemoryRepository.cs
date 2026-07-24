#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using System.Collections.Concurrent;
using System.Reflection;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Configuration for bounded retention policies in InMemoryRepository.
/// </summary>
public class RepositoryRetentionPolicy
{
    /// <summary>
    /// Maximum number of entities to retain per device/collection.
    /// When this limit is exceeded, oldest entries are evicted.
    /// Set to null or 0 to disable max points retention.
    /// </summary>
    public int? MaxPointsPerDevice { get; set; }

    /// <summary>
    /// Maximum age of entities to retain.
    /// When this limit is exceeded, entries are evicted.
    /// Set to null or TimeSpan.Zero to disable max age retention.
    /// </summary>
    public TimeSpan? MaxAge { get; set; }

    /// <summary>
    /// Whether eviction should be enabled.
    /// </summary>
    public bool IsEnabled => MaxPointsPerDevice > 0 || (MaxAge != null && MaxAge > TimeSpan.Zero);
}

/// <summary>
/// Generic in-memory repository implementation for testing and demo purposes.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    protected readonly ConcurrentDictionary<string, T> _store = new();
    protected readonly RepositoryRetentionPolicy? _retentionPolicy;
    protected readonly SemaphoreSlim _evictionLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the InMemoryRepository with default retention policy (no eviction).
    /// </summary>
    public InMemoryRepository()
    {
        _retentionPolicy = null;
    }

    /// <summary>
    /// Initializes a new instance of the InMemoryRepository with the specified retention policy.
    /// </summary>
    /// <param name="retentionPolicy">Retention policy configuration. Pass null for unlimited retention.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="retentionPolicy"/> is null.</exception>
    public InMemoryRepository(RepositoryRetentionPolicy? retentionPolicy)
    {
        _retentionPolicy = retentionPolicy;
    }

    /// <summary>
    /// Performs eviction based on retention policy if enabled.
    /// </summary>
    /// <param name="id">The ID of the newly added entity (used for device-specific retention).</param>
    /// <param name="entity">The newly added entity (used for age-based retention).</param>
    /// <exception cref="ArgumentNullException">Thrown if entity is null when retention policy requires it.</exception>
    protected virtual async Task EvictIfNeededAsync(string id, T? entity = null)
    {
        if (_retentionPolicy == null || !_retentionPolicy.IsEnabled)
        {
            return;
        }

        await _evictionLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_retentionPolicy.MaxPointsPerDevice > 0)
            {
                await EvictByMaxPointsAsync().ConfigureAwait(false);
            }

            if (_retentionPolicy.MaxAge != null && _retentionPolicy.MaxAge > TimeSpan.Zero && entity != null)
            {
                await EvictByMaxAgeAsync(entity).ConfigureAwait(false);
            }
        }
        finally
        {
            _evictionLock.Release();
        }
    }

    /// <summary>
    /// Evicts oldest entries when max points per device limit is exceeded.
    /// For generic repositories without device-specific grouping, this evicts globally.
    /// Subclasses should override for device-specific logic.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if eviction fails.</exception>
    protected virtual async Task EvictByMaxPointsAsync()
    {
        var maxPoints = _retentionPolicy?.MaxPointsPerDevice ?? 0;
        if (maxPoints <= 0)
        {
            return;
        }

        // Check if this is a LocationData repository by checking the type
        if (typeof(T) == typeof(LocationData))
        {
            // Use device-specific eviction for LocationData
            await EvictByMaxPointsForLocationDataAsync().ConfigureAwait(false);
        }
        else
        {
            // For generic repositories, evict oldest entries globally
            await EvictByMaxPointsGloballyAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Evicts oldest entries globally when max points limit is exceeded.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if eviction fails.</exception>
    protected virtual async Task EvictByMaxPointsGloballyAsync()
    {
        var maxPoints = _retentionPolicy?.MaxPointsPerDevice ?? 0;
        if (maxPoints <= 0)
        {
            return;
        }

        // For generic repositories, evict oldest entries globally
        while (_store.Count > maxPoints)
        {
            // Find the oldest entry by timestamp if available, otherwise use any entry
            var oldestEntry = _store.OrderBy(kvp => GetTimestamp(kvp.Value)).FirstOrDefault();
            if (oldestEntry.Key != null)
            {
                if (!_store.TryRemove(oldestEntry.Key, out _))
                {
                    throw new InvalidOperationException("Failed to remove entry during eviction");
                }
            }
            else
            {
                // Fallback: remove any entry
                var anyEntry = _store.FirstOrDefault();
                if (anyEntry.Key != null)
                {
                    if (!_store.TryRemove(anyEntry.Key, out _))
                    {
                        throw new InvalidOperationException("Failed to remove entry during eviction");
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Evicts oldest entries by device when max points per device limit is exceeded (for LocationData).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if eviction fails.</exception>
    protected virtual async Task EvictByMaxPointsForLocationDataAsync()
    {
        var maxPoints = _retentionPolicy?.MaxPointsPerDevice ?? 0;
        if (maxPoints <= 0)
        {
            return;
        }

        // Group by device and evict oldest entries per device
        var devices = _store.Values
            .Where(v => v is LocationData)
            .Select(v => v as LocationData)
            .Where(l => !string.IsNullOrEmpty(l?.DeviceId))
            .GroupBy(l => l!.DeviceId);

        foreach (var deviceGroup in devices)
        {
            var deviceEntries = deviceGroup
                .OrderBy(l => l!.Timestamp)
                .ToList();

            if (deviceEntries.Count > maxPoints)
            {
                var entriesToRemove = deviceEntries.Take(deviceEntries.Count - maxPoints);
                foreach (var entry in entriesToRemove)
                {
                    // Find the key for this entry
                    var keyToRemove = _store.FirstOrDefault(kvp =>
                        GetId(kvp.Value) == entry!.Id &&
                        (kvp.Value as LocationData)?.DeviceId == entry!.DeviceId).Key;
                    if (keyToRemove != null)
                    {
                        if (!_store.TryRemove(keyToRemove, out _))
                        {
                            throw new InvalidOperationException("Failed to remove entry during eviction");
                        }
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Evicts entries older than the max age limit.
    /// </summary>
    /// <param name="entity">The entity being added (used to determine timestamp).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if eviction fails.</exception>
    protected virtual async Task EvictByMaxAgeAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var maxAge = _retentionPolicy?.MaxAge ?? TimeSpan.Zero;
        if (maxAge <= TimeSpan.Zero)
        {
            return;
        }

        var cutoff = DateTime.UtcNow - maxAge;
        var keysToRemove = _store
            .Where(kvp => GetTimestamp(kvp.Value) < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            if (!_store.TryRemove(key, out _))
            {
                throw new InvalidOperationException("Failed to remove entry during age-based eviction");
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Extracts timestamp from entity for age-based eviction.
    /// Override this in subclasses for entity-specific timestamp extraction.
    /// </summary>
    /// <param name="entity">The entity to extract timestamp from.</param>
    /// <returns>The timestamp of the entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
    protected virtual DateTime GetTimestamp(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Default implementation: try to get Timestamp property
        var timestampProperty = entity.GetType().GetProperty("Timestamp");
        if (timestampProperty?.GetValue(entity) is DateTime timestamp)
        {
            return timestamp;
        }

        // Fallback: return minimum date
        return DateTime.MinValue;
    }

    public virtual Task<T?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.TryGetValue(id, out var entity) ? CreateSnapshot(entity) : null);
    }

    public virtual Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<T>>(_store.Values.Select(CreateSnapshot).ToList());
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        if (!_store.TryAdd(id, snapshot))
        {
            throw new InvalidOperationException($"Entity with ID {id} already exists");
        }

        await EvictIfNeededAsync(id, entity).ConfigureAwait(false);
        return snapshot;
    }

    public virtual Task<T> UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        _store.AddOrUpdate(id,
            (key) => throw new KeyNotFoundException($"Entity with ID {id} not found"),
            (key, old) => snapshot);
        return Task.FromResult(snapshot);
    }

    public virtual Task<bool> DeleteAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.TryRemove(id, out _));
    }

    public virtual Task<bool> ExistsAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.ContainsKey(id));
    }

    /// <summary>
    /// Gets the ID of an entity.
    /// </summary>
    /// <param name="entity">The entity to get ID from.</param>
    /// <returns>The entity ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if entity has no Id property.</exception>
    protected string GetId(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Use reflection to get Id property
        var idProperty = entity.GetType().GetProperty("Id", typeof(string));
        if (idProperty?.GetValue(entity) is string id)
        {
            return id;
        }

        throw new InvalidOperationException($"Entity {entity.GetType().Name} must have an Id property of type string");
    }

    /// <summary>
    /// Creates a deep copy of the entity to prevent external modifications.
    /// </summary>
    /// <param name="entity">The entity to create snapshot from.</param>
    /// <returns>A snapshot copy of the entity.</returns>
    protected virtual T CreateSnapshot(T entity)
    {
        if (entity == null)
        {
            return null!;
        }

        // Use MemberwiseClone for shallow copy, then deep copy any mutable properties
        var copy = (T)entity.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(entity, null);

        // Deep copy ExtendedData if it exists (for LocationData)
        if (entity is LocationData locationData && copy is LocationData locationDataCopy)
        {
            lock (locationData.ExtendedData)
            {
                locationDataCopy.ExtendedData = new Dictionary<string, object>(locationData.ExtendedData);
            }
        }

        return copy!;
    }
}

/// <summary>
/// In-memory implementation of location data repository.
/// </summary>
public class InMemoryLocationDataRepository : InMemoryRepository<LocationData>, ILocationDataRepository
{
    private readonly SemaphoreSlim _deleteLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the InMemoryLocationDataRepository with the specified retention policy.
    /// </summary>
    /// <param name="retentionPolicy">Retention policy configuration. Pass null for unlimited retention.</param>
    public InMemoryLocationDataRepository(RepositoryRetentionPolicy? retentionPolicy = null)
        : base(retentionPolicy)
    {
    }

    protected override DateTime GetTimestamp(LocationData entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Timestamp;
    }

    protected override async Task EvictByMaxPointsAsync()
    {
        var maxPoints = _retentionPolicy?.MaxPointsPerDevice ?? 0;
        if (maxPoints <= 0)
        {
            return;
        }

        // Group by device and evict oldest entries per device
        var devices = _store.Values
            .GroupBy(l => l.DeviceId)
            .Where(g => !string.IsNullOrEmpty(g.Key));

        foreach (var deviceGroup in devices)
        {
            var deviceEntries = deviceGroup
                .OrderBy(l => l.Timestamp)
                .ToList();

            if (deviceEntries.Count > maxPoints)
            {
                var entriesToRemove = deviceEntries.Take(deviceEntries.Count - maxPoints);
                foreach (var entry in entriesToRemove)
                {
                    var keyToRemove = _store.FirstOrDefault(kvp =>
                        kvp.Value.Id == entry!.Id &&
                        kvp.Value.DeviceId == entry!.DeviceId).Key;
                    if (keyToRemove != null)
                    {
                        if (!_store.TryRemove(keyToRemove, out _))
                        {
                            throw new InvalidOperationException("Failed to remove entry during device-specific eviction");
                        }
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    protected override async Task EvictByMaxAgeAsync(LocationData entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var maxAge = _retentionPolicy?.MaxAge ?? TimeSpan.Zero;
        if (maxAge <= TimeSpan.Zero)
        {
            return;
        }

        var cutoff = DateTime.UtcNow - maxAge;
        var keysToRemove = _store
            .Where(kvp => kvp.Value.Timestamp < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            if (!_store.TryRemove(key, out _))
            {
                throw new InvalidOperationException("Failed to remove entry during age-based eviction");
            }
        }

        await Task.CompletedTask;
    }

    public async Task<IEnumerable<LocationData>> GetByDeviceIdAsync(string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        return _store.Values.Where(l => l.DeviceId == deviceId).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<LocationData>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        return _store.Values
            .Where(l => l.Timestamp >= start && l.Timestamp <= end)
            .Select(CreateSnapshot)
            .ToList();
    }

    public async Task<IEnumerable<LocationData>> GetByDeviceAndTimeRangeAsync(string deviceId, DateTime start, DateTime end)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        return _store.Values
            .Where(l => l.DeviceId == deviceId && l.Timestamp >= start && l.Timestamp <= end)
            .OrderBy(l => l.Timestamp)
            .Select(CreateSnapshot)
            .ToList();
    }

    public async Task<LocationData?> GetLatestByDeviceIdAsync(string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        return _store.Values
            .Where(l => l.DeviceId == deviceId)
            .OrderByDescending(l => l.Timestamp)
            .Select(CreateSnapshot)
            .FirstOrDefault();
    }

    public async Task<IEnumerable<LocationData>> GetWithinRadiusAsync(double latitude, double longitude, double radiusKm)
    {
        var center = new LocationData { Latitude = latitude, Longitude = longitude };
        return _store.Values
            .Where(l => center.DistanceTo(l) <= radiusKm)
            .Select(CreateSnapshot)
            .ToList();
    }

    public async Task<int> DeleteOlderThanAsync(DateTime dateTime)
    {
        await _deleteLock.WaitAsync();
        try
        {
            var keysToDelete = _store
                .Where(kvp => kvp.Value.Timestamp < dateTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToDelete)
            {
                _store.TryRemove(key, out _);
            }

            return keysToDelete.Count;
        }
        finally
        {
            _deleteLock.Release();
        }
    }
}