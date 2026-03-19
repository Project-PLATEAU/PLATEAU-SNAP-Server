using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class RoofSurface
{
    public int? BuildingId { get; set; }

    public int? FaceId { get; set; }

    public string? Gmlid { get; set; }

    public Geometry? Geom { get; set; }
}
