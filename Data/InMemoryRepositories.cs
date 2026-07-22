#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using System.Collections.Concurrent;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// In-memory implementation of device repository.
/// </summary>
public class InMemoryDeviceRepository : InMemoryRepository<Device>, IDeviceRepository
{
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
}

/// <summary>
/// In-memory implementation of journey repository.
/// </summary>
public class InMemoryJourneyRepository : InMemoryRepository<Journey>, IJourneyRepository
{
    private readonly SemaphoreSlim _deleteLock = new(1, 1);

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
}

/// <summary>
/// In-memory implementation of command repository.
/// </summary>
public class InMemoryCommandRepository : InMemoryRepository<Command>, ICommandRepository
{
    private readonly SemaphoreSlim _deleteLock = new(1, 1);

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
}

/// <summary>
/// In-memory implementation of response message repository.
/// </summary>
public class InMemoryResponseMessageRepository : InMemoryRepository<ResponseMessage>, IResponseMessageRepository
{
    private readonly SemaphoreSlim _deleteLock = new(1, 1);

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
