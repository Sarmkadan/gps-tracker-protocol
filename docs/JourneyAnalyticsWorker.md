# JourneyAnalyticsWorker

A lightweight worker class that computes and aggregates journey analytics for a GPS-tracking device. It processes raw telemetry data to derive key performance indicators such as total distance traveled, duration, average speed, speeding incidents, and idle time percentage. The class is designed to be instantiated per device and reused across multiple analysis cycles.

## API

### `public JourneyAnalyticsWorker()`

Initializes a new instance of the `JourneyAnalyticsWorker` with default values for all metrics. All properties will be zero or empty until populated by subsequent analysis operations.

### `public string DeviceId`

Gets the unique identifier of the device whose journey data is being analyzed. This value is set during analysis and remains constant for the lifetime of the worker instance.

### `public DateTime AnalysisTime`

Gets the timestamp indicating when the journey analytics were computed. This reflects the moment the analysis was finalized and is typically set to `DateTime.UtcNow` at the end of the analysis process.

### `public int TotalJourneys`

Gets the total number of distinct journeys detected during the analysis period. A journey is defined as a continuous period of movement separated by periods of inactivity or device shutdown. The value is non-negative and reflects the count of completed journeys.

### `public double TotalDistanceKm`

Gets the cumulative distance traveled across all journeys, expressed in kilometers. The value is a non-negative floating-point number representing the sum of all segment distances. Precision is limited by the underlying GPS data resolution.

### `public double TotalDurationHours`

Gets the total duration of all journeys combined, expressed in hours. This includes both active driving time and any periods of motion below the minimum speed threshold. The value is a non-negative floating-point number and may include minor rounding due to time aggregation.

### `public double AverageSpeedKmh`

Gets the average speed across all journeys, calculated as `TotalDistanceKm / TotalDurationHours`. If `TotalDurationHours` is zero, this property returns `0.0` to avoid division by zero. The result is a non-negative floating-point number rounded to two decimal places.

### `public int SpeedingIncidents`

Gets the number of times the device exceeded the configured speeding threshold during the analysis period. Each continuous period above the threshold counts as one incident. The value is a non-negative integer and reflects the count of distinct threshold breaches.

### `public double IdleTimePercentage`

Gets the percentage of total analysis time spent in an idle state, where the device was powered on but not moving above the minimum speed threshold. Calculated as `(IdleDuration / TotalDurationHours) * 100`. The value is a non-negative floating-point number between `0.0` and `100.0`, inclusive. If `TotalDurationHours` is zero, this property returns `0.0`.

## Usage
