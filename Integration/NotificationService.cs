#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GpsTrackerProtocol.Integration;

/// <summary>
/// Notification service for alerting about speed violations, geofence breaches, etc.
/// Can be extended to support email, SMS, and push notifications.
/// </summary>
public interface INotificationService
{
    Task SendSpeedingAlertAsync(string deviceId, double speed, double speedLimit);
    Task SendGeofenceAlertAsync(string deviceId, double latitude, double longitude);
    Task SendOfflineAlertAsync(string deviceId);
}

/// <summary>
/// In-memory implementation of <see cref="INotificationService"/>. Stores every alert as a
/// <see cref="Notification"/> and writes it to the log at a severity matching the alert type.
/// </summary>
public class NotificationService : INotificationService
{
    private const string DeviceIdErrorMessage = "Device ID cannot be null or empty";
    private const string SpeedingAlertMessageFormat = "Device {0} exceeded speed limit: {1:F1}km/h (limit: {2:F1}km/h)";
    private const string GeofenceAlertMessageFormat = "Device {0} breached geofence at {1:F6}, {2:F6}";
    private const string OfflineAlertMessageFormat = "Device {0} has gone offline";
    private const string SpeedingAlertLogMessage = "Speeding alert: {Message}";
    private const string GeofenceAlertLogMessage = "Geofence alert: {Message}";
    private const string OfflineAlertLogMessage = "Offline alert: {Message}";

    private readonly ILogger<NotificationService> _logger;
    private readonly List<Notification> _notifications = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record each alert.</param>
    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Records a speeding violation for the device and logs it as a warning.
    /// </summary>
    /// <param name="deviceId">Identifier of the device that exceeded the limit.</param>
    /// <param name="speed">Measured speed in km/h.</param>
    /// <param name="speedLimit">Applicable speed limit in km/h.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="deviceId"/> is null, empty, or whitespace.
    /// </exception>
    public Task SendSpeedingAlertAsync(string deviceId, double speed, double speedLimit)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException(DeviceIdErrorMessage, nameof(deviceId));
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Type = NotificationType.SpeedingViolation,
            DeviceId = deviceId,
            Message = string.Format(SpeedingAlertMessageFormat, deviceId, speed, speedLimit),
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _notifications.Add(notification);
        _logger.LogWarning(SpeedingAlertLogMessage, notification.Message);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a geofence breach for the device and logs it as a warning.
    /// </summary>
    /// <param name="deviceId">Identifier of the device that breached the geofence.</param>
    /// <param name="latitude">Latitude of the device at the time of the breach.</param>
    /// <param name="longitude">Longitude of the device at the time of the breach.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="deviceId"/> is null, empty, or whitespace.
    /// </exception>
    public Task SendGeofenceAlertAsync(string deviceId, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException(DeviceIdErrorMessage, nameof(deviceId));
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Type = NotificationType.GeofenceBreach,
            DeviceId = deviceId,
            Message = string.Format(GeofenceAlertMessageFormat, deviceId, latitude, longitude),
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _notifications.Add(notification);
        _logger.LogWarning(GeofenceAlertLogMessage, notification.Message);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Records that the device has gone offline and logs it as a critical event.
    /// </summary>
    /// <param name="deviceId">Identifier of the device that went offline.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="deviceId"/> is null, empty, or whitespace.
    /// </exception>
    public Task SendOfflineAlertAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException(DeviceIdErrorMessage, nameof(deviceId));
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Type = NotificationType.DeviceOffline,
            DeviceId = deviceId,
            Message = string.Format(OfflineAlertMessageFormat, deviceId),
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _notifications.Add(notification);
        _logger.LogCritical(OfflineAlertLogMessage, notification.Message);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the stored notifications, optionally filtered by device.
    /// </summary>
    /// <param name="deviceId">
    /// Device to filter by. When null, notifications for all devices are returned.
    /// </param>
    /// <returns>The matching notifications, in the order they were recorded.</returns>
    public IEnumerable<Notification> GetNotifications(string deviceId = null)
    {
        return deviceId is null
            ? _notifications
            : _notifications.Where(n => n.DeviceId == deviceId);
    }

    /// <summary>
    /// Marks the notification with the given identifier as read.
    /// </summary>
    /// <param name="notificationId">Identifier of the notification to mark.</param>
    /// <remarks>
    /// Does nothing when <paramref name="notificationId"/> is null, empty, whitespace,
    /// or does not match any stored notification.
    /// </remarks>
    public void MarkAsRead(string notificationId)
    {
        if (string.IsNullOrWhiteSpace(notificationId))
            return;

        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification is not null)
            notification.IsRead = true;
    }
}

public class Notification
{
    public string Id { get; set; }
    public string DeviceId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}

public enum NotificationType
{
    SpeedingViolation,
    GeofenceBreach,
    DeviceOffline,
    LowBattery,
    HighTemperature,
    CommunicationError
}
