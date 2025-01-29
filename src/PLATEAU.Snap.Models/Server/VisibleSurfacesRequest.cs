using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Models.Server;

public class VisibleSurfacesRequest
{
    public CoordinateZ From { get; set; } = null!;

    public CoordinateZ To { get; set; } = null!;

    public double Roll { get; set; }

    public double MinDistance { get; set; }

    public double MaxDistance { get; set; }

    public double FieldOfView { get; set; }
}
