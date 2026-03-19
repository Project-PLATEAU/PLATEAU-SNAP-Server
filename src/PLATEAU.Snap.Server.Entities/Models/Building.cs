using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace PLATEAU.Snap.Server.Entities.Models;

public partial class Building
{
    public int Id { get; set; }

    public int ObjectclassId { get; set; }

    public int? BuildingParentId { get; set; }

    public int? BuildingRootId { get; set; }

    public string? Class { get; set; }

    public string? ClassCodespace { get; set; }

    public string? Function { get; set; }

    public string? FunctionCodespace { get; set; }

    public string? Usage { get; set; }

    public string? UsageCodespace { get; set; }

    public DateOnly? YearOfConstruction { get; set; }

    public DateOnly? YearOfDemolition { get; set; }

    public string? RoofType { get; set; }

    public string? RoofTypeCodespace { get; set; }

    public double? MeasuredHeight { get; set; }

    public string? MeasuredHeightUnit { get; set; }

    public decimal? StoreysAboveGround { get; set; }

    public decimal? StoreysBelowGround { get; set; }

    public string? StoreyHeightsAboveGround { get; set; }

    public string? StoreyHeightsAgUnit { get; set; }

    public string? StoreyHeightsBelowGround { get; set; }

    public string? StoreyHeightsBgUnit { get; set; }

    public MultiLineString? Lod1TerrainIntersection { get; set; }

    public MultiLineString? Lod2TerrainIntersection { get; set; }

    public MultiLineString? Lod3TerrainIntersection { get; set; }

    public MultiLineString? Lod4TerrainIntersection { get; set; }

    public MultiLineString? Lod2MultiCurve { get; set; }

    public MultiLineString? Lod3MultiCurve { get; set; }

    public MultiLineString? Lod4MultiCurve { get; set; }

    public int? Lod0FootprintId { get; set; }

    public int? Lod0RoofprintId { get; set; }

    public int? Lod1MultiSurfaceId { get; set; }

    public int? Lod2MultiSurfaceId { get; set; }

    public int? Lod3MultiSurfaceId { get; set; }

    public int? Lod4MultiSurfaceId { get; set; }

    public int? Lod1SolidId { get; set; }

    public int? Lod2SolidId { get; set; }

    public int? Lod3SolidId { get; set; }

    public int? Lod4SolidId { get; set; }

    public virtual Building? BuildingParent { get; set; }

    public virtual Building? BuildingRoot { get; set; }

    public virtual Cityobject IdNavigation { get; set; } = null!;

    public virtual ICollection<Building> InverseBuildingParent { get; set; } = new List<Building>();

    public virtual ICollection<Building> InverseBuildingRoot { get; set; } = new List<Building>();

    public virtual SurfaceGeometry? Lod0Footprint { get; set; }

    public virtual SurfaceGeometry? Lod0Roofprint { get; set; }

    public virtual SurfaceGeometry? Lod1MultiSurface { get; set; }

    public virtual SurfaceGeometry? Lod1Solid { get; set; }

    public virtual SurfaceGeometry? Lod2MultiSurface { get; set; }

    public virtual SurfaceGeometry? Lod2Solid { get; set; }

    public virtual SurfaceGeometry? Lod3MultiSurface { get; set; }

    public virtual SurfaceGeometry? Lod3Solid { get; set; }

    public virtual SurfaceGeometry? Lod4MultiSurface { get; set; }

    public virtual SurfaceGeometry? Lod4Solid { get; set; }

    public virtual Objectclass Objectclass { get; set; } = null!;
}
