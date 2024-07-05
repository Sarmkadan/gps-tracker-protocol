#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;

/// <summary>
/// Webhook delivery service for location updates and events.
/// Sends HTTP POST to registered webhooks with automatic retry.
/// </summary>
public interface IWebhookClient
{
    Task SendLocationUpdateAsync(string webhookUrl, LocationData location);
    Task SendJourneyCompletedAsync(string webhookUrl, Journey journey);
    Task SendDeviceStatusAsync(string webhookUrl, Device device);

    /// <summary>
    /// POSTs a <see cref="GeofenceWebhookPayload"/> to the specified URL.
    /// Retries up to three times on transient HTTP failures.
    /// </summary>
    Task SendGeofenceEventAsync(string webhookUrl, GeofenceWebhookPayload payload);
}

public class WebhookClient : ExternalApiClient, IWebhookClient
{
    public WebhookClient(HttpClient httpClient, ILogger<WebhookClient> logger)
        : base(httpClient, logger)
    {
    }

    public async Task SendLocationUpdateAsync(string webhookUrl, LocationData location)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        var payload = new WebhookPayload
        {
            EventType = "location_update",
            Timestamp = DateTime.UtcNow,
            Data = new
            {
                location.DeviceId,
                location.Latitude,
                location.Longitude,
                location.Speed,
                location.Bearing,
                location.Altitude,
                Timestamp = location.Timestamp.ToString("o")
            }
        };

        await ExecuteWithRetryAsync(async () =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Webhook sent successfully to {Url}", webhookUrl);
            return true;
        });
    }

    public async Task SendJourneyCompletedAsync(string webhookUrl, Journey journey)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        var payload = new WebhookPayload
        {
            EventType = "journey_completed",
            Timestamp = DateTime.UtcNow,
            Data = new
            {
                journey.Id,
                journey.DeviceId,
                StartTime = journey.StartTime.ToString("o"),
                EndTime = journey.EndTime?.ToString("o"),
                TotalDistance = journey.GetTotalDistance(),
                Duration = journey.GetDuration().TotalMinutes,
                WaypointCount = journey.Waypoints.Count
            }
        };

        await ExecuteWithRetryAsync(async () =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Journey webhook sent to {Url}", webhookUrl);
            return true;
        });
    }

    public async Task SendDeviceStatusAsync(string webhookUrl, Device device)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        var payload = new WebhookPayload
        {
            EventType = "device_status",
            Timestamp = DateTime.UtcNow,
            Data = new
            {
                device.Id,
                device.Imei,
                device.DeviceName,
                device.IsActive,
                device.Status,
                LastSeen = device.LastSeen.ToString("o")
            }
        };

        await ExecuteWithRetryAsync(async () =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Device status webhook sent to {Url}", webhookUrl);
            return true;
        });
    }

    public async Task SendGeofenceEventAsync(string webhookUrl, GeofenceWebhookPayload payload)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        var envelope = new WebhookPayload
        {
            EventType = payload.EventType,
            Timestamp = DateTime.UtcNow,
            Data = new
            {
                payload.DeviceId,
                payload.GeofenceId,
                payload.Latitude,
                payload.Longitude,
                payload.Speed,
                payload.DwellSeconds,
                payload.Timestamp
            }
        };

        await ExecuteWithRetryAsync(async () =>
        {
            var content = new StringContent(
                JsonSerializer.Serialize(envelope),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Geofence webhook sent to {Url}: {EventType}",
                webhookUrl, payload.EventType);
            return true;
        });
    }
}

public class WebhookPayload
{
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public object Data { get; set; }
}

public class WebhookSubscription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; }
    public string WebhookUrl { get; set; }
    public string EventType { get; set; } // "location_update", "journey_completed", "device_status"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
