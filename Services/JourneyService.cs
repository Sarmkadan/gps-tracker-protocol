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
    Task<Journey> StartJourneyAsync(string deviceId);
    Task<Journey?> GetOngoingJourneyAsync(string deviceId);
    Task<bool> AddWaypointAsync(string journeyId, LocationData location);
    Task<Journey> CompleteJourneyAsync(string journeyId);
    Task<IEnumerable<Journey>> GetJourneyHistoryAsync(string deviceId);
    Task<Journey?> GetJourneyAsync(string journeyId);
    Task<double> GetTotalDistanceAsync(string deviceId);
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

        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        var existingJourney = await _journeyRepository.GetOngoingJourneyAsync(deviceId);
        if (existingJourney != null)
            throw new InvalidOperationException($"Device {deviceId} already has an ongoing journey");

        var journey = new Journey
        {
            DeviceId = deviceId,
            StartTime = DateTime.UtcNow,
            Status = 0 // ongoing
        };

        return await _journeyRepository.CreateAsync(journey);
    }

    /// <summary>
    /// Gets the ongoing journey for a device.
    /// </summary>
    public async Task<Journey?> GetOngoingJourneyAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _journeyRepository.GetOngoingJourneyAsync(deviceId);
    }

    /// <summary>
    /// Adds a waypoint to an ongoing journey.
    /// </summary>
    public async Task<bool> AddWaypointAsync(string journeyId, LocationData location)
    {
        if (string.IsNullOrWhiteSpace(journeyId))
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));

        if (location == null)
            throw new ArgumentNullException(nameof(location));

        if (!location.IsValid())
            throw new ValidationException("Location data validation failed");

        var journey = await _journeyRepository.GetByIdAsync(journeyId);
        if (journey == null)
            throw new InvalidOperationException($"Journey {journeyId} not found");

        if (journey.Status != 0)
            throw new InvalidOperationException($"Journey {journeyId} is not ongoing");

        if (journey.Waypoints.Count >= ConfigConstants.MAX_JOURNEY_WAYPOINTS)
            throw new InvalidOperationException("Journey has reached maximum waypoint limit");

        journey.AddWaypoint(location);
        await _journeyRepository.UpdateAsync(journey);
        return true;
    }

    /// <summary>
    /// Completes a journey and calculates summary metrics.
    /// </summary>
    public async Task<Journey> CompleteJourneyAsync(string journeyId)
    {
        var journey = await _journeyRepository.GetByIdAsync(journeyId);
        if (journey == null)
            throw new InvalidOperationException($"Journey {journeyId} not found");

        if (journey.Status != 0)
            throw new InvalidOperationException($"Journey {journeyId} is not ongoing");

        journey.Complete();
        await _journeyRepository.UpdateAsync(journey);
        return journey;
    }

    /// <summary>
    /// Gets journey history for a device.
    /// </summary>
    public async Task<IEnumerable<Journey>> GetJourneyHistoryAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var journeys = await _journeyRepository.GetByDeviceIdAsync(deviceId);
        return journeys.OrderByDescending(j => j.StartTime).ToList();
    }

    /// <summary>
    /// Gets a specific journey by ID.
    /// </summary>
    public async Task<Journey?> GetJourneyAsync(string journeyId)
    {
        if (string.IsNullOrWhiteSpace(journeyId))
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));

        return await _journeyRepository.GetByIdAsync(journeyId);
    }

    /// <summary>
    /// Calculates total distance traveled by a device.
    /// </summary>
    public async Task<double> GetTotalDistanceAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _journeyRepository.GetTotalDistanceAsync(deviceId);
    }

    /// <summary>
    /// Cleans up old journey records.
    /// </summary>
    public async Task<int> CleanupOldJourneysAsync(DateTime olderThan)
    {
        if (olderThan >= DateTime.UtcNow)
            throw new ArgumentException("Cleanup date must be in the past");

        return await _journeyRepository.DeleteOlderThanAsync(olderThan);
    }
}
