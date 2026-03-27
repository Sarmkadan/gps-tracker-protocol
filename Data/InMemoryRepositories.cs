#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// In-memory implementation of device repository.
/// </summary>
public class InMemoryDeviceRepository : InMemoryRepository<Device>, IDeviceRepository
{
    public async Task<Device?> GetByImeiAsync(string imei)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.FirstOrDefault(d => d.Imei == imei);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Device>> GetByStatusAsync(DeviceStatus status)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(d => d.Status == status).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Device>> GetByProtocolAsync(ProtocolType protocol)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(d => d.Protocol == protocol).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Device>> GetActiveDevicesAsync()
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(d => d.IsActive).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<int> GetTotalCountAsync()
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Count;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(d => d.IsOffline(timeout)).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}

/// <summary>
/// In-memory implementation of journey repository.
/// </summary>
public class InMemoryJourneyRepository : InMemoryRepository<Journey>, IJourneyRepository
{
    public async Task<IEnumerable<Journey>> GetByDeviceIdAsync(string deviceId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(j => j.DeviceId == deviceId).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Journey>> GetCompletedAsync()
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(j => j.Status == 1).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Journey>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values
                .Where(j => j.StartTime >= start && (j.EndTime is null || j.EndTime <= end))
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<Journey?> GetOngoingJourneyAsync(string deviceId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.FirstOrDefault(j => j.DeviceId == deviceId && j.Status == 0);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<double> GetTotalDistanceAsync(string deviceId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values
                .Where(j => j.DeviceId == deviceId && j.Status == 1)
                .Sum(j => j.GetTotalDistance());
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
                .Where(kvp => kvp.Value.StartTime < dateTime)
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

/// <summary>
/// In-memory implementation of command repository.
/// </summary>
public class InMemoryCommandRepository : InMemoryRepository<Command>, ICommandRepository
{
    public async Task<IEnumerable<Command>> GetByDeviceIdAsync(string deviceId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(c => c.DeviceId == deviceId).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Command>> GetPendingAsync()
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(c => c.Status == CommandStatus.Pending).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Command>> GetByStatusAsync(CommandStatus status)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(c => c.Status == status).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<Command>> GetExpiredAsync(TimeSpan timeout)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values
                .Where(c => c.Status == CommandStatus.Pending && DateTime.UtcNow - c.CreatedAt > timeout)
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
                .Where(kvp => kvp.Value.CreatedAt < dateTime)
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

/// <summary>
/// In-memory implementation of response message repository.
/// </summary>
public class InMemoryResponseMessageRepository : InMemoryRepository<ResponseMessage>, IResponseMessageRepository
{
    public async Task<IEnumerable<ResponseMessage>> GetByDeviceIdAsync(string deviceId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(r => r.DeviceId == deviceId).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<ResponseMessage>> GetByCommandIdAsync(string commandId)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(r => r.CommandId == commandId).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<ResponseMessage>> GetByTimeRangeAsync(DateTime start, DateTime end)
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values
                .Where(r => r.ReceivedAt >= start && r.ReceivedAt <= end)
                .ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public async Task<IEnumerable<ResponseMessage>> GetErrorMessagesAsync()
    {
        _lock.EnterReadLock();
        try
        {
            return _store.Values.Where(r => !r.IsSuccess).ToList();
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
                .Where(kvp => kvp.Value.ReceivedAt < dateTime)
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
