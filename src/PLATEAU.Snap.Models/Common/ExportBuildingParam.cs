namespace PLATEAU.Snap.Models.Common;

public class ExportBuildingParam : AbstractJobParam
{
    public override JobType Type => JobType.export_building;

    public int BuildingId { get; set; }

    public string? FileName { get; set; }
}
