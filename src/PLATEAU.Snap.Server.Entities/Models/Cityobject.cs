using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Cityobject
{
    public int Id { get; set; }

    public int ObjectclassId { get; set; }

    public string? Gmlid { get; set; }

    public string? GmlidCodespace { get; set; }

    public string? Name { get; set; }

    public string? NameCodespace { get; set; }

    public string? Description { get; set; }

    public Polygon? Envelope { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? TerminationDate { get; set; }

    public string? RelativeToTerrain { get; set; }

    public string? RelativeToWater { get; set; }

    public DateTime? LastModificationDate { get; set; }

    public string? UpdatingPerson { get; set; }

    public string? ReasonForUpdate { get; set; }

    public string? Lineage { get; set; }

    public string? XmlSource { get; set; }

    public virtual ICollection<Appearance> Appearances { get; set; } = new List<Appearance>();

    public virtual Building? Building { get; set; }

    public virtual Objectclass Objectclass { get; set; } = null!;

    public virtual ICollection<SurfaceGeometry> SurfaceGeometries { get; set; } = new List<SurfaceGeometry>();
}
