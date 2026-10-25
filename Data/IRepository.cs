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
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching entity, or <c>null</c> if none exists.</returns>
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities in the repository.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>All stored entities.</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a new entity.
    /// </summary>
    /// <param name="entity">The entity to store.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created entity.</returns>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns><c>true</c> if the entity was deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Checks whether an entity with the given identifier exists.
    /// </summary>
    /// <param name="id">The identifier to check.</param>
    /// <returns><c>true</c> if an entity exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Adds a new entity. Convenience alias for <see cref="CreateAsync"/>.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default) => CreateAsync(entity, cancellationToken);

    /// <summary>
    /// Finds all entities matching the given predicate.
    /// </summary>
    async Task<IEnumerable<T>> FindManyAsync(Func<T, bool> predicate)
    {
        var all = await GetAllAsync().ConfigureAwait(false);
        return all.Where(predicate);
    }
}

/// <summary>
/// Repository for location data operations.
/// </summary>
public interface ILocationDataRepository : IRepository<LocationData>
{
    /// <summary>
    /// Gets location records for a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Location records belonging to the device.</returns>
    Task<IEnumerable<LocationData>> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location records recorded within a time range.
    /// </summary>
    /// <param name="start">The start of the time range.</param>
    /// <param name="end">The end of the time range.</param>
    /// <returns>Location records within the range.</returns>
    Task<IEnumerable<LocationData>> GetByTimeRangeAsync(DateTime start, DateTime end);

