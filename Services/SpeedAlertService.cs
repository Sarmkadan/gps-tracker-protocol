using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Events;

namespace GpsTrackerProtocol.Services;

/// <summary>
/// Service that monitors device speed and publishes alerts when a configurable
/// speed limit is exceeded. Alerts are emitted with hysteresis – once an alert
/// is sent for a device, no further alerts are published until the speed falls
/// back below the configured limit.
/// </summary>
public interface ISpeedAlertService
{
    /// <summary>
    /// Sets the maximum allowed speed for a specific device.
    /// </summary>
    void SetMaxSpeed(string deviceId, double maxSpeed);

    /// <summary>
    /// Processes a speed measurement for a device. If the speed exceeds the
    /// configured limit and an alert has not already been sent, a
    /// <see cref="SpeedLimitExceededEvent"/> is published.
    /// </summary>
    Task ProcessSpeedAsync(string deviceId, double speed);
}

public class SpeedAlertService : ISpeedAlertService
{
    private readonly IEventPublisher _publisher;
    private readonly ILogger<SpeedAlertService> _logger;

    // Configured max speed per device
    private readonly Dictionary<string, double> _maxSpeeds = new();

    // Tracks devices that have already triggered an alert (hysteresis)
    private readonly HashSet<string> _alertedDevices = new();

    public SpeedAlertService(IEventPublisher publisher, ILogger<SpeedAlertService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public void SetMaxSpeed(string deviceId, double maxSpeed)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("DeviceId cannot be null or whitespace.", nameof(deviceId));

        if (maxSpeed <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSpeed), "Max speed must be positive.");

        _maxSpeeds[deviceId] = maxSpeed;
        _logger.LogDebug("Max speed set for device {DeviceId}: {MaxSpeed}", deviceId, maxSpeed);
    }

    public async Task ProcessSpeedAsync(string deviceId, double speed)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogWarning("ProcessSpeedAsync called with empty deviceId.");
            return;
        }

        if (!_maxSpeeds.TryGetValue(deviceId, out var maxSpeed))
        {
            _logger.LogDebug("No max speed configured for device {DeviceId}. Skipping speed check.", deviceId);
            return;
        }

        if (speed > maxSpeed)
        {
            // Speed exceeds limit – publish alert only if we haven't already alerted
            if (_alertedDevices.Add(deviceId))
            {
                var alertEvent = new SpeedLimitExceededEvent
                {
                    AggregateId = deviceId,
                    DeviceId = deviceId,
                    Speed = speed,
                    MaxSpeed = maxSpeed,
                    Timestamp = DateTime.UtcNow,
                    EventId = Guid.NewGuid().ToString()
                };

                await _publisher.PublishAsync(alertEvent);
                _logger.LogInformation(
                    "Speed limit exceeded for device {DeviceId}: {Speed} > {MaxSpeed}",
                    deviceId, speed, maxSpeed);
            }
        }
        else
        {
            // Speed back under limit – clear hysteresis state
            if (_alertedDevices.Remove(deviceId))
            {
                _logger.LogDebug(
                    "Speed back under limit for device {DeviceId}. Hysteresis state cleared.",
                    deviceId);
            }
        }
    }
}
