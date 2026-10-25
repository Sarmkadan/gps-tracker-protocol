#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using System.Globalization;
using System.Text.Json.Serialization;

using GpsTrackerProtocol.Domain;
using Microsoft.Extensions.Logging;

/// <summary>
/// Weather API integration for getting weather conditions at GPS locations.
/// Uses Open-Meteo API (free, no authentication required).
/// </summary>
public interface IWeatherApiClient
{
    /// <summary>
    /// Gets the current weather conditions at the specified coordinates.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <returns>The current weather data for the location.</returns>
    /// <exception cref="GpsTrackerException">Thrown when the weather data cannot be retrieved.</exception>
    Task<WeatherData> GetWeatherAsync(double latitude, double longitude);
}

/// <summary>
/// Default <see cref="IWeatherApiClient"/> implementation that queries the Open-Meteo forecast API.
/// </summary>
public class WeatherApiClient : ExternalApiClient, IWeatherApiClient
{
    private const string OPEN_METEO_URL = "https://api.open-meteo.com/v1/forecast";
    private const int REQUEST_TIMEOUT_SECONDS = 10;
    private const string COORDINATE_FORMAT = "F2";

    private const string PARAM_LATITUDE = "latitude";
    private const string PARAM_LONGITUDE = "longitude";
    private const string PARAM_CURRENT = "current";
    private const string PARAM_TEMPERATURE_UNIT = "temperature_unit";
    private const string CURRENT_FIELDS = "temperature,weather_code,wind_speed";
    private const string TEMPERATURE_UNIT_CELSIUS = "celsius";

    private const string INVALID_RESPONSE_MESSAGE = "Invalid weather response";
    private const string FETCH_FAILED_MESSAGE = "Failed to fetch weather data";

    // WMO weather interpretation codes reported in the "weather_code" field.
    private const int WEATHER_CODE_CLEAR_SKY = 0;
    private const int WEATHER_CODE_MAINLY_CLEAR = 1;
    private const int WEATHER_CODE_PARTLY_CLOUDY = 2;
    private const int WEATHER_CODE_OVERCAST = 3;
    private const int WEATHER_CODE_FOG = 45;
    private const int WEATHER_CODE_LIGHT_DRIZZLE = 51;
    private const int WEATHER_CODE_SLIGHT_RAIN = 61;
    private const int WEATHER_CODE_SLIGHT_SNOW = 71;
    private const int WEATHER_CODE_SLIGHT_RAIN_SHOWERS = 80;
    private const int WEATHER_CODE_SLIGHT_SNOW_SHOWERS = 85;

    private const string DESCRIPTION_CLEAR_SKY = "Clear sky";
    private const string DESCRIPTION_MAINLY_CLEAR = "Mainly clear";
    private const string DESCRIPTION_PARTLY_CLOUDY = "Partly cloudy";
    private const string DESCRIPTION_OVERCAST = "Overcast";
    private const string DESCRIPTION_FOGGY = "Foggy";
    private const string DESCRIPTION_LIGHT_DRIZZLE = "Light drizzle";
    private const string DESCRIPTION_SLIGHT_RAIN = "Slight rain";
    private const string DESCRIPTION_SLIGHT_SNOW = "Slight snow";
    private const string DESCRIPTION_SLIGHT_RAIN_SHOWERS = "Slight rain showers";
    private const string DESCRIPTION_SLIGHT_SNOW_SHOWERS = "Slight snow showers";
    private const string DESCRIPTION_UNKNOWN = "Unknown";

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to call the Open-Meteo API.</param>
    /// <param name="logger">The logger used to record fetch failures.</param>
    public WeatherApiClient(HttpClient httpClient, ILogger<WeatherApiClient> logger)
        : base(httpClient, logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);
    }

    /// <summary>
    /// Fetches the current temperature, wind speed and weather code at the specified coordinates.
    /// The request is cancelled after 10 seconds.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees, formatted with two decimal places.</param>
    /// <param name="longitude">Longitude in decimal degrees, formatted with two decimal places.</param>
    /// <returns>
    /// A <see cref="WeatherData"/> instance with the current conditions and a UTC timestamp.
    /// </returns>
    /// <exception cref="GpsTrackerException">
    /// Thrown when the HTTP request fails, times out, returns a non-success status code,
    /// or the response does not contain current weather data. The original exception is preserved as the inner exception.
    /// </exception>
    public async Task<WeatherData> GetWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var parameters = new Dictionary<string, string>
            {
                { PARAM_LATITUDE, latitude.ToString(COORDINATE_FORMAT, CultureInfo.InvariantCulture) },
                { PARAM_LONGITUDE, longitude.ToString(COORDINATE_FORMAT, CultureInfo.InvariantCulture) },
                { PARAM_CURRENT, CURRENT_FIELDS },
                { PARAM_TEMPERATURE_UNIT, TEMPERATURE_UNIT_CELSIUS }
            };

            var url = OPEN_METEO_URL + BuildQueryString(parameters);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(REQUEST_TIMEOUT_SECONDS));
            using var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var weatherResponse = System.Text.Json.JsonSerializer.Deserialize<WeatherResponse>(json);

            if (weatherResponse?.Current is null)
                throw new InvalidOperationException(INVALID_RESPONSE_MESSAGE);

            return new WeatherData
            {
                Latitude = latitude,
                Longitude = longitude,
                Temperature = weatherResponse.Current.Temperature,
                WindSpeed = weatherResponse.Current.WindSpeed,
                WeatherCode = weatherResponse.Current.WeatherCode,
                Description = GetWeatherDescription(weatherResponse.Current.WeatherCode),
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, FETCH_FAILED_MESSAGE);
            throw new GpsTrackerException($"{FETCH_FAILED_MESSAGE} for ({latitude}, {longitude})", ex);
        }
    }

    private string GetWeatherDescription(int weatherCode)
    {
        return weatherCode switch
        {
            WEATHER_CODE_CLEAR_SKY => DESCRIPTION_CLEAR_SKY,
            WEATHER_CODE_MAINLY_CLEAR => DESCRIPTION_MAINLY_CLEAR,
            WEATHER_CODE_PARTLY_CLOUDY => DESCRIPTION_PARTLY_CLOUDY,
            WEATHER_CODE_OVERCAST => DESCRIPTION_OVERCAST,
            WEATHER_CODE_FOG => DESCRIPTION_FOGGY,
            WEATHER_CODE_LIGHT_DRIZZLE => DESCRIPTION_LIGHT_DRIZZLE,
            WEATHER_CODE_SLIGHT_RAIN => DESCRIPTION_SLIGHT_RAIN,
            WEATHER_CODE_SLIGHT_SNOW => DESCRIPTION_SLIGHT_SNOW,
            WEATHER_CODE_SLIGHT_RAIN_SHOWERS => DESCRIPTION_SLIGHT_RAIN_SHOWERS,
            WEATHER_CODE_SLIGHT_SNOW_SHOWERS => DESCRIPTION_SLIGHT_SNOW_SHOWERS,
            _ => DESCRIPTION_UNKNOWN
        };
    }
}

public class WeatherData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int WeatherCode { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
}

internal class WeatherResponse
{
    [JsonPropertyName("current")]
    public WeatherCurrent Current { get; set; }
}

internal class WeatherCurrent
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("wind_speed")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}
