using NetTopologySuite.IO;
using PLATEAU.Snap.Models.Client;
using System.Text.Json.Serialization;

namespace PLATEAU.Snap.Models.Server;

public class BuildingImageMetadata
{
    public string Gmlid { get; set; } = null!;

    public Coordinate From { get; set; } = null!;

    public Coordinate To { get; set; } = null!;

    public double Roll { get; set; }

    public string Coordinates { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    [JsonIgnore]
    public NetTopologySuite.Geometries.Polygon Polygon { get; set; } = null!;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Gmlid))
        {
            throw new ArgumentException($"{nameof(Gmlid)} is required.");
        }
        if (From == null)
        {
            throw new ArgumentException($"{nameof(From)} is required.");
        }
        if (To == null)
        {
            throw new ArgumentException($"{nameof(To)} is required.");
        }
        if (string.IsNullOrEmpty(Coordinates))
        {
            throw new ArgumentException($"{nameof(Coordinates)} is required.");
        }
        var reader = new WKTReader();
        var polygon = reader.Read(Coordinates) as NetTopologySuite.Geometries.Polygon;
        if (polygon is null || !polygon.IsValid)
        {
            throw new ArgumentException($"{nameof(Coordinates)} is not a valid polygon.");
        }
        Polygon = polygon;

        if (Timestamp == default)
        {
            throw new ArgumentException($"{nameof(Timestamp)} is required.");
        }

        From.Validate();
        To.Validate();
    }
}
