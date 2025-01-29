using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class SurfaceGeometry
{
    public int Id { get; set; }

    public string? Gmlid { get; set; }

    public string? GmlidCodespace { get; set; }

    public int? ParentId { get; set; }

    public int? RootId { get; set; }

    public decimal? IsSolid { get; set; }

    public decimal? IsComposite { get; set; }

    public decimal? IsTriangulated { get; set; }

    public decimal? IsXlink { get; set; }

    public decimal? IsReverse { get; set; }

    public Polygon? Geometry { get; set; }

    public Polygon? ImplicitGeometry { get; set; }

    public int? CityobjectId { get; set; }

    public virtual ICollection<SurfaceGeometry> InverseParent { get; set; } = new List<SurfaceGeometry>();

    public virtual ICollection<SurfaceGeometry> InverseRoot { get; set; } = new List<SurfaceGeometry>();

    public virtual SurfaceGeometry? Parent { get; set; }

    public virtual SurfaceGeometry? Root { get; set; }
}
