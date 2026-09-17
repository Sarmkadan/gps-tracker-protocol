#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using Microsoft.Extensions.Logging;

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

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

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

    public IEnumerable<Notification> GetNotifications(string deviceId = null)
    {
        return deviceId is null
            ? _notifications
            : _notifications.Where(n => n.DeviceId == deviceId);
    }

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
