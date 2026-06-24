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
    /// <returns>The stored location data.</returns>
    Task<LocationData> StoreLocationAsync(LocationData location);

    /// <summary>
    /// Gets the latest location for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The latest location data if found, otherwise null.</returns>
    Task<LocationData?> GetLatestLocationAsync(string deviceId);

    /// <summary>
    /// Gets location history for a device with limit.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="limit">The maximum number of records to return.</param>
    /// <returns>A collection of location data.</returns>
    Task<IEnumerable<LocationData>> GetLocationHistoryAsync(string deviceId, int limit = 100);

    /// <summary>
    /// Gets location data within a time range.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <returns>A collection of location data.</returns>
    Task<IEnumerable<LocationData>> GetLocationsByTimeRangeAsync(string deviceId, DateTime start, DateTime end);

    /// <summary>
    /// Gets locations within a specified radius.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="radiusKm">The radius in kilometers.</param>
    /// <returns>A collection of location data.</returns>
    Task<IEnumerable<LocationData>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusKm);

    /// <summary>
    /// Calculates total travel distance for a device in a time period.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="start">The start time.</param>
    /// <param name="end">The end time.</param>
    /// <returns>The total distance traveled in kilometers.</returns>
    Task<double> CalculateTravelDistanceAsync(string deviceId, DateTime start, DateTime end);

    /// <summary>
    /// Cleans up old location data.
    /// </summary>
    /// <param name="olderThan">The threshold date.</param>
    /// <returns>The number of records deleted.</returns>
    Task<int> CleanupOldDataAsync(DateTime olderThan);
}

/// <summary>
/// Implementation of location data service.
/// </summary>
public class LocationDataService : ILocationDataService
{
    private readonly ILocationDataRepository _repository;
    private readonly IDeviceRepository _deviceRepository;

    public LocationDataService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.LocationData;
        _deviceRepository = unitOfWork.Devices;
    }

    /// <summary>
    /// Stores a new location data point.
    /// </summary>
    public async Task<LocationData> StoreLocationAsync(LocationData location)
    {
        if (location is null)
            throw new ArgumentNullException(nameof(location));

        if (!location.IsValid())
            throw new ValidationException("Location data validation failed", nameof(location));

        var device = await _deviceRepository.GetByIdAsync(location.DeviceId).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {location.DeviceId} not found", location.DeviceId);

        if (location.Timestamp == default)
            location.Timestamp = DateTime.UtcNow;

        var stored = await _repository.CreateAsync(location).ConfigureAwait(false);

        // Update device location cache
        device.UpdateHeartbeat();
        await _deviceRepository.UpdateAsync(device).ConfigureAwait(false);

        return stored;
    }

    /// <summary>
    /// Gets the latest location for a device.
    /// </summary>
    public async Task<LocationData?> GetLatestLocationAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _repository.GetLatestByDeviceIdAsync(deviceId).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets location history for a device with limit.
    /// </summary>
    public async Task<IEnumerable<LocationData>> GetLocationHistoryAsync(string deviceId, int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        if (limit <= 0)
            throw new ArgumentException("Limit must be greater than 0", nameof(limit));

        var history = await _repository.GetByDeviceIdAsync(deviceId).ConfigureAwait(false);
        return history.OrderByDescending(l => l.Timestamp).Take(Math.Min(limit, ConfigConstants.MAX_LOCATION_HISTORY)).ToList();
    }

    /// <summary>
    /// Gets location data within a time range.
    /// </summary>
    public async Task<IEnumerable<LocationData>> GetLocationsByTimeRangeAsync(string deviceId, DateTime start, DateTime end)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        if (start >= end)
            throw new ArgumentException("Start time must be before end time");

        return await _repository.GetByDeviceAndTimeRangeAsync(deviceId, start, end).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets locations within a specified radius.
    /// </summary>
    public async Task<IEnumerable<LocationData>> GetLocationsNearbyAsync(double latitude, double longitude, double radiusKm)
    {
        if (latitude < MeasurementBounds.MIN_LATITUDE || latitude > MeasurementBounds.MAX_LATITUDE)
            throw new ValidationException("Invalid latitude", nameof(latitude), latitude);

        if (longitude < MeasurementBounds.MIN_LONGITUDE || longitude > MeasurementBounds.MAX_LONGITUDE)
            throw new ValidationException("Invalid longitude", nameof(longitude), longitude);

        if (radiusKm <= 0)
            throw new ArgumentException("Radius must be positive", nameof(radiusKm));

        return await _repository.GetWithinRadiusAsync(latitude, longitude, radiusKm).ConfigureAwait(false);
    }

    /// <summary>
    /// Calculates total travel distance for a device in a time period.
    /// </summary>
    public async Task<double> CalculateTravelDistanceAsync(string deviceId, DateTime start, DateTime end)
    {
        var locations = await GetLocationsByTimeRangeAsync(deviceId, start, end).ConfigureAwait(false);
        var sortedLocations = locations.OrderBy(l => l.Timestamp).ToList();

        if (sortedLocations.Count < 2)
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
    public async Task<int> CleanupOldDataAsync(DateTime olderThan)
    {
        if (olderThan >= DateTime.UtcNow)
            throw new ArgumentException("Cleanup date must be in the past");

        return await _repository.DeleteOlderThanAsync(olderThan).ConfigureAwait(false);
    }
}
