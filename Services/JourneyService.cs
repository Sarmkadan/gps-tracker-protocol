#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

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

    /// <summary>
    /// Detects idle periods within a journey based on consecutive locations within a small radius.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <param name="maxDistanceMeters">Maximum distance in meters to consider locations as stationary.</param>
    /// <param name="minDurationSeconds">Minimum duration in seconds to consider as idle.</param>
    /// <returns>A collection of idle periods.</returns>
    Task<IEnumerable<IdlePeriod>> DetectIdlePeriodsAsync(string journeyId, double maxDistanceMeters = 25.0, int minDurationSeconds = 300);

    /// <summary>
    /// Gets idle periods for a journey.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <returns>A collection of idle periods.</returns>
    Task<IEnumerable<IdlePeriod>> GetIdlePeriodsAsync(string journeyId);
}

/// <summary>
/// Configuration for journey segmentation and edge-case handling.
/// </summary>
public record JourneySegmentationConfig
{
    /// <summary>
    /// Maximum time gap between points to consider them part of the same journey (in minutes).
    /// Points separated by more than this gap should start a new journey.
    /// </summary>
    public int MaxDataGapMinutes { get; init; } = 30;

    /// <summary>
    /// Duration of stationary period (speed == 0) that should end a journey (in minutes).
    /// </summary>
    public int StationaryDurationMinutes { get; init; } = 30;

    /// <summary>
    /// Maximum reordering window for out-of-order points (in seconds).
    /// Points within this window can be reordered; points outside are rejected.
    /// </summary>
    public int MaxReorderingWindowSeconds { get; init; } = 300;

    /// <summary>
    /// Minimum speed threshold to consider a point as moving (in km/h).
    /// Points below this speed are considered stationary.
    /// </summary>
    public double StationarySpeedThresholdKmh { get; init; } = 1.0;

    /// <summary>
    /// Whether to automatically end journeys when ignition-off is detected.
    /// </summary>
    public bool AutoEndOnIgnitionOff { get; init; } = true;

    /// <summary>
    /// Whether to automatically end journeys when device goes offline (no data for MaxDataGapMinutes).
    /// </summary>
    public bool AutoEndOnDeviceOffline { get; init; } = true;
}

/// <summary>
/// Implementation of journey service.
/// </summary>
public class JourneyService : IJourneyService
{
    private readonly IJourneyRepository _journeyRepository;
    private readonly ILocationDataRepository _locationRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILocationSanityFilter _locationSanityFilter;
    private readonly JourneySegmentationConfig _config;

    public JourneyService(
        IUnitOfWork unitOfWork,
        ILocationSanityFilter? locationSanityFilter = null,
        JourneySegmentationConfig? config = null)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _journeyRepository = unitOfWork.Journeys;
        _locationRepository = unitOfWork.LocationData;
        _deviceRepository = unitOfWork.Devices;
        _locationSanityFilter = locationSanityFilter ?? new LocationSanityFilter();
        _config = config ?? new JourneySegmentationConfig();
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
    /// Adds a waypoint to an ongoing journey with journey segmentation rules.
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

        // Apply GPS fix quality filtering before adding the waypoint
        var previousLocation = journey.Waypoints.Count > 0 ? journey.Waypoints[^1] : null;
        var filterResult = _locationSanityFilter.FilterLocation(location, previousLocation);

        if (!filterResult.IsAccepted)
        {
            // Location was rejected by sanity filter
            // In a real application, this would be logged/metrics would be incremented
            return false;
        }

        // Apply journey segmentation rules
        var segmentationResult = await ApplyJourneySegmentationRulesAsync(journey, location, previousLocation);
        if (!segmentationResult.ShouldAddToCurrentJourney)
        {
            // Journey should be ended and a new one started
            await CompleteJourneyAsync(journey.Id).ConfigureAwait(false);

            // Start a new journey for the same device
            var newJourney = await StartJourneyAsync(journey.DeviceId).ConfigureAwait(false);

            // Add the location to the new journey
            newJourney.AddWaypoint(location);
            await _journeyRepository.UpdateAsync(newJourney).ConfigureAwait(false);

            return true;
        }

        // Handle out-of-order points and duplicates
        if (segmentationResult.IsOutOfOrder || segmentationResult.IsDuplicateTimestamp)
        {
            // For out-of-order or duplicate points, we don't add them to the journey
            // but we still return true to indicate the point was processed
            return true;
        }

