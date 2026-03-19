using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Image
{
    public long Id { get; set; }

    public string Uri { get; set; } = null!;

    public double FromLatitude { get; set; }

    public double FromLongitude { get; set; }

    public double FromAltitude { get; set; }

    public double ToLatitude { get; set; }

    public double ToLongitude { get; set; }

    public double ToAltitude { get; set; }

    public double Roll { get; set; }

    public Polygon Coordinates { get; set; } = null!;

    public byte[] Thumbnail { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public virtual ICollection<ImageSurfaceRelation> ImageSurfaceRelations { get; set; } = new List<ImageSurfaceRelation>();
}
