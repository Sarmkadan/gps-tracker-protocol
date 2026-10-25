#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Service for managing location data.
/// </summary>
public interface ILocationDataService
{
    /// <summary>
    /// Stores a new location data point.
    /// </summary>
    /// <param name="location">The location data to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stored location data.</returns>
    Task<LocationData> StoreLocationAsync(LocationData location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest location for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The latest location data if found, otherwise null.</returns>
    Task<LocationData?> GetLatestLocationAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location history for a device with limit.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="limit">The maximum number of records to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of location data.</returns>
    Task<IEnumerable<LocationData>> GetLocationHistoryAsync(string deviceId, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location data within a time range.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of location data.</returns>
    Task<IEnumerable<LocationData>> GetLocationsByTimeRangeAsync(string deviceId, DateTime start, DateTime end, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations within a specified radius.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="radiusKm">The radius in kilometers.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of location data.</returns>
    Task<IEnumerable<LocationData>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates total travel distance for a device in a time period.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The total distance traveled in kilometers.</returns>
    Task<double> CalculateTravelDistanceAsync(string deviceId, DateTime start, DateTime end, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old location data.
    /// </summary>
    /// <param name="olderThan">The threshold date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of records deleted.</returns>
    Task<int> CleanupOldDataAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of location data service.
/// </summary>
public class LocationDataService : ILocationDataService
{
    private const int DEFAULT_HISTORY_LIMIT = 100;
    private const int MIN_LOCATIONS_FOR_DISTANCE = 2;

    private const string DEVICE_ID_EMPTY_MESSAGE = "Device ID cannot be empty";
    private const string LOCATION_VALIDATION_FAILED_MESSAGE = "Location data validation failed";
    private const string LIMIT_NOT_POSITIVE_MESSAGE = "Limit must be greater than 0";
    private const string START_NOT_BEFORE_END_MESSAGE = "Start time must be before end time";
    private const string INVALID_LATITUDE_MESSAGE = "Invalid latitude";
    private const string INVALID_LONGITUDE_MESSAGE = "Invalid longitude";
    private const string RADIUS_NOT_POSITIVE_MESSAGE = "Radius must be positive";
    private const string CLEANUP_DATE_NOT_PAST_MESSAGE = "Cleanup date must be in the past";

    private readonly ILocationDataRepository _repository;
    private readonly IDeviceRepository _deviceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationDataService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work providing location data and device repositories.</param>
    public LocationDataService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.LocationData;
        _deviceRepository = unitOfWork.Devices;
    }

    /// <summary>
    /// Stores a new location data point.
    /// </summary>
    /// <param name="location">The location data to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stored location data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="location"/> is null.</exception>
    /// <exception cref="ValidationException">Thrown when the location data fails validation.</exception>
    /// <exception cref="DeviceException">Thrown when the device referenced by the location does not exist.</exception>
    public async Task<LocationData> StoreLocationAsync(LocationData location, CancellationToken cancellationToken = default)
    {
        if (location is null)
            throw new ArgumentNullException(nameof(location));

        if (!location.IsValid())
            throw new ValidationException(LOCATION_VALIDATION_FAILED_MESSAGE, nameof(location));

        var device = await _deviceRepository.GetByIdAsync(location.DeviceId, cancellationToken).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {location.DeviceId} not found", location.DeviceId);

        if (location.Timestamp == default)
            location.Timestamp = DateTime.UtcNow;

        var stored = await _repository.CreateAsync(location, cancellationToken).ConfigureAwait(false);

        // Update device location cache
        device.UpdateHeartbeat();
        await _deviceRepository.UpdateAsync(device, cancellationToken).ConfigureAwait(false);

        return stored;
    }

    /// <summary>
    /// Gets the latest location for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The latest location data if found, otherwise null.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="deviceId"/> is null, empty, or whitespace.</exception>
    public async Task<LocationData?> GetLatestLocationAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException(DEVICE_ID_EMPTY_MESSAGE, nameof(deviceId));

        return await _repository.GetLatestByDeviceIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets location history for a device with limit.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="limit">The maximum number of records to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of location data, ordered by timestamp descending.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="deviceId"/> is null, empty, or whitespace, or when <paramref name="limit"/> is not greater than 0.</exception>
    public async Task<IEnumerable<LocationData>> GetLocationHistoryAsync(string deviceId, int limit = DEFAULT_HISTORY_LIMIT, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException(DEVICE_ID_EMPTY_MESSAGE, nameof(deviceId));

        if (limit <= 0)
            throw new ArgumentException(LIMIT_NOT_POSITIVE_MESSAGE, nameof(limit));

        var history = await _repository.GetByDeviceIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
        return history.OrderByDescending(l => l.Timestamp).Take(Math.Min(limit, ConfigConstants.MAX_LOCATION_HISTORY)).ToList();
    }

    /// <summary>
    /// Gets location data within a time range.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of location data within the specified range.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="deviceId"/> is null, empty, or whitespace, or when <paramref name="start"/> is not before <paramref name="end"/>.</exception>
    public async Task<IEnumerable<LocationData>> GetLocationsByTimeRangeAsync(string deviceId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException(DEVICE_ID_EMPTY_MESSAGE, nameof(deviceId));

        if (start >= end)
            throw new ArgumentException(START_NOT_BEFORE_END_MESSAGE);

        return await _repository.GetByDeviceAndTimeRangeAsync(deviceId, start, end, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets locations within a specified radius.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="radiusKm">The radius in kilometers.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of location data within the specified radius.</returns>
    /// <exception cref="ValidationException">Thrown when <paramref name="latitude"/> or <paramref name="longitude"/> is out of bounds.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="radiusKm"/> is not positive.</exception>
    public async Task<IEnumerable<LocationData>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default)
    {
        if (latitude < MeasurementBounds.MIN_LATITUDE || latitude > MeasurementBounds.MAX_LATITUDE)
            throw new ValidationException(INVALID_LATITUDE_MESSAGE, nameof(latitude), latitude);

        if (longitude < MeasurementBounds.MIN_LONGITUDE || longitude > MeasurementBounds.MAX_LONGITUDE)
            throw new ValidationException(INVALID_LONGITUDE_MESSAGE, nameof(longitude), longitude);

        if (radiusKm <= 0)
            throw new ArgumentException(RADIUS_NOT_POSITIVE_MESSAGE, nameof(radiusKm));

        return await _repository.GetWithinRadiusAsync(latitude, longitude, radiusKm, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Calculates total travel distance for a device in a time period.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The total distance traveled in kilometers, or 0 when fewer than two locations exist in the period.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="deviceId"/> is null, empty, or whitespace, or when <paramref name="start"/> is not before <paramref name="end"/>.</exception>
    public async Task<double> CalculateTravelDistanceAsync(string deviceId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var locations = await GetLocationsByTimeRangeAsync(deviceId, start, end, cancellationToken).ConfigureAwait(false);
        var sortedLocations = locations.OrderBy(l => l.Timestamp).ToList();

        if (sortedLocations.Count < MIN_LOCATIONS_FOR_DISTANCE)
            return 0;

        double totalDistance = 0;
        for (int i = 0; i < sortedLocations.Count - 1; i++)
        {
            totalDistance += sortedLocations[i].DistanceTo(sortedLocations[i + 1]);
        }

        return totalDistance;
    }

    /// <summary>
    /// Cleans up old location data.
    /// </summary>
    /// <param name="olderThan">The threshold date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of records deleted.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="olderThan"/> is not in the past.</exception>
    public async Task<int> CleanupOldDataAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        if (olderThan >= DateTime.UtcNow)
            throw new ArgumentException(CLEANUP_DATE_NOT_PAST_MESSAGE);

        return await _repository.DeleteOlderThanAsync(olderThan, cancellationToken).ConfigureAwait(false);
    }
}
