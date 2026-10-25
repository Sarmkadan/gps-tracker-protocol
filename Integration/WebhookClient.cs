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
    /// <summary>
    /// POSTs a location update payload to the specified URL.
    /// Retries up to three times on transient HTTP failures.
    /// </summary>
    /// <param name="webhookUrl">Absolute HTTP or HTTPS URL to deliver to.</param>
    /// <param name="location">Location data to send.</param>
    Task SendLocationUpdateAsync(string webhookUrl, LocationData location);

    /// <summary>
    /// POSTs a journey completion payload to the specified URL.
    /// Retries up to three times on transient HTTP failures.
    /// </summary>
    /// <param name="webhookUrl">Absolute HTTP or HTTPS URL to deliver to.</param>
    /// <param name="journey">Completed journey to summarize.</param>
    Task SendJourneyCompletedAsync(string webhookUrl, Journey journey);

    /// <summary>
    /// POSTs a device status payload to the specified URL.
    /// Retries up to three times on transient HTTP failures.
    /// </summary>
    /// <param name="webhookUrl">Absolute HTTP or HTTPS URL to deliver to.</param>
    /// <param name="device">Device whose current status is sent.</param>
    Task SendDeviceStatusAsync(string webhookUrl, Device device);

    /// <summary>
    /// POSTs a <see cref="GeofenceWebhookPayload"/> to the specified URL.
    /// Retries up to three times on transient HTTP failures.
    /// </summary>
    Task SendGeofenceEventAsync(string webhookUrl, GeofenceWebhookPayload payload);
}

/// <summary>
/// Default <see cref="IWebhookClient"/> implementation that delivers webhook payloads
/// over HTTP using the shared <see cref="ExternalApiClient"/> retry logic.
/// </summary>
public class WebhookClient : ExternalApiClient, IWebhookClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookClient"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client used to send webhook requests.</param>
    /// <param name="logger">Logger for delivery results and warnings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> or <paramref name="logger"/> is null.</exception>
    public WebhookClient(HttpClient httpClient, ILogger<WebhookClient> logger)
        : base(httpClient, logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
    }

    /// <summary>
    /// Sends a <c>location_update</c> webhook containing the device ID, coordinates,
    /// speed, bearing, altitude and timestamp of the given location.
    /// Returns without sending if <paramref name="webhookUrl"/> is empty or not a valid
    /// absolute HTTP or HTTPS URI.
    /// </summary>
    /// <param name="webhookUrl">Absolute HTTP or HTTPS URL to deliver to.</param>
    /// <param name="location">Location data to send.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="location"/> is null.</exception>
    public async Task SendLocationUpdateAsync(string webhookUrl, LocationData location)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        ArgumentNullException.ThrowIfNull(location);

        if (!TryGetWebhookUri(webhookUrl, out var webhookUri))
        {
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

            var response = await _httpClient.PostAsync(webhookUri, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Webhook sent successfully to {Url}", webhookUrl);
            return true;
        });
    }

    /// <summary>
    /// Sends a <c>journey_completed</c> webhook summarizing the journey: ID, device, start
    /// and end times, total distance, duration in minutes and waypoint count.
    /// Returns without sending if <paramref name="webhookUrl"/> is empty or not a valid
    /// absolute HTTP or HTTPS URI.
    /// </summary>
    /// <param name="webhookUrl">Absolute HTTP or HTTPS URL to deliver to.</param>
    /// <param name="journey">Completed journey to summarize.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="journey"/> is null.</exception>
    public async Task SendJourneyCompletedAsync(string webhookUrl, Journey journey)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        ArgumentNullException.ThrowIfNull(journey);

        if (!TryGetWebhookUri(webhookUrl, out var webhookUri))
        {
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

            var response = await _httpClient.PostAsync(webhookUri, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Journey webhook sent to {Url}", webhookUrl);
            return true;
        });
    }

    /// <summary>
    /// Sends a <c>device_status</c> webhook with the device ID, IMEI, name, active flag,
    /// status and last-seen time.
    /// Returns without sending if <paramref name="webhookUrl"/> is empty or not a valid
    /// absolute HTTP or HTTPS URI.
    /// </summary>
    /// <param name="webhookUrl">Absolute HTTP or HTTPS URL to deliver to.</param>
    /// <param name="device">Device whose current status is sent.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> is null.</exception>
    public async Task SendDeviceStatusAsync(string webhookUrl, Device device)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        ArgumentNullException.ThrowIfNull(device);

        if (!TryGetWebhookUri(webhookUrl, out var webhookUri))
        {
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

            var response = await _httpClient.PostAsync(webhookUri, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Device status webhook sent to {Url}", webhookUrl);
            return true;
        });
    }

    /// <summary>
    /// POSTs a <see cref="GeofenceWebhookPayload"/> to the specified URL.
    /// Retries up to three times on transient HTTP failures.
    /// </summary>
    /// <param name="webhookUrl">Absolute HTTP or HTTPS URL to deliver to.</param>
    /// <param name="payload">Geofence event to send.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
    public async Task SendGeofenceEventAsync(string webhookUrl, GeofenceWebhookPayload payload)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook URL is empty");
            return;
        }

        ArgumentNullException.ThrowIfNull(payload);

        if (!TryGetWebhookUri(webhookUrl, out var webhookUri))
        {
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

            var response = await _httpClient.PostAsync(webhookUri, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Geofence webhook sent to {Url}: {EventType}",
                webhookUrl, payload.EventType);
            return true;
        });
    }

    private bool TryGetWebhookUri(string webhookUrl, out Uri webhookUri)
    {
        if (Uri.TryCreate(webhookUrl, UriKind.Absolute, out var parsedUri) &&
            (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
        {
            webhookUri = parsedUri;
            return true;
        }

        _logger.LogWarning("Webhook URL is not a valid absolute HTTP or HTTPS URI: {Url}", webhookUrl);
        webhookUri = null!;
        return false;
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
