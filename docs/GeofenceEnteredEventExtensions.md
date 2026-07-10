# GeofenceEnteredEventExtensions

The `GeofenceEnteredEventExtensions` static class provides a suite of extension methods designed to enhance the utility of `GeofenceEnteredEvent` objects within the `gps-tracker-protocol` namespace. These methods facilitate the transformation of event data into external service-friendly formats, perform spatial calculations for proximity analysis, and generate human-readable summaries for logging or UI display. By encapsulating these operations within extension methods, the class promotes cleaner, more expressive code when processing geofence-related events in GPS tracking applications.

## API

### ToWebhookPayload
Converts a `GeofenceEnteredEvent` instance into a `GeofenceWebhookPayload` suitable for transmission to external webhook endpoints.

*   **Parameters:**
    *   `event` (this `GeofenceEnteredEvent`): The event to convert.
*   **Return Value:** `GeofenceWebhookPayload` containing the mapped event data.
*   **Exceptions:** Throws `ArgumentNullException` if `event` is null.

### DistanceTo
Calculates the geodesic distance between the event location and a target coordinate.

*   **Parameters:**
    *   `event` (this `GeofenceEnteredEvent`): The event instance.
    *   `target` (`Coordinate`): The destination coordinate to calculate the distance to.
*   **Return Value:** `double` representing the distance in meters.
*   **Exceptions:** Throws `ArgumentNullException` if `event` or `target` is null.

### IsWithinBoundingBox
Determines if the event location resides within the specified geographic bounding box.

*   **Parameters:**
    *   `event` (this `GeofenceEnteredEvent`): The event instance.
    *   `box` (`BoundingBox`): The geographic boundary to check against.
*   **Return Value:** `bool` returning `true` if the event coordinate is within the bounds; otherwise, `false`.
*   **Exceptions:** Throws `ArgumentNullException` if `event` or `box` is null.

### ToSummaryString
Generates a concise, human-readable string representation of the geofence event, suitable for logging, debugging, or display in user interfaces.

*   **Parameters:**
    *   `event` (this `GeofenceEnteredEvent`): The event to summarize.
*   **Return Value:** `string` representing the event summary.
*   **Exceptions:** Throws `ArgumentNullException` if `event` is null.

## Usage

### Example 1: Preparing a Webhook Payload
```csharp
public async Task HandleEventAsync(GeofenceEnteredEvent enteredEvent)
{
    // Convert the event to the required webhook format
    GeofenceWebhookPayload payload = enteredEvent.ToWebhookPayload();
    
    // Transmit to the external service
    await _httpClient.PostAsJsonAsync("https://api.tracking.service/hooks", payload);
}
```

### Example 2: Filtering and Logging Events
```csharp
public void ProcessAndLog(GeofenceEnteredEvent enteredEvent, BoundingBox serviceArea, Coordinate depotLocation)
{
    // Perform spatial filtering
    if (enteredEvent.IsWithinBoundingBox(serviceArea))
    {
        double distance = enteredEvent.DistanceTo(depotLocation);
        
        // Use summary string for logging
        _logger.LogInformation("Event inside service area: {Summary}. Distance to depot: {Dist}m", 
            enteredEvent.ToSummaryString(), distance);
    }
}
```

## Notes

*   **Thread Safety:** The methods within this class are thread-safe, provided the `GeofenceEnteredEvent` instance passed to them remains immutable during the method execution. The methods themselves do not maintain internal state.
*   **Coordinate Validity:** The accuracy of `DistanceTo` and `IsWithinBoundingBox` relies on the validity of the underlying coordinates. Ensure that latitude and longitude values within the `GeofenceEnteredEvent` and the `Coordinate` target object are within their standard ranges ([-90, 90] for latitude, [-180, 180] for longitude).
*   **Floating-Point Precision:** `DistanceTo` relies on floating-point arithmetic. Minor discrepancies may occur due to standard IEEE 754 floating-point representation limitations.