        journey.AddWaypoint(location);
        await _journeyRepository.UpdateAsync(journey).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Applies journey segmentation rules to determine if a location should be added to the current journey.
    /// </summary>
    private async Task<JourneySegmentationResult> ApplyJourneySegmentationRulesAsync(
        Journey journey,
        LocationData location,
        LocationData? previousLocation)
    {
        // Check for duplicate timestamp
        if (previousLocation != null && location.Timestamp == previousLocation.Timestamp)
        {
            return new JourneySegmentationResult(
                shouldAddToCurrentJourney: false,
                isOutOfOrder: false,
                isDuplicateTimestamp: true,
                reason: "Duplicate timestamp detected");
        }

        // Check for out-of-order points (with tolerance window)
        if (previousLocation != null && location.Timestamp < previousLocation.Timestamp)
        {
            var timeDifference = (previousLocation.Timestamp - location.Timestamp).TotalSeconds;
            if (timeDifference > _config.MaxReorderingWindowSeconds)
            {
                // Point is too far out of order, reject it
                return new JourneySegmentationResult(
                    shouldAddToCurrentJourney: false,
                    isOutOfOrder: true,
                    isDuplicateTimestamp: false,
                    reason: $"Point is out of order by {timeDifference}s, exceeding tolerance of {_config.MaxReorderingWindowSeconds}s");
            }
            else
            {
                // Point is within reordering window, accept it but mark as out-of-order
                return new JourneySegmentationResult(
                    shouldAddToCurrentJourney: true,
                    isOutOfOrder: true,
                    isDuplicateTimestamp: false,
                    reason: $"Point is out of order by {timeDifference}s but within tolerance");
            }
        }

        // Check for data gap (device offline for too long)
        if (_config.AutoEndOnDeviceOffline && previousLocation != null)
        {
            var timeGap = (location.Timestamp - previousLocation.Timestamp).TotalMinutes;
            if (timeGap > _config.MaxDataGapMinutes)
            {
                return new JourneySegmentationResult(
                    shouldAddToCurrentJourney: false,
                    isOutOfOrder: false,
                    isDuplicateTimestamp: false,
                    reason: $"Data gap of {timeGap} minutes exceeds maximum of {_config.MaxDataGapMinutes} minutes");
            }
        }

        // Check for stationary period (ignition-off detection)
        if (_config.AutoEndOnIgnitionOff && previousLocation != null)
        {
            // Count consecutive stationary points
            int stationaryCount = 0;
            LocationData? checkPoint = previousLocation;

            // Look backwards through recent points to find consecutive stationary points
            var recentWaypoints = journey.Waypoints
                .TakeLast(5) // Check last 5 points
                .Reverse()
                .ToList();

            foreach (var point in recentWaypoints)
            {
                if (point.Speed <= _config.StationarySpeedThresholdKmh)
                {
                    stationaryCount++;
                }
                else
                {
                    break; // Stop counting when we find a moving point
                }
            }

            // Include the current point if it's also stationary
            if (location.Speed <= _config.StationarySpeedThresholdKmh)
            {
                stationaryCount++;
            }

            // If we have enough consecutive stationary points, end the journey
            if (stationaryCount >= 2) // At least 2 consecutive stationary points (previous + current)
            {
                // Calculate time span of stationary period
                var firstStationaryPoint = recentWaypoints.LastOrDefault(p => p.Speed <= _config.StationarySpeedThresholdKmh) ?? previousLocation;
                if (firstStationaryPoint != null)
                {
                    var stationaryDuration = (location.Timestamp - firstStationaryPoint.Timestamp).TotalMinutes;
                    if (stationaryDuration >= _config.StationaryDurationMinutes)
                    {
                        return new JourneySegmentationResult(
                            shouldAddToCurrentJourney: false,
                            isOutOfOrder: false,
                            isDuplicateTimestamp: false,
                            reason: $"Stationary period of {stationaryDuration} minutes exceeds threshold of {_config.StationaryDurationMinutes} minutes");
                    }
                }
            }
        }

        // Point is acceptable for current journey
        return new JourneySegmentationResult(
            shouldAddToCurrentJourney: true,
            isOutOfOrder: false,
            isDuplicateTimestamp: false,
            reason: "Point accepted");
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
    /// <param name="journeyId">The journey ID.</param>
    /// <returns>The journey if found, otherwise null.</returns>
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
    /// Detects idle periods within a journey based on consecutive locations within a small radius.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <param name="maxDistanceMeters">Maximum distance in meters to consider locations as stationary.</param>
    /// <param name="minDurationSeconds">Minimum duration in seconds to consider as idle.</param>
    /// <returns>A collection of idle periods.</returns>
    public async Task<IEnumerable<IdlePeriod>> DetectIdlePeriodsAsync(string journeyId, double maxDistanceMeters = 25.0, int minDurationSeconds = 300)
    {
        if (string.IsNullOrWhiteSpace(journeyId))
            throw new ArgumentException("Journey ID cannot be empty", nameof(journeyId));

        var journey = await _journeyRepository.GetByIdAsync(journeyId).ConfigureAwait(false);
        if (journey is null)
            throw new InvalidOperationException($"Journey {journeyId} not found");

        if (journey.Waypoints.Count < 2)
            return Enumerable.Empty<IdlePeriod>();

        var idlePeriods = new List<IdlePeriod>();
        var waypoints = journey.Waypoints.OrderBy(w => w.Timestamp).ToList();

        LocationData? startPoint = null;
        DateTime? idleStartTime = null;
        double? initialLatitude = null;
        double? initialLongitude = null;

        for (int i = 0; i < waypoints.Count; i++)
        {
            var current = waypoints[i];

            if (startPoint is null)
            {
                startPoint = current;
                idleStartTime = current.Timestamp;
                initialLatitude = current.Latitude;
                initialLongitude = current.Longitude;
                continue;
            }

            var distanceFromStart = startPoint.DistanceTo(current) * 1000; // Convert km to meters
            var timeElapsed = (current.Timestamp - idleStartTime.Value).TotalSeconds;

            if (distanceFromStart <= maxDistanceMeters && timeElapsed >= minDurationSeconds)
            {
                // Continue accumulating this idle period
                continue;
            }
            else
            {
                // End of current idle period
                if (timeElapsed >= minDurationSeconds)
                {
                    idlePeriods.Add(new IdlePeriod
                    {
                        StartTime = idleStartTime.Value,
                        EndTime = current.Timestamp,
                        Duration = TimeSpan.FromSeconds(timeElapsed),
                        StartLocation = new LocationData
                        {
                            Latitude = initialLatitude!.Value,
                            Longitude = initialLongitude!.Value,
                            Timestamp = idleStartTime.Value
                        },
                        EndLocation = new LocationData
                        {
                            Latitude = current.Latitude,
                            Longitude = current.Longitude,
                            Timestamp = current.Timestamp
                        },
                        MaxDistanceMeters = maxDistanceMeters
                    });
                }

                // Start new potential idle period
                startPoint = current;
                idleStartTime = current.Timestamp;
                initialLatitude = current.Latitude;
                initialLongitude = current.Longitude;
            }
        }

        // Check if journey ended while still in idle state
        if (startPoint is not null && idleStartTime is not null)
        {
            var timeElapsed = (waypoints.Last().Timestamp - idleStartTime.Value).TotalSeconds;
            if (timeElapsed >= minDurationSeconds)
            {
                idlePeriods.Add(new IdlePeriod
                {
                    StartTime = idleStartTime.Value,
                    EndTime = waypoints.Last().Timestamp,
                    Duration = TimeSpan.FromSeconds(timeElapsed),
                    StartLocation = new LocationData
                    {
                        Latitude = initialLatitude!.Value,
                        Longitude = initialLongitude!.Value,
                        Timestamp = idleStartTime.Value
                    },
                    EndLocation = new LocationData
                    {
                        Latitude = waypoints.Last().Latitude,
                        Longitude = waypoints.Last().Longitude,
                        Timestamp = waypoints.Last().Timestamp
                    },
                    MaxDistanceMeters = maxDistanceMeters
                });
            }
        }

        return idlePeriods;
    }

    /// <summary>
    /// Gets idle periods for a journey.
    /// </summary>
    /// <param name="journeyId">The journey ID.</param>
    /// <returns>A collection of idle periods.</returns>
    public async Task<IEnumerable<IdlePeriod>> GetIdlePeriodsAsync(string journeyId)
    {
        return await DetectIdlePeriodsAsync(journeyId).ConfigureAwait(false);
    }

    /// <summary>
    /// Cleans up old journey records.
    /// </summary>
    /// <param name="olderThan">The threshold date.</param>
    /// <returns>The number of journeys deleted.</returns>
    public async Task<int> CleanupOldJourneysAsync(DateTime olderThan)
    {
        if (olderThan >= DateTime.UtcNow)
            throw new ArgumentException("Cleanup date must be in the past");

        return await _journeyRepository.DeleteOlderThanAsync(olderThan).ConfigureAwait(false);
    }
}

/// <summary>
/// Result of applying journey segmentation rules to a location point.
/// </summary>
public record JourneySegmentationResult
{
    /// <summary>
    /// Whether the point should be added to the current journey.
    /// </summary>
    public bool ShouldAddToCurrentJourney { get; }

    /// <summary>
    /// Whether the point is out of chronological order.
    /// </summary>
    public bool IsOutOfOrder { get; }

    /// <summary>
    /// Whether the point has a duplicate timestamp.
    /// </summary>
    public bool IsDuplicateTimestamp { get; }

    /// <summary>
    /// Reason for the segmentation decision.
    /// </summary>
    public string Reason { get; }

    public JourneySegmentationResult(
        bool shouldAddToCurrentJourney,
        bool isOutOfOrder,
        bool isDuplicateTimestamp,
        string reason)
    {
        ShouldAddToCurrentJourney = shouldAddToCurrentJourney;
        IsOutOfOrder = isOutOfOrder;
        IsDuplicateTimestamp = isDuplicateTimestamp;
        Reason = reason;
    }
}