using System;
using System.Collections.Generic;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Objectclass
{
    public int Id { get; set; }

    public decimal? IsAdeClass { get; set; }

    public decimal? IsToplevel { get; set; }

    public string? Classname { get; set; }

    public string? Tablename { get; set; }

    public int? SuperclassId { get; set; }

    public int? BaseclassId { get; set; }

    public int? AdeId { get; set; }

    public virtual Objectclass? Baseclass { get; set; }

    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    public virtual ICollection<Cityobject> Cityobjects { get; set; } = new List<Cityobject>();

    public virtual ICollection<Objectclass> InverseBaseclass { get; set; } = new List<Objectclass>();

    public virtual ICollection<Objectclass> InverseSuperclass { get; set; } = new List<Objectclass>();

    public virtual Objectclass? Superclass { get; set; }

    public virtual ICollection<SurfaceDatum> SurfaceData { get; set; } = new List<SurfaceDatum>();
}
