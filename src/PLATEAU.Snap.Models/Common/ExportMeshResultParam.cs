namespace PLATEAU.Snap.Models.Common;

public class ExportMeshResultParam : AbstractJobResultParam
{
    public override JobType Type => JobType.export_mesh;

    public string Path { get; set; } = null!;
}
