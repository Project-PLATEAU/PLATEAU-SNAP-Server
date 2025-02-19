using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Models.Client;

public class Surface
{
    public string Gmlid { get; set; } = null!;

    public List<double[][]> Coordinates { get; set; } = new ();

    public Surface(string gmlId, Polygon polygon) : this(gmlId, polygon, 0)
    {      
    }

    public Surface(string gmlId, Polygon polygon, double geoidHeight)
    {
        Gmlid = gmlId;

        this.Coordinates.Add(polygon.ExteriorRing.Coordinates.Select(c => new double[] { c.X, c.Y, c.Z + geoidHeight }).ToArray());
        foreach (var interiorRing in polygon.InteriorRings)
        {
            this.Coordinates.Add(interiorRing.Coordinates.Select(c => new double[] { c.X, c.Y, c.Z + geoidHeight }).ToArray());
        }
    }
}
