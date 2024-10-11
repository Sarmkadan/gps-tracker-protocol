# IGeoJsonFormatter
The `IGeoJsonFormatter` interface is designed to provide a standardized way of formatting geographic data into GeoJSON format. It allows for the creation of GeoJSON objects that represent locations, tracks, and collections of locations, making it easier to work with geographic data in a variety of applications.

## API
* `public string FormatLocation`: Returns a string representing a location in GeoJSON format.
* `public string FormatTrack`: Returns a string representing a track in GeoJSON format.
* `public string FormatLocationCollection`: Returns a string representing a collection of locations in GeoJSON format.
* `public string Type`: Gets the type of the GeoJSON object.
* `public GeoJsonGeometry Geometry`: Gets the geometry of the GeoJSON object.
* `public Dictionary<string, object> Properties`: Gets the properties of the GeoJSON object.
* `public object Coordinates`: Gets the coordinates of the GeoJSON object.
* `public List<GeoJsonFeature> Features`: Gets the features of the GeoJSON object.

## Usage
The following examples demonstrate how to use the `IGeoJsonFormatter` interface:
```csharp
// Example 1: Formatting a location
IGeoJsonFormatter formatter = new GeoJsonFormatter();
string locationJson = formatter.FormatLocation();
Console.WriteLine(locationJson);

// Example 2: Creating a track and formatting it
IGeoJsonFormatter trackFormatter = new GeoJsonFormatter();
List<GeoJsonGeometry> trackGeometries = new List<GeoJsonGeometry>();
// Add geometries to the track
string trackJson = trackFormatter.FormatTrack();
Console.WriteLine(trackJson);
```

## Notes
When working with the `IGeoJsonFormatter` interface, it's essential to consider the following edge cases:
* The `FormatLocation`, `FormatTrack`, and `FormatLocationCollection` methods may throw exceptions if the input data is invalid or cannot be formatted correctly.
* The `Type`, `Geometry`, `Properties`, `Coordinates`, and `Features` properties may return null or empty values if the corresponding data is not available.
* The `IGeoJsonFormatter` interface is not thread-safe by default, so it's crucial to implement proper synchronization mechanisms when using it in multi-threaded environments.
* The `GeoJsonGeometry` and `GeoJsonFeature` classes used in the `IGeoJsonFormatter` interface may have their own set of rules and constraints that need to be considered when working with them.
