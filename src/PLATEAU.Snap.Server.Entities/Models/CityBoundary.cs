using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class CityBoundary
{
    public int Fid { get; set; }

    public string? GstCssName { get; set; }

    public string? SystemNumber { get; set; }

    public string? AreaCode { get; set; }

    public string? PrefName { get; set; }

    public string? CityName { get; set; }

    public string? GstName { get; set; }

    public string? CssName { get; set; }

    public double? Area { get; set; }

    public double? XCode { get; set; }

    public double? YCode { get; set; }

    public Geometry? Geom { get; set; }
}
