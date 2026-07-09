using System.Xml.Linq;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Formatting;

public interface IGpxFormatter
{
    string FormatWaypoints(IEnumerable<LocationData> locations);
    string FormatTrack(IEnumerable<LocationData> trackPoints, string trackName);
    string FormatMultiTrack(IReadOnlyDictionary<string, IEnumerable<LocationData>> tracksByName);
}

/// <summary>
/// Serializes location data to GPX 1.1 XML (http://www.topografix.com/GPX/1/1).
/// </summary>
public class GpxFormatter : IGpxFormatter
{
    /// <summary>Creator attribute written into the gpx root element.</summary>
    public string Creator { get; set; } = "GpsTrackerProtocol";

    /// <summary>Each location becomes a <wpt lat lon> with <ele>, <time> (ISO 8601 UTC), <sat>, and <name> = LocationData.Id.</summary>
    public string FormatWaypoints(IEnumerable<LocationData> locations)
    {
        var gpx = new XDocument(
            new XElement("gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("creator", Creator),
                locations.Select(location => new XElement("wpt",
                    new XAttribute("lat", location.Latitude),
                    new XAttribute("lon", location.Longitude),
                    new XElement("ele", location.Altitude),
                    new XElement("time", location.Timestamp.ToString("o")),
                    new XElement("sat", location.SatelliteCount),
                    new XElement("name", location.Id)
                ))
            )
        );

        return gpx.ToString();
    }

    /// <summary>One <trk> with <name> and a single <trkseg> of <trkpt> elements ordered by Timestamp; speed/bearing go into <extensions>.</summary>
    public string FormatTrack(IEnumerable<LocationData> trackPoints, string trackName)
    {
        var orderedTrackPoints = trackPoints.OrderBy(tp => tp.Timestamp);

        var gpx = new XDocument(
            new XElement("gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("creator", Creator),
                new XElement("trk",
                    new XElement("name", trackName),
                    new XElement("trkseg",
                        orderedTrackPoints.Select(location => new XElement("trkpt",
                            new XAttribute("lat", location.Latitude),
                            new XAttribute("lon", location.Longitude),
                            new XElement("ele", location.Altitude),
                            new XElement("time", location.Timestamp.ToString("o")),
                            new XElement("extensions",
                                new XElement("speed", location.Speed),
                                new XElement("bearing", location.Bearing)
                            )
                        ))
                    )
                )
            )
        );

        return gpx.ToString();
    }

    /// <summary>One <trk> per dictionary entry (key = track name); useful for exporting several devices or journeys into a single file.</summary>
    public string FormatMultiTrack(IReadOnlyDictionary<string, IEnumerable<LocationData>> tracksByName)
    {
        var tracks = tracksByName.Select(kvp => new XElement("trk",
            new XElement("name", kvp.Key),
            new XElement("trkseg",
                kvp.Value.OrderBy(tp => tp.Timestamp).Select(location => new XElement("trkpt",
                    new XAttribute("lat", location.Latitude),
                    new XAttribute("lon", location.Longitude),
                    new XElement("ele", location.Altitude),
                    new XElement("time", location.Timestamp.ToString("o")),
                    new XElement("extensions",
                        new XElement("speed", location.Speed),
                        new XElement("bearing", location.Bearing)
                    )
                ))
            )
        ));

        var gpx = new XDocument(
            new XElement("gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("creator", Creator),
                tracks
            )
        );

        return gpx.ToString();
    }
}
