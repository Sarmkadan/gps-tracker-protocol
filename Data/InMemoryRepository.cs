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
/// Generic in-memory repository implementation for testing and demo purposes.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    protected readonly ConcurrentDictionary<string, T> _store = new();

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return _store.TryGetValue(id, out var entity) ? CreateSnapshot(entity) : null;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return _store.Values.Select(CreateSnapshot).ToList();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        if (!_store.TryAdd(id, snapshot))
            throw new InvalidOperationException($"Entity with ID {id} already exists");
        return snapshot;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        _store.AddOrUpdate(id,
            (key) => throw new KeyNotFoundException($"Entity with ID {id} not found"),
            (key, old) => snapshot);
        return snapshot;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        return _store.TryRemove(id, out _);
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        return _store.ContainsKey(id);
    }

    protected string GetId(T entity)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty?.GetValue(entity) is string id)
            return id;
        throw new InvalidOperationException($"Entity {entity.GetType().Name} must have an Id property");
    }

    /// <summary>
    /// Creates a deep copy of the entity to prevent external modifications.
    /// </summary>
    protected virtual T CreateSnapshot(T entity)
    {
        if (entity == null)
            return null!;

        // Use MemberwiseClone for shallow copy, then deep copy any mutable properties
        var copy = (T)entity.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(entity, null);

        // Deep copy ExtendedData if it exists (for LocationData)
        if (entity is LocationData locationData && copy is LocationData locationDataCopy)
        {
            locationDataCopy.ExtendedData = new Dictionary<string, object>(locationData.ExtendedData);
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

    public async Task<IEnumerable<LocationData>> GetByDeviceIdAsync(string deviceId)
    {
        return _store.Values.Where(l => l.DeviceId == deviceId).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<LocationData>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        return _store.Values.Where(l => l.Timestamp >= start && l.Timestamp <= end).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<LocationData>> GetByDeviceAndTimeRangeAsync(string deviceId, DateTime start, DateTime end)
    {
        return _store.Values
            .Where(l => l.DeviceId == deviceId && l.Timestamp >= start && l.Timestamp <= end)
            .OrderBy(l => l.Timestamp)
            .Select(CreateSnapshot)
            .ToList();
    }

    public async Task<LocationData?> GetLatestByDeviceIdAsync(string deviceId)
    {
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
                _store.TryRemove(key, out _);

            return keysToDelete.Count;
        }
        finally
        {
            _deleteLock.Release();
        }
    }
}
