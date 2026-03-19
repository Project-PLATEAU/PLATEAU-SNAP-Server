namespace PLATEAU.Snap.Models.Common;

public class ExportMeshParam : AbstractJobParam
{
    public override JobType Type => JobType.export_mesh;

    public string MeshCode { get; set; } = null!;

    public string? FileName { get; set; }
}
