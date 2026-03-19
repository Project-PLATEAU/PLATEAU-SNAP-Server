using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Textureparam
{
    public int SurfaceGeometryId { get; set; }

    public decimal? IsTextureParametrization { get; set; }

    public string? WorldToTexture { get; set; }

    public Polygon? TextureCoordinates { get; set; }

    public int SurfaceDataId { get; set; }

    public virtual SurfaceDatum SurfaceData { get; set; } = null!;

    public virtual SurfaceGeometry SurfaceGeometry { get; set; } = null!;
}
