# IGpxFormatter
The `IGpxFormatter` type is designed to provide a standardized way of formatting GPS data, specifically GPX (GPS Exchange Format) files. It allows for the creation of custom formatting logic for waypoints, tracks, and multi-tracks, enabling developers to tailor the output to their specific requirements.

## API
The `IGpxFormatter` interface exposes the following public members:
* `Creator`: A string representing the creator of the GPX file.
* `FormatWaypoints`: A string that defines how waypoints should be formatted.
* `FormatTrack`: A string that defines how tracks should be formatted.
* `FormatMultiTrack`: A string that defines how multi-tracks should be formatted.

## Usage
Here are two examples of using the `IGpxFormatter` interface in C#:
```csharp
// Example 1: Basic usage
IGpxFormatter formatter = new GpxFormatter();
string creator = formatter.Creator;
string formattedWaypoints = formatter.FormatWaypoints;
string formattedTrack = formatter.FormatTrack;
string formattedMultiTrack = formatter.FormatMultiTrack;

// Example 2: Custom formatting
public class CustomGpxFormatter : IGpxFormatter
{
    public string Creator => "Custom Creator";
    public string FormatWaypoints => "Custom Waypoint Format";
    public string FormatTrack => "Custom Track Format";
    public string FormatMultiTrack => "Custom Multi-Track Format";
}

IGpxFormatter customFormatter = new CustomGpxFormatter();
string customCreator = customFormatter.Creator;
string customFormattedWaypoints = customFormatter.FormatWaypoints;
string customFormattedTrack = customFormatter.FormatTrack;
string customFormattedMultiTrack = customFormatter.FormatMultiTrack;
```

## Notes
When implementing the `IGpxFormatter` interface, consider the following edge cases:
* The `Creator` property should return a non-empty string to ensure proper attribution.
* The `FormatWaypoints`, `FormatTrack`, and `FormatMultiTrack` properties should return valid formatting strings to avoid errors during GPX file creation.
* The `IGpxFormatter` interface does not provide any thread-safety guarantees, so implementations should ensure that their properties are properly synchronized if accessed from multiple threads. Additionally, the `IGpxFormatter` interface does not specify any exceptions that may be thrown, so implementers should carefully consider the potential exceptions that may occur during formatting and handle them accordingly.
