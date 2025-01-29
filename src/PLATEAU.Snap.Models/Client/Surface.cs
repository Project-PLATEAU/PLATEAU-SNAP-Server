using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Models.Client;

public class Surface
{

    public string Gmlid { get; set; } = null!;

    public List<List<double[][]>> Coordinates { get; set; } = new ();

    public Surface(string gmlId, Polygon polygon)
    {
        Gmlid = gmlId;

        var exteriorRingCoordinates = new List<double[][]>();
        exteriorRingCoordinates.Add(polygon.ExteriorRing.Coordinates.Select(c => new double[] { c.X, c.Y, c.Z }).ToArray());
        this.Coordinates.Add(exteriorRingCoordinates);

        foreach (var interiorRing in polygon.InteriorRings)
        {
            var interiorRingCoordinates = new List<double[][]>();
            interiorRingCoordinates.Add(interiorRing.Coordinates.Select(c => new double[] { c.X, c.Y, c.Z }).ToArray());
            this.Coordinates.Add(interiorRingCoordinates);
        }        
    }
}
