using System;
using System.Collections.Generic;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Appearance
{
    public int Id { get; set; }

    public string? Gmlid { get; set; }

    public string? GmlidCodespace { get; set; }

    public string? Name { get; set; }

    public string? NameCodespace { get; set; }

    public string? Description { get; set; }

    public string? Theme { get; set; }

    public int? CitymodelId { get; set; }

    public int? CityobjectId { get; set; }

    public virtual Cityobject? Cityobject { get; set; }

    public virtual ICollection<SurfaceDatum> SurfaceData { get; set; } = new List<SurfaceDatum>();
}
