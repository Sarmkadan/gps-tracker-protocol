#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Generic repository interface for CRUD operations.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

/// <summary>
/// Repository for location data operations.
/// </summary>
public interface ILocationDataRepository : IRepository<LocationData>
{
    Task<IEnumerable<LocationData>> GetByDeviceIdAsync(string deviceId);
    Task<IEnumerable<LocationData>> GetByTimeRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<LocationData>> GetByDeviceAndTimeRangeAsync(string deviceId, DateTime start, DateTime end);
    Task<LocationData?> GetLatestByDeviceIdAsync(string deviceId);
    Task<IEnumerable<LocationData>> GetWithinRadiusAsync(double latitude, double longitude, double radiusKm);
    Task<int> DeleteOlderThanAsync(DateTime dateTime);
}

/// <summary>
/// Repository for device operations.
/// </summary>
public interface IDeviceRepository : IRepository<Device>
{
    Task<Device?> GetByImeiAsync(string imei);
    Task<IEnumerable<Device>> GetByStatusAsync(DeviceStatus status);
    Task<IEnumerable<Device>> GetByProtocolAsync(ProtocolType protocol);
    Task<IEnumerable<Device>> GetActiveDevicesAsync();
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout);
}

/// <summary>
/// Repository for journey/trip operations.
/// </summary>
public interface IJourneyRepository : IRepository<Journey>
{
    Task<IEnumerable<Journey>> GetByDeviceIdAsync(string deviceId);
    Task<IEnumerable<Journey>> GetCompletedAsync();
    Task<IEnumerable<Journey>> GetByTimeRangeAsync(DateTime start, DateTime end);
    Task<Journey?> GetOngoingJourneyAsync(string deviceId);
    Task<double> GetTotalDistanceAsync(string deviceId);
    Task<int> DeleteOlderThanAsync(DateTime dateTime);
}

/// <summary>
/// Repository for command operations.
/// </summary>
public interface ICommandRepository : IRepository<Command>
{
    Task<IEnumerable<Command>> GetByDeviceIdAsync(string deviceId);
    Task<IEnumerable<Command>> GetPendingAsync();
    Task<IEnumerable<Command>> GetByStatusAsync(CommandStatus status);
    Task<IEnumerable<Command>> GetExpiredAsync(TimeSpan timeout);
    Task<int> DeleteOlderThanAsync(DateTime dateTime);
}

/// <summary>
/// Repository for response message operations.
/// </summary>
public interface IResponseMessageRepository : IRepository<ResponseMessage>
{
    Task<IEnumerable<ResponseMessage>> GetByDeviceIdAsync(string deviceId);
    Task<IEnumerable<ResponseMessage>> GetByCommandIdAsync(string commandId);
    Task<IEnumerable<ResponseMessage>> GetByTimeRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<ResponseMessage>> GetErrorMessagesAsync();
    Task<int> DeleteOlderThanAsync(DateTime dateTime);
}

/// <summary>
/// Unit of work pattern for coordinating multiple repositories.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    ILocationDataRepository LocationData { get; }
    IDeviceRepository Devices { get; }
    IJourneyRepository Journeys { get; }
    ICommandRepository Commands { get; }
    IResponseMessageRepository ResponseMessages { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
