using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Models.Server;

public class GeometryInfo
{
    public int Id { get; set; }

    public Polygon Geom { get; set; } = null!;
}
