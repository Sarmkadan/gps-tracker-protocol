#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json.Serialization;
using GpsTrackerProtocol.Caching;

/// <summary>
/// Reverse geocoding service for converting coordinates to addresses.
/// Can integrate with Nominatim, Google Maps, or other geocoding providers.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Resolves geographic coordinates to a human-readable location.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <returns>The geocoding result for the given coordinates.</returns>
    Task<GeocodingResult> ReverseGeocodeAsync(double latitude, double longitude);

    /// <summary>
    /// Checks whether the given coordinates resolve to a known location.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <param name="regionCode">Region code to check against.</param>
    /// <returns>True if reverse geocoding succeeds and a country value is present.</returns>
    Task<bool> IsInRegionAsync(double latitude, double longitude, string regionCode);
}

/// <summary>
/// Default <see cref="IGeocodingService"/> implementation backed by the Nominatim reverse geocoding API.
/// Results are cached for 24 hours per coordinate pair, and transient HTTP failures are retried.
/// </summary>
public class GeocodingService : ExternalApiClient, IGeocodingService
{
    private const string NominatimUrl = "https://nominatim.openstreetmap.org/reverse";
    private readonly ICachingService _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeocodingService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client used to call the Nominatim API.</param>
    /// <param name="logger">Logger for retry warnings.</param>
    /// <param name="cache">Cache used to store geocoding results.</param>
    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger, ICachingService cache)
        : base(httpClient, logger)
    {
        _cache = cache;
    }

    /// <summary>
    /// Resolves coordinates to a location via Nominatim. Returns a cached result when one exists
    /// for the same coordinates (rounded to 6 decimal places); otherwise queries the API and caches
    /// the successful result for 24 hours.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <returns>
    /// The geocoding result. Address fields fall back to "Unknown" when the provider returns no value.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when every retry attempt fails with an <see cref="HttpRequestException"/>.
    /// </exception>
    public async Task<GeocodingResult> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var cacheKey = $"geocoding:{latitude.ToString("F6", CultureInfo.InvariantCulture)}:{longitude.ToString("F6", CultureInfo.InvariantCulture)}";

        if (_cache.TryGet(cacheKey, out GeocodingResult cached))
            return cached;

        var result = await ExecuteWithRetryAsync(async () =>
        {
            var parameters = new Dictionary<string, string>
            {
                { "lat", latitude.ToString("F6", CultureInfo.InvariantCulture) },
                { "lon", longitude.ToString("F6", CultureInfo.InvariantCulture) },
                { "format", "json" }
            };

            var url = NominatimUrl + BuildQueryString(parameters);
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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

    /// <summary>
    /// Checks whether the coordinates resolve to a location with a country value.
    /// </summary>
    /// <remarks>
    /// The <paramref name="regionCode"/> argument is not currently used in the check; the result
    /// depends only on whether reverse geocoding succeeds and returns a non-empty country.
    /// </remarks>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <param name="regionCode">Region code to check against (currently unused).</param>
    /// <returns>True if reverse geocoding succeeds and a country value is present; otherwise false.</returns>
    /// <exception cref="InvalidOperationException">
    /// Propagated from <see cref="ReverseGeocodeAsync"/> when all retry attempts fail.
    /// </exception>
    public async Task<bool> IsInRegionAsync(double latitude, double longitude, string regionCode)
    {
        var geocoding = await ReverseGeocodeAsync(latitude, longitude).ConfigureAwait(false);
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
