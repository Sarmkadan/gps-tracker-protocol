#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using GpsTrackerProtocol.Domain;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json.Serialization;

/// <summary>
/// Weather API integration for getting weather conditions at GPS locations.
/// Uses Open-Meteo API (free, no authentication required).
/// </summary>
public interface IWeatherApiClient
{
    Task<WeatherData> GetWeatherAsync(double latitude, double longitude);
}

public class WeatherApiClient : ExternalApiClient, IWeatherApiClient
{
    private const string OpenMeteoUrl = "https://api.open-meteo.com/v1/forecast";

    public WeatherApiClient(HttpClient httpClient, ILogger<WeatherApiClient> logger)
        : base(httpClient, logger)
    {
    }

    public async Task<WeatherData> GetWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "latitude", latitude.ToString("F2", CultureInfo.InvariantCulture) },
                { "longitude", longitude.ToString("F2", CultureInfo.InvariantCulture) },
                { "current", "temperature,weather_code,wind_speed" },
                { "temperature_unit", "celsius" }
            };

            var url = OpenMeteoUrl + BuildQueryString(parameters);
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var weatherResponse = System.Text.Json.JsonSerializer.Deserialize<WeatherResponse>(json);

            if (weatherResponse?.Current is null)
                throw new InvalidOperationException("Invalid weather response");

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
            _logger?.LogError(ex, "Failed to fetch weather data");
            throw new GpsTrackerException($"Failed to fetch weather data for ({latitude}, {longitude})", ex);
        }
    }

    private string GetWeatherDescription(int weatherCode)
    {
        return weatherCode switch
        {
            0 => "Clear sky",
            1 => "Mainly clear",
            2 => "Partly cloudy",
            3 => "Overcast",
            45 => "Foggy",
            51 => "Light drizzle",
            61 => "Slight rain",
            71 => "Slight snow",
            80 => "Slight rain showers",
            85 => "Slight snow showers",
            _ => "Unknown"
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
