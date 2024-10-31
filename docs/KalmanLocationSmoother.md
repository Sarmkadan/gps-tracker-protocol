# KalmanLocationSmoother

A Kalman filter implementation for smoothing GPS location data, estimating position and variance while filtering out implausible jumps in speed or accuracy. The filter maintains state for latitude, longitude, and their variances, and provides methods to reset the filter state or all parameters.

## API

### `public double ProcessNoiseMetersPerSecond`
The process noise parameter in meters per second, representing the expected uncertainty in the movement model. Higher values allow the filter to adapt more quickly to changes but may reduce smoothing. Default value is typically set to a low value (e.g., 1.0) to favor stability.

### `public double MaxPlausibleSpeedKmh`
The maximum plausible speed in kilometers per hour. Any input speed exceeding this value will be clamped to this limit before filtering. This prevents unrealistic jumps in location due to GPS errors. Default is typically set to a high value (e.g., 1000.0 km/h).

### `public double DefaultAccuracyMeters`
The default accuracy in meters used when no accuracy is provided in input location data. This value is used to initialize the measurement noise covariance. Default is typically set to a moderate value (e.g., 10.0 meters).

### `public LocationData? Smooth`
The most recently smoothed location data. This property is `null` if no data has been processed or after a reset. Accessing this property returns the latest estimated position and its variance.

### `public void Reset()`
Resets the filter state to the initial conditions using the current `DefaultAccuracyMeters` and the last known position (if available). Does not reset tunable parameters like `ProcessNoiseMetersPerSecond` or `MaxPlausibleSpeedKmh`. After reset, `Smooth` will be `null` until new data is processed.

### `public void ResetAll()`
Resets all internal state and tunable parameters to their default values. This includes resetting the filter state, measurement noise, and all configurable parameters (`ProcessNoiseMetersPerSecond`, `MaxPlausibleSpeedKmh`, `DefaultAccuracyMeters`). After reset, `Smooth` will be `null` until new data is processed.

### `public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)`
Computes the great-circle distance in meters between two geographic coordinates using the Haversine formula.

- **Parameters**:
  - `lat1`, `lon1`: Latitude and longitude of the first point in degrees.
  - `lat2`, `lon2`: Latitude and longitude of the second point in degrees.
- **Returns**: The distance in meters between the two points.
- **Throws**: `ArgumentException` if the input coordinates are invalid (e.g., latitude outside [-90, 90]).

### `public double Latitude`
The estimated latitude of the last smoothed location in degrees. Returns `double.NaN` if no data has been processed or after a reset.

### `public double Longitude`
The estimated longitude of the last smoothed location in degrees. Returns `double.NaN` if no data has been processed or after a reset.

### `public double LatVariance`
The estimated variance of the latitude estimate in square meters. Represents the uncertainty in the latitude component of the smoothed position. Returns `double.NaN` if no data has been processed or after a reset.

### `public double LonVariance`
The estimated variance of the longitude estimate in square meters. Represents the uncertainty in the longitude component of the smoothed position. Returns `double.NaN` if no data has been processed or after a reset.

### `public DateTime LastTimestamp`
The timestamp of the last processed location data. This is updated whenever new data is smoothed. Returns `DateTime.MinValue` if no data has been processed or after a reset.

## Usage

### Example 1: Basic Usage with GPS Data
