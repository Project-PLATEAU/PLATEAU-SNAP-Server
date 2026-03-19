namespace PLATEAU.Snap.Models.Common;

public class ExportBuildingResultParam : AbstractJobResultParam
{
    public override JobType Type => JobType.export_mesh;

    public string? Path { get; set; }
}
