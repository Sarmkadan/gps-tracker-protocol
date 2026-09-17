# WeatherApiClient

## Responsibilities

The `WeatherApiClient` class is responsible for fetching weather data for GPS coordinates using the Open-Meteo API. It provides:

- Integration with the free Open-Meteo weather service (no authentication required)
- Retrieval of current weather conditions including temperature, wind speed, and weather codes
- Automatic retry mechanism for failed requests (up to 3 attempts)
- Proper error handling and logging
- Conversion of weather codes to human-readable descriptions
- Cancellation token support for timeout handling

## Public Methods

### `GetWeatherAsync(double latitude, double longitude)`

Fetches current weather data for the specified GPS coordinates.

**Parameters:**
- `latitude`: Latitude coordinate (-90 to 90)
- `longitude`: Longitude coordinate (-180 to 180)

**Returns:**
- `Task<WeatherData>`: Weather data containing temperature, wind speed, weather code, description, and timestamp

**Exceptions:**
- `GpsTrackerException`: Thrown when weather data cannot be fetched after all retry attempts
- `InvalidOperationException`: Thrown when the weather response is invalid or missing current data
- `HttpRequestException`: Thrown when HTTP requests fail (handled internally with retries)

**Usage Notes:**
- Uses Open-Meteo API endpoint: `https://api.open-meteo.com/v1/forecast`
- Requests current temperature (in Celsius), weather code, and wind speed
- Implements 10-second timeout for each request
- Automatically retries failed requests up to 3 times with 1-second delays
- Weather codes are converted to descriptive strings using a predefined mapping

## Example Usage

```csharp
// Using dependency injection (recommended)
public class WeatherService
{
    private readonly IWeatherApiClient _weatherClient;
    
    public WeatherService(IWeatherApiClient weatherClient)
    {
        _weatherClient = weatherClient;
    }
    
    public async Task DisplayCurrentWeather(double latitude, double longitude)
    {
        try
        {
            WeatherData weather = await _weatherClient.GetWeatherAsync(latitude, longitude);
            
            Console.WriteLine($"Weather at ({weather.Latitude:F2}, {weather.Longitude:F2}):");
            Console.WriteLine($"Temperature: {weather.Temperature}°C");
            Console.WriteLine($"Wind Speed: {weather.WindSpeed} km/h");
            Console.WriteLine($"Conditions: {weather.Description}");
            Console.WriteLine($"Updated: {weather.Timestamp:u}");
        }
        catch (GpsTrackerException ex)
        {
            Console.WriteLine($"Unable to fetch weather data: {ex.Message}");
            // Handle error appropriately (fallback, cached data, etc.)
        }
    }
}

// Manual instantiation (less common)
var httpClient = new HttpClient();
var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Information));
var logger = loggerFactory.CreateLogger<WeatherApiClient>();

var weatherClient = new WeatherApiClient(httpClient, logger);
WeatherData weather = await weatherClient.GetWeatherAsync(40.7128, -74.0060); // New York City
```

## Implementation Details

### Inheritance
- Inherits from `ExternalApiClient` base class which provides:
  - HTTP client management
  - Logging capabilities
  - Retry logic with exponential backoff
  - Query string building utilities

### API Integration
- Uses Open-Meteo's free forecast API
- Request parameters:
  - `latitude`: Formatted to 2 decimal places
  - `longitude`: Formatted to 2 decimal places
  - `current`: Requests temperature, weather_code, and wind_speed
  - `temperature_unit`: Set to "celsius"

### Response Processing
- Deserializes JSON response into internal `WeatherResponse` and `WeatherCurrent` classes
- Maps Open-Meteo weather codes to descriptive strings:
  - 0: Clear sky
  - 1: Mainly clear
  - 2: Partly cloudy
  - 3: Overcast
  - 45: Foggy
  - 51: Light drizzle
  - 61: Slight rain
  - 71: Slight snow
  - 80: Slight rain showers
  - 85: Slight snow showers
  - Other: Unknown

### Error Handling
- Network failures are retried up to 3 times with 1-second delays
- Invalid responses throw `InvalidOperationException`
- All exceptions are logged before rethrowing as `GpsTrackerException`
- Timeout cancellation is implemented using `CancellationTokenSource`

## Thread Safety
The class is thread-safe when used with dependency injection as it contains no mutable state beyond its readonly dependencies (`HttpClient` and `ILogger`).

## Configuration
The client requires:
- An `HttpClient` instance (typically configured via dependency injection)
- An `ILogger<WeatherApiClient>` instance for logging

No additional configuration is needed as the Open-Meteo API is free and doesn't require authentication.