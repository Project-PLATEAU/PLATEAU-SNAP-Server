using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class SurfaceImage
{
    public int? BuildingId { get; set; }

    public int? FaceId { get; set; }

    public long? ImageId { get; set; }

    public string? Gmlid { get; set; }

    public byte[]? Thumbnail { get; set; }

    public Polygon? Coordinates { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? Uri { get; set; }

    public Geometry? Center { get; set; }
}
