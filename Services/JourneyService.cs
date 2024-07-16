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
/// Service for managing device journeys and trips.
/// </summary>
public interface IJourneyService
{
/// <summary>
    /// Starts a new journey for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The newly created journey.</returns>
    Task<Journey> StartJourneyAsync(string deviceId);

    /// <summary>
    /// Gets the ongoing journey for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The ongoing journey if found, otherwise null.</returns>
    Task<Journey?> GetOngoingJourneyAsync(string deviceId);

    /// <summary>
    /// Adds a waypoint to an ongoing journey.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <param name="location">The location data to add.</param>
    /// <returns>True if successful, otherwise false.</returns>
    Task<bool> AddWaypointAsync(string journeyId, LocationData location);

    /// <summary>
    /// Completes a journey and calculates summary metrics.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <returns>The completed journey.</returns>
    Task<Journey> CompleteJourneyAsync(string journeyId);

    /// <summary>
    /// Gets journey history for a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>A collection of journeys.</returns>
    Task<IEnumerable<Journey>> GetJourneyHistoryAsync(string deviceId);

    /// <summary>
    /// Gets a specific journey by ID.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <returns>The journey if found, otherwise null.</returns>
    Task<Journey?> GetJourneyAsync(string journeyId);

    /// <summary>
    /// Calculates total distance traveled by a device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The total distance in kilometers.</returns>
    Task<double> GetTotalDistanceAsync(string deviceId);

    /// <summary>
    /// Cleans up old journey records.
    /// </summary>
    /// <param name="olderThan">The threshold date.</param>
    /// <returns>The number of journeys deleted.</returns>
    Task<int> CleanupOldJourneysAsync(DateTime olderThan);
}

/// <summary>
/// Implementation of journey service.
/// </summary>
public class JourneyService : IJourneyService
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly ILocationDataRepository _locationRepository;
    private readonly IDeviceRepository _deviceRepository;

    public JourneyService(IUnitOfWork unitOfWork)
    {
        _journeyRepository = unitOfWork.Journeys;
        _locationRepository = unitOfWork.LocationData;
        _deviceRepository = unitOfWork.Devices;
    }

    /// <summary>
    /// Starts a new journey for a device.
    /// </summary>
    public async Task<Journey> StartJourneyAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var device = await _deviceRepository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        var existingJourney = await _journeyRepository.GetOngoingJourneyAsync(deviceId).ConfigureAwait(false);
        if (existingJourney is not null)
            throw new InvalidOperationException($"Device {deviceId} already has an ongoing journey");

        var journey = new Journey
        {
            DeviceId = deviceId,
            StartTime = DateTime.UtcNow,
            Status = 0 // ongoing
        };

        return await _journeyRepository.CreateAsync(journey).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the ongoing journey for a device.
    /// </summary>
    public async Task<Journey?> GetOngoingJourneyAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _journeyRepository.GetOngoingJourneyAsync(deviceId).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a waypoint to an ongoing journey.
    /// </summary>
    public async Task<bool> AddWaypointAsync(string journeyId, LocationData location)
    {
        if (string.IsNullOrWhiteSpace(journeyId))
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));

        if (location is null)
            throw new ArgumentNullException(nameof(location));

        if (!location.IsValid())
            throw new ValidationException("Location data validation failed");

        var journey = await _journeyRepository.GetByIdAsync(journeyId).ConfigureAwait(false);
        if (journey is null)
            throw new InvalidOperationException($"Journey {journeyId} not found");

        if (journey.Status != 0)
            throw new InvalidOperationException($"Journey {journeyId} is not ongoing");

        if (journey.Waypoints.Count >= ConfigConstants.MAX_JOURNEY_WAYPOINTS)
            throw new InvalidOperationException("Journey has reached maximum waypoint limit");

        journey.AddWaypoint(location);
        await _journeyRepository.UpdateAsync(journey).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Completes a journey and calculates summary metrics.
    /// </summary>
    public async Task<Journey> CompleteJourneyAsync(string journeyId)
    {
        var journey = await _journeyRepository.GetByIdAsync(journeyId).ConfigureAwait(false);
        if (journey is null)
            throw new InvalidOperationException($"Journey {journeyId} not found");

        if (journey.Status != 0)
            throw new InvalidOperationException($"Journey {journeyId} is not ongoing");

        journey.Complete();
        await _journeyRepository.UpdateAsync(journey).ConfigureAwait(false);
        return journey;
    }

    /// <summary>
    /// Gets journey history for a device.
    /// </summary>
    public async Task<IEnumerable<Journey>> GetJourneyHistoryAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var journeys = await _journeyRepository.GetByDeviceIdAsync(deviceId).ConfigureAwait(false);
        return journeys.OrderByDescending(j => j.StartTime).ToList();
    }

    /// <summary>
    /// Gets a specific journey by ID.
    /// </summary>
    public async Task<Journey?> GetJourneyAsync(string journeyId)
    {
        if (string.IsNullOrWhiteSpace(journeyId))
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));

        return await _journeyRepository.GetByIdAsync(journeyId).ConfigureAwait(false);
    }

    /// <summary>
    /// Calculates total distance traveled by a device.
    /// </summary>
    public async Task<double> GetTotalDistanceAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _journeyRepository.GetTotalDistanceAsync(deviceId).ConfigureAwait(false);
    }

    /// <summary>
    /// Cleans up old journey records.
    /// </summary>
    public async Task<int> CleanupOldJourneysAsync(DateTime olderThan)
    {
        if (olderThan >= DateTime.UtcNow)
            throw new ArgumentException("Cleanup date must be in the past");

        return await _journeyRepository.DeleteOlderThanAsync(olderThan).ConfigureAwait(false);
    }
}
