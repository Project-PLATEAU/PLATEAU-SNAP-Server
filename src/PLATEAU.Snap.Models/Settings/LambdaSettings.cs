namespace PLATEAU.Snap.Models.Settings;

public class LambdaSettings
{
    public string TransformFunctionName { get; set; } = null!;

    public string RoofExtractionFunctionName { get; set; } = null!;

    public string ApplyTextureFunctionName { get; set; } = null!;

    public string ExportBuildingFunctionName { get; set; } = null!;

    public string ExportMeshFunctionName { get; set; } = null!;
}
