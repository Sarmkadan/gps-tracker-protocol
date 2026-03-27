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
    private readonly ILogger<NotificationService> _logger;
    private readonly List<Notification> _notifications = new();

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendSpeedingAlertAsync(string deviceId, double speed, double speedLimit)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Type = NotificationType.SpeedingViolation,
            DeviceId = deviceId,
            Message = $"Device {deviceId} exceeded speed limit: {speed:F1}km/h (limit: {speedLimit:F1}km/h)",
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _notifications.Add(notification);
        _logger.LogWarning("Speeding alert: {Message}", notification.Message);

        return Task.CompletedTask;
    }

    public Task SendGeofenceAlertAsync(string deviceId, double latitude, double longitude)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Type = NotificationType.GeofenceBreach,
            DeviceId = deviceId,
            Message = $"Device {deviceId} breached geofence at {latitude:F6}, {longitude:F6}",
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _notifications.Add(notification);
        _logger.LogWarning("Geofence alert: {Message}", notification.Message);

        return Task.CompletedTask;
    }

    public Task SendOfflineAlertAsync(string deviceId)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            Type = NotificationType.DeviceOffline,
            DeviceId = deviceId,
            Message = $"Device {deviceId} has gone offline",
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _notifications.Add(notification);
        _logger.LogCritical("Offline alert: {Message}", notification.Message);

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
