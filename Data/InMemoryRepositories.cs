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
/// In-memory implementation of device repository.
/// </summary>
public class InMemoryDeviceRepository : IDeviceRepository
{
    private readonly ConcurrentDictionary<string, Device> _store = new();
    private readonly SemaphoreSlim _evictionLock = new(1, 1);
    private readonly RepositoryRetentionPolicy? _retentionPolicy;

    public InMemoryDeviceRepository()
    {
        _retentionPolicy = null;
    }

    public InMemoryDeviceRepository(RepositoryRetentionPolicy? retentionPolicy)
    {
        _retentionPolicy = retentionPolicy;
    }

    public async Task<Device?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryGetValue(id, out var entity) ? CreateSnapshot(entity) : null;
    }

    public async Task<IEnumerable<Device>> GetAllAsync()
    {
        return _store.Values.Select(CreateSnapshot).ToList();
    }

    public async Task<Device> CreateAsync(Device entity)
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

    public async Task<Device> UpdateAsync(Device entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        _store.AddOrUpdate(id,
            (key) => throw new KeyNotFoundException($"Entity with ID {id} not found"),
            (key, old) => snapshot);
        return snapshot;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryRemove(id, out _);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.ContainsKey(id);
    }

    public async Task<Device?> GetByImeiAsync(string imei)
    {
        return _store.Values.FirstOrDefault(d => d.Imei == imei) is Device device ? CreateSnapshot(device) : null;
    }

    public async Task<IEnumerable<Device>> GetByStatusAsync(DeviceStatus status)
    {
        return _store.Values.Where(d => d.Status == status).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<Device>> GetByProtocolAsync(ProtocolType protocol)
    {
        return _store.Values.Where(d => d.Protocol == protocol).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<Device>> GetActiveDevicesAsync()
    {
        return _store.Values.Where(d => d.IsActive).Select(CreateSnapshot).ToList();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return _store.Count;
    }

    public async Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout)
    {
        return _store.Values.Where(d => d.IsOffline(timeout)).Select(CreateSnapshot).ToList();
    }

    protected virtual async Task EvictIfNeededAsync(string id, Device? entity = null)
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

    protected virtual async Task EvictByMaxAgeAsync(Device entity)
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

    protected virtual async Task EvictByMaxPointsAsync()
    {
        var maxPoints = _retentionPolicy?.MaxPointsPerDevice ?? 0;
        if (maxPoints <= 0)
        {
            return;
        }

        await EvictByMaxPointsGloballyAsync().ConfigureAwait(false);
    }

    protected virtual async Task EvictByMaxPointsGloballyAsync()
    {
        var maxPoints = _retentionPolicy?.MaxPointsPerDevice ?? 0;
        if (maxPoints <= 0)
        {
            return;
        }

        while (_store.Count > maxPoints)
        {
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

    protected virtual DateTime GetTimestamp(Device entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.RegistrationDate;
    }

    protected virtual Device CreateSnapshot(Device entity)
    {
        if (entity == null)
        {
            return null!;
        }

        var copy = (Device)entity.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(entity, null);
        return copy!;
    }

    protected string GetId(Device entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Id ?? throw new InvalidOperationException($"Device must have an Id");
    }
}

/// <summary>
/// In-memory implementation of journey repository.
/// </summary>
public class InMemoryJourneyRepository : IJourneyRepository
{
    private readonly ConcurrentDictionary<string, Journey> _store = new();
    private readonly SemaphoreSlim _deleteLock = new(1, 1);
    private readonly RepositoryRetentionPolicy? _retentionPolicy;

    public InMemoryJourneyRepository()
    {
        _retentionPolicy = null;
    }

    public InMemoryJourneyRepository(RepositoryRetentionPolicy? retentionPolicy)
    {
        _retentionPolicy = retentionPolicy;
    }

    public async Task<Journey?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryGetValue(id, out var entity) ? CreateSnapshot(entity) : null;
    }

    public async Task<IEnumerable<Journey>> GetAllAsync()
    {
        return _store.Values.Select(CreateSnapshot).ToList();
    }

    public async Task<Journey> CreateAsync(Journey entity)
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

    public async Task<Journey> UpdateAsync(Journey entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        _store.AddOrUpdate(id,
            (key) => throw new KeyNotFoundException($"Entity with ID {id} not found"),
            (key, old) => snapshot);
        return snapshot;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryRemove(id, out _);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.ContainsKey(id);
    }

    public async Task<IEnumerable<Journey>> GetByDeviceIdAsync(string deviceId)
    {
        return _store.Values.Where(j => j.DeviceId == deviceId).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<Journey>> GetCompletedAsync()
    {
        return _store.Values.Where(j => j.Status == 1).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<Journey>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        return _store.Values
            .Where(j => j.StartTime >= start && (j.EndTime is null || j.EndTime <= end))
            .Select(CreateSnapshot)
            .ToList();
    }

    public async Task<Journey?> GetOngoingJourneyAsync(string deviceId)
    {
        return _store.Values.FirstOrDefault(j => j.DeviceId == deviceId && j.Status == 0) is Journey journey ? CreateSnapshot(journey) : null;
    }

    public async Task<double> GetTotalDistanceAsync(string deviceId)
    {
        return _store.Values
            .Where(j => j.DeviceId == deviceId && j.Status == 1)
            .Sum(j => j.GetTotalDistance());
    }

    public async Task<int> DeleteOlderThanAsync(DateTime dateTime)
    {
        await _deleteLock.WaitAsync();
        try
        {
            var keysToDelete = _store
                .Where(kvp => kvp.Value.StartTime < dateTime)
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

    protected virtual async Task EvictIfNeededAsync(string id, Journey? entity = null)
    {
        if (_retentionPolicy == null || !_retentionPolicy.IsEnabled)
        {
            return;
        }

        await Task.CompletedTask;
    }

    protected virtual Journey CreateSnapshot(Journey entity)
    {
        if (entity == null)
        {
            return null!;
        }

        var copy = (Journey)entity.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(entity, null);
        return copy!;
    }

    protected string GetId(Journey entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Id ?? throw new InvalidOperationException($"Journey must have an Id");
    }
}

/// <summary>
/// In-memory implementation of command repository.
/// </summary>
public class InMemoryCommandRepository : ICommandRepository
{
    private readonly ConcurrentDictionary<string, Command> _store = new();
    private readonly SemaphoreSlim _deleteLock = new(1, 1);
    private readonly RepositoryRetentionPolicy? _retentionPolicy;

    public InMemoryCommandRepository()
    {
        _retentionPolicy = null;
    }

    public InMemoryCommandRepository(RepositoryRetentionPolicy? retentionPolicy)
    {
        _retentionPolicy = retentionPolicy;
    }

    public async Task<Command?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryGetValue(id, out var entity) ? CreateSnapshot(entity) : null;
    }

    public async Task<IEnumerable<Command>> GetAllAsync()
    {
        return _store.Values.Select(CreateSnapshot).ToList();
    }

    public async Task<Command> CreateAsync(Command entity)
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

    public async Task<Command> UpdateAsync(Command entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        _store.AddOrUpdate(id,
            (key) => throw new KeyNotFoundException($"Entity with ID {id} not found"),
            (key, old) => snapshot);
        return snapshot;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryRemove(id, out _);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.ContainsKey(id);
    }

    public async Task<IEnumerable<Command>> GetByDeviceIdAsync(string deviceId)
    {
        return _store.Values.Where(c => c.DeviceId == deviceId).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<Command>> GetPendingAsync()
    {
        return _store.Values.Where(c => c.Status == CommandStatus.Pending).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<Command>> GetByStatusAsync(CommandStatus status)
    {
        return _store.Values.Where(c => c.Status == status).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<Command>> GetExpiredAsync(TimeSpan timeout)
    {
        return _store.Values
            .Where(c => c.Status == CommandStatus.Pending && DateTime.UtcNow - c.CreatedAt > timeout)
            .Select(CreateSnapshot)
            .ToList();
    }

    public async Task<int> DeleteOlderThanAsync(DateTime dateTime)
    {
        await _deleteLock.WaitAsync();
        try
        {
            var keysToDelete = _store
                .Where(kvp => kvp.Value.CreatedAt < dateTime)
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

    protected virtual async Task EvictIfNeededAsync(string id, Command? entity = null)
    {
        if (_retentionPolicy == null || !_retentionPolicy.IsEnabled)
        {
            return;
        }

        await Task.CompletedTask;
    }

    protected virtual Command CreateSnapshot(Command entity)
    {
        if (entity == null)
        {
            return null!;
        }

        var copy = (Command)entity.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(entity, null);
        return copy!;
    }

    protected string GetId(Command entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Id ?? throw new InvalidOperationException($"Command must have an Id");
    }
}

/// <summary>
/// In-memory implementation of response message repository.
/// </summary>
public class InMemoryResponseMessageRepository : IResponseMessageRepository
{
    private readonly ConcurrentDictionary<string, ResponseMessage> _store = new();
    private readonly SemaphoreSlim _deleteLock = new(1, 1);
    private readonly RepositoryRetentionPolicy? _retentionPolicy;

    public InMemoryResponseMessageRepository()
    {
        _retentionPolicy = null;
    }

    public InMemoryResponseMessageRepository(RepositoryRetentionPolicy? retentionPolicy)
    {
        _retentionPolicy = retentionPolicy;
    }

    public async Task<ResponseMessage?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryGetValue(id, out var entity) ? CreateSnapshot(entity) : null;
    }

    public async Task<IEnumerable<ResponseMessage>> GetAllAsync()
    {
        return _store.Values.Select(CreateSnapshot).ToList();
    }

    public async Task<ResponseMessage> CreateAsync(ResponseMessage entity)
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

    public async Task<ResponseMessage> UpdateAsync(ResponseMessage entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetId(entity);
        var snapshot = CreateSnapshot(entity);
        _store.AddOrUpdate(id,
            (key) => throw new KeyNotFoundException($"Entity with ID {id} not found"),
            (key, old) => snapshot);
        return snapshot;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.TryRemove(id, out _);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _store.ContainsKey(id);
    }

    public async Task<IEnumerable<ResponseMessage>> GetByDeviceIdAsync(string deviceId)
    {
        return _store.Values.Where(r => r.DeviceId == deviceId).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<ResponseMessage>> GetByCommandIdAsync(string commandId)
    {
        return _store.Values.Where(r => r.CommandId == commandId).Select(CreateSnapshot).ToList();
    }

    public async Task<IEnumerable<ResponseMessage>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        return _store.Values
            .Where(r => r.ReceivedAt >= start && r.ReceivedAt <= end)
            .Select(CreateSnapshot)
            .ToList();
    }

    public async Task<IEnumerable<ResponseMessage>> GetErrorMessagesAsync()
    {
        return _store.Values.Where(r => !r.IsSuccess).Select(CreateSnapshot).ToList();
    }

    public async Task<int> DeleteOlderThanAsync(DateTime dateTime)
    {
        await _deleteLock.WaitAsync();
        try
        {
            var keysToDelete = _store
                .Where(kvp => kvp.Value.ReceivedAt < dateTime)
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

    protected virtual async Task EvictIfNeededAsync(string id, ResponseMessage? entity = null)
    {
        if (_retentionPolicy == null || !_retentionPolicy.IsEnabled)
        {
            return;
        }

        await Task.CompletedTask;
    }

    protected virtual ResponseMessage CreateSnapshot(ResponseMessage entity)
    {
        if (entity == null)
        {
            return null!;
        }

        var copy = (ResponseMessage)entity.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(entity, null);
        return copy!;
    }

    protected string GetId(ResponseMessage entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return entity.Id ?? throw new InvalidOperationException($"ResponseMessage must have an Id");
    }
}

/// <summary>
/// In-memory implementation of unit of work pattern.
/// </summary>
public class InMemoryUnitOfWork : IUnitOfWork
{
    public ILocationDataRepository LocationData { get; }
    public IDeviceRepository Devices { get; }
    public IJourneyRepository Journeys { get; }
    public ICommandRepository Commands { get; }
    public IResponseMessageRepository ResponseMessages { get; }

    public InMemoryUnitOfWork()
    {
        LocationData = new InMemoryLocationDataRepository();
        Devices = new InMemoryDeviceRepository();
        Journeys = new InMemoryJourneyRepository();
        Commands = new InMemoryCommandRepository();
        ResponseMessages = new InMemoryResponseMessageRepository();
    }

    public async Task<int> SaveChangesAsync() => 1;
    public async Task BeginTransactionAsync() { }
    public async Task CommitAsync() { }
    public async Task RollbackAsync() { }
    public async ValueTask DisposeAsync() { }
}
