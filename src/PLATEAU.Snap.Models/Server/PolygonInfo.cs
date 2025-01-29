using NetTopologySuite.Geometries;
using PLATEAU.Snap.Models.Extensions.Geometries;
using System.Numerics;

namespace PLATEAU.Snap.Models.Server;

public class PolygonInfo
{
    //private Polygon? _projectionPolygon = null;

    public int Id { get; set; }

    public string Gmlid { get; set; }

    public Polygon Polygon { get; set; }

    public Polygon PlanePolygon { get; set; }

    public Plane Plane { get; set; }

    public Vector3 FirstVector => PlanePolygon.Coordinates.First().ToVector3();

    //public Polygon ProjectionPolygon
    //{
    //    get
    //    {
    //        return _projectionPolygon != null ? _projectionPolygon : throw new InvalidOperationException();
    //    }
    //    set
    //    {
    //        _projectionPolygon = value;
    //    }
    //}

    public Vector3[] Vectors => PlanePolygon.Coordinates.Select(c => c.ToVector3()).ToArray();

    public PolygonInfo(int id, string gmlId, Polygon polygon, Polygon planePolygon)
    {
        Id = id;
        Gmlid = gmlId;
        Polygon = polygon;
        PlanePolygon = planePolygon;

        var vectors = planePolygon.Coordinates.Take(3).Select(c => c.ToVector3()).ToArray();
        Plane = Plane.CreateFromVertices(vectors[0], vectors[1], vectors[2]);
    }
}
