# IWeatherApiClient

The `IWeatherApiClient` interface defines a contract for interacting with a weather data API, enabling retrieval of current weather conditions at specified geographic coordinates. It is designed for integration with GPS tracking systems where location-specific weather data is required for reporting or analysis.

## API

### `WeatherApiClient`
The concrete implementation of `IWeatherApiClient` that provides access to weather data through the configured API endpoint. This class is responsible for establishing network connections, handling authentication, and parsing API responses.

### `public async Task<WeatherData> GetWeatherAsync()`
Asynchronously retrieves the latest weather data for the configured geographic coordinates.

- **Parameters**: None.
- **Return value**: A `Task<WeatherData>` that resolves to a `WeatherData` object containing current weather conditions.
- **Exceptions**: Throws `HttpRequestException` if the network request fails, `JsonException` if the response cannot be deserialized, or `InvalidOperationException` if the API key or endpoint is misconfigured.

### `public double Latitude`
Gets the geographic latitude coordinate (in degrees) for which weather data is retrieved. This value is used as the query parameter in the API request.

- **Type**: `double`
- **Range**: Typically between -90.0 and 90.0.
- **Exceptions**: None (read-only property).

### `public double Longitude`
Gets the geographic longitude coordinate (in degrees) for which weather data is retrieved. This value is used as the query parameter in the API request.

- **Type**: `double`
- **Range**: Typically between -180.0 and 180.0.
- **Exceptions**: None (read-only property).

### `public double Temperature`
Gets the current temperature in degrees Celsius.

- **Type**: `double`
- **Unit**: Celsius (°C)
- **Exceptions**: None (read-only property). May be `double.NaN` if data is unavailable.

### `public double WindSpeed`
Gets the current wind speed in meters per second.

- **Type**: `double`
- **Unit**: Meters per second (m/s)
- **Exceptions**: None (read-only property). May be `double.NaN` if data is unavailable.

### `public int WeatherCode`
Gets the current weather condition code as defined by the API provider (e.g., OpenWeatherMap or similar).

- **Type**: `int`
- **Range**: API-specific; commonly 200–800 for standard conditions.
- **Exceptions**: None (read-only property). May be `-1` if data is unavailable.

### `public string Description`
Gets a human-readable description of the current weather conditions (e.g., "light rain", "clear sky").

- **Type**: `string`
- **Exceptions**: None (read-only property). May be `null` if data is unavailable.

### `public DateTime Timestamp`
Gets the UTC timestamp when the weather data was recorded or retrieved.

- **Type**: `DateTime` (UTC)
- **Exceptions**: None (read-only property).

### `public WeatherCurrent Current`
Gets the current weather data object, which may contain additional fields beyond the basic properties exposed here.

- **Type**: `WeatherCurrent`
- **Exceptions**: None (read-only property). May be `null` if data is unavailable.

## Usage

### Example 1: Fetch and Display Weather Data