    /// <summary>
    /// Gets location records for a device recorded within a time range.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="start">The start of the time range.</param>
    /// <param name="end">The end of the time range.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Matching location records for the device within the range.</returns>
    Task<IEnumerable<LocationData>> GetByDeviceAndTimeRangeAsync(string deviceId, DateTime start, DateTime end, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent location record for a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The latest location record, or <c>null</c> if the device has none.</returns>
    Task<LocationData?> GetLatestByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location records within a radius of a geographic point.
    /// </summary>
    /// <param name="latitude">Latitude of the center point, in degrees.</param>
    /// <param name="longitude">Longitude of the center point, in degrees.</param>
    /// <param name="radiusKm">Search radius, in kilometers.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Location records within the radius.</returns>
    Task<IEnumerable<LocationData>> GetWithinRadiusAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes location records recorded before the given date and time.
    /// </summary>
    /// <param name="dateTime">Records older than this value are deleted.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The number of deleted records.</returns>
    Task<int> DeleteOlderThanAsync(DateTime dateTime, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for device operations.
/// </summary>
public interface IDeviceRepository : IRepository<Device>
{
    /// <summary>
    /// Gets a device by its IMEI.
    /// </summary>
    /// <param name="imei">The IMEI of the device.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching device, or <c>null</c> if none exists.</returns>
    Task<Device?> GetByImeiAsync(string imei, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets devices with the given status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Devices with the specified status.</returns>
    Task<IEnumerable<Device>> GetByStatusAsync(DeviceStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets devices that use the given protocol.
    /// </summary>
    /// <param name="protocol">The protocol to filter by.</param>
    /// <returns>Devices using the specified protocol.</returns>
    Task<IEnumerable<Device>> GetByProtocolAsync(ProtocolType protocol);

    /// <summary>
    /// Gets devices that are currently active.
    /// </summary>
    /// <returns>Active devices.</returns>
    Task<IEnumerable<Device>> GetActiveDevicesAsync();

    /// <summary>
    /// Gets the total number of devices.
    /// </summary>
    /// <returns>The number of devices in the repository.</returns>
    Task<int> GetTotalCountAsync();

    /// <summary>
    /// Gets devices that have not been seen within the given timeout.
    /// </summary>
    /// <param name="timeout">The inactivity period after which a device is considered offline.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Devices considered offline.</returns>
    Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for journey/trip operations.
/// </summary>
public interface IJourneyRepository : IRepository<Journey>
{
    /// <summary>
    /// Gets journeys for a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>Journeys belonging to the device.</returns>
    Task<IEnumerable<Journey>> GetByDeviceIdAsync(string deviceId);

    /// <summary>
    /// Gets journeys that have been completed.
    /// </summary>
    /// <returns>Completed journeys.</returns>
    Task<IEnumerable<Journey>> GetCompletedAsync();

    /// <summary>
    /// Gets journeys that started within a time range.
    /// </summary>
    /// <param name="start">The start of the time range.</param>
    /// <param name="end">The end of the time range.</param>
    /// <returns>Journeys within the range.</returns>
    Task<IEnumerable<Journey>> GetByTimeRangeAsync(DateTime start, DateTime end);

    /// <summary>
    /// Gets the journey currently in progress for a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>The ongoing journey, or <c>null</c> if the device has none.</returns>
    Task<Journey?> GetOngoingJourneyAsync(string deviceId);

    /// <summary>
    /// Gets the total distance traveled across all journeys of a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>The total distance of the device's journeys.</returns>
    Task<double> GetTotalDistanceAsync(string deviceId);

    /// <summary>
    /// Deletes journeys that started before the given date and time.
    /// </summary>
    /// <param name="dateTime">Journeys older than this value are deleted.</param>
    /// <returns>The number of deleted journeys.</returns>
    Task<int> DeleteOlderThanAsync(DateTime dateTime);
}

/// <summary>
/// Repository for command operations.
/// </summary>
public interface ICommandRepository : IRepository<Command>
{
    /// <summary>
    /// Gets commands sent to a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>Commands targeting the device.</returns>
    Task<IEnumerable<Command>> GetByDeviceIdAsync(string deviceId);

    /// <summary>
    /// Gets commands that are still pending.
    /// </summary>
    /// <returns>Pending commands.</returns>
    Task<IEnumerable<Command>> GetPendingAsync();

    /// <summary>
    /// Gets commands with the given status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <returns>Commands with the specified status.</returns>
    Task<IEnumerable<Command>> GetByStatusAsync(CommandStatus status);

    /// <summary>
    /// Gets commands that have exceeded the given timeout.
    /// </summary>
    /// <param name="timeout">The period after which a command is considered expired.</param>
    /// <returns>Expired commands.</returns>
    Task<IEnumerable<Command>> GetExpiredAsync(TimeSpan timeout);

    /// <summary>
    /// Deletes commands created before the given date and time.
    /// </summary>
    /// <param name="dateTime">Commands older than this value are deleted.</param>
    /// <returns>The number of deleted commands.</returns>
    Task<int> DeleteOlderThanAsync(DateTime dateTime);
}

/// <summary>
/// Repository for response message operations.
/// </summary>
public interface IResponseMessageRepository : IRepository<ResponseMessage>
{
    /// <summary>
    /// Gets response messages received from a device.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>Response messages from the device.</returns>
    Task<IEnumerable<ResponseMessage>> GetByDeviceIdAsync(string deviceId);

    /// <summary>
    /// Gets response messages that answer a given command.
    /// </summary>
    /// <param name="commandId">The identifier of the command.</param>
    /// <returns>Response messages for the command.</returns>
    Task<IEnumerable<ResponseMessage>> GetByCommandIdAsync(string commandId);

    /// <summary>
    /// Gets response messages received within a time range.
    /// </summary>
    /// <param name="start">The start of the time range.</param>
    /// <param name="end">The end of the time range.</param>
    /// <returns>Response messages within the range.</returns>
    Task<IEnumerable<ResponseMessage>> GetByTimeRangeAsync(DateTime start, DateTime end);

    /// <summary>
    /// Gets response messages that represent errors.
    /// </summary>
    /// <returns>Error response messages.</returns>
    Task<IEnumerable<ResponseMessage>> GetErrorMessagesAsync();

    /// <summary>
    /// Deletes response messages received before the given date and time.
    /// </summary>
    /// <param name="dateTime">Messages older than this value are deleted.</param>
    /// <returns>The number of deleted messages.</returns>
    Task<int> DeleteOlderThanAsync(DateTime dateTime);
}

/// <summary>
/// Unit of work pattern for coordinating multiple repositories.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets the location data repository.
    /// </summary>
    ILocationDataRepository LocationData { get; }

    /// <summary>
    /// Gets the device repository.
    /// </summary>
    IDeviceRepository Devices { get; }

    /// <summary>
    /// Gets the journey repository.
    /// </summary>
    IJourneyRepository Journeys { get; }

    /// <summary>
    /// Gets the command repository.
    /// </summary>
    ICommandRepository Commands { get; }

    /// <summary>
    /// Gets the response message repository.
    /// </summary>
    IResponseMessageRepository ResponseMessages { get; }

    /// <summary>
    /// Persists all pending changes.
    /// </summary>
    /// <returns>The number of affected records.</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begins a transaction.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackAsync();
}
