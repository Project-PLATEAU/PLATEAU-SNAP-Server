using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class SurfaceDatum
{
    public int Id { get; set; }

    public string? Gmlid { get; set; }

    public string? GmlidCodespace { get; set; }

    public string? Name { get; set; }

    public string? NameCodespace { get; set; }

    public string? Description { get; set; }

    public decimal? IsFront { get; set; }

    public int ObjectclassId { get; set; }

    public double? X3dShininess { get; set; }

    public double? X3dTransparency { get; set; }

    public double? X3dAmbientIntensity { get; set; }

    public string? X3dSpecularColor { get; set; }

    public string? X3dDiffuseColor { get; set; }

    public string? X3dEmissiveColor { get; set; }

    public decimal? X3dIsSmooth { get; set; }

    public int? TexImageId { get; set; }

    public string? TexTextureType { get; set; }

    public string? TexWrapMode { get; set; }

    public string? TexBorderColor { get; set; }

    public decimal? GtPreferWorldfile { get; set; }

    public string? GtOrientation { get; set; }

    public Point? GtReferencePoint { get; set; }

    public virtual Objectclass Objectclass { get; set; } = null!;

    public virtual TexImage? TexImage { get; set; }

    public virtual ICollection<Textureparam> Textureparams { get; set; } = new List<Textureparam>();

    public virtual ICollection<Appearance> Appearances { get; set; } = new List<Appearance>();
}
