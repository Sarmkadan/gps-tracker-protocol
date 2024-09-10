#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using GpsTrackerProtocol.Caching;

/// <summary>
/// Reverse geocoding service for converting coordinates to addresses.
/// Can integrate with Nominatim, Google Maps, or other geocoding providers.
/// </summary>
public interface IGeocodingService
{
    Task<GeocodingResult> ReverseGeocodeAsync(double latitude, double longitude);
    Task<bool> IsInRegionAsync(double latitude, double longitude, string regionCode);
}

public class GeocodingService : ExternalApiClient, IGeocodingService
{
    private const string NominatimUrl = "https://nominatim.openstreetmap.org/reverse";
    private readonly ICachingService _cache;

    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger, ICachingService cache)
        : base(httpClient, logger)
    {
        _cache = cache;
    }

    public async Task<GeocodingResult> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var cacheKey = $"geocoding:{latitude:F6}:{longitude:F6}";

        if (_cache.TryGet(cacheKey, out GeocodingResult cached))
            return cached;

        var result = await ExecuteWithRetryAsync(async () =>
        {
            var parameters = new Dictionary<string, string>
            {
                { "lat", latitude.ToString("F6") },
                { "lon", longitude.ToString("F6") },
                { "format", "json" }
            };

            var url = NominatimUrl + BuildQueryString(parameters);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var nominatimResult = System.Text.Json.JsonSerializer.Deserialize<NominatimResponse>(json);

            return new GeocodingResult
            {
                Latitude = latitude,
                Longitude = longitude,
                Address = nominatimResult?.Address ?? "Unknown",
                City = nominatimResult?.Address ?? "Unknown",
                Country = nominatimResult?.Address ?? "Unknown",
                DisplayName = nominatimResult?.DisplayName ?? "Unknown Location",
                Success = true
            };
        });

        _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
        return result;
    }

    public async Task<bool> IsInRegionAsync(double latitude, double longitude, string regionCode)
    {
        var geocoding = await ReverseGeocodeAsync(latitude, longitude);
        return geocoding.Success && !string.IsNullOrEmpty(geocoding.Country);
    }
}

public class GeocodingResult
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string DisplayName { get; set; }
    public bool Success { get; set; }
}

internal class NominatimResponse
{
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    [JsonPropertyName("lon")]
    public double Longitude { get; set; }
}
