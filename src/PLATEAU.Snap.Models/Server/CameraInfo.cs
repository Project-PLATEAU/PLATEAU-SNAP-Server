using NetTopologySuite.Geometries;
using PLATEAU.Snap.Models.Extensions.Geometries;
using System.Numerics;

namespace PLATEAU.Snap.Models.Server;

public class CameraInfo
{
    public Coordinate FromCoordinate { get; set; }

    public Coordinate ToCoordinate { get; set; }

    public Vector3 From => FromCoordinate.ToVector3();

    public Vector3 To => ToCoordinate.ToVector3();

    public Vector3 Direction => Vector3.Normalize(To - From);

    public CameraInfo(Coordinate fromCoordinate, Coordinate toCoordinate)
    {
        FromCoordinate = fromCoordinate;
        ToCoordinate = toCoordinate;
    }
}
