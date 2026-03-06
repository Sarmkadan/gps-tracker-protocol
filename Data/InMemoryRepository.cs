// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Generic in-memory repository implementation for testing and demo purposes.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    protected readonly Dictionary<string, T> _store = [];
    protected readonly ReaderWriterLockSlim _lock = new();

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.TryGetValue(id, out var entity) ? entity : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetId(entity);
        _lock.EnterWriteLock();
        try
        {
            if (_store.ContainsKey(id))
                throw new InvalidOperationException($"Entity with ID {id} already exists");
            _store[id] = entity;
            return entity;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetId(entity);
        _lock.EnterWriteLock();
        try
        {
            if (!_store.ContainsKey(id))
                throw new KeyNotFoundException($"Entity with ID {id} not found");
            _store[id] = entity;
            return entity;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        _lock.EnterWriteLock();
        try
        {
            return _store.Remove(id);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.ContainsKey(id);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    protected string GetId(T entity)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty?.GetValue(entity) is string id)
            return id;
        throw new InvalidOperationException($"Entity {entity.GetType().Name} must have an Id property");
    }
}

/// <summary>
/// In-memory implementation of location data repository.
/// </summary>
public class InMemoryLocationDataRepository : InMemoryRepository<LocationData>, ILocationDataRepository
{
    public async Task<IEnumerable<LocationData>> GetByDeviceIdAsync(string deviceId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(l => l.DeviceId == deviceId).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<LocationData>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(l => l.Timestamp >= start && l.Timestamp <= end).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<LocationData>> GetByDeviceAndTimeRangeAsync(string deviceId, DateTime start, DateTime end)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values
                .Where(l => l.DeviceId == deviceId && l.Timestamp >= start && l.Timestamp <= end)
                .OrderBy(l => l.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<LocationData?> GetLatestByDeviceIdAsync(string deviceId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values
                .Where(l => l.DeviceId == deviceId)
                .OrderByDescending(l => l.Timestamp)
                .FirstOrDefault();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<LocationData>> GetWithinRadiusAsync(double latitude, double longitude, double radiusKm)
    {
        var center = new LocationData { Latitude = latitude, Longitude = longitude };
        _lock.EnterReadLock();
        try
        {
            return _store.Values
                .Where(l => center.DistanceTo(l) <= radiusKm)
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<int> DeleteOlderThanAsync(DateTime dateTime)
    {
        _lock.EnterWriteLock();
        try
        {
            var keysToDelete = _store
                .Where(kvp => kvp.Value.Timestamp < dateTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToDelete)
                _store.Remove(key);

            return keysToDelete.Count;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
