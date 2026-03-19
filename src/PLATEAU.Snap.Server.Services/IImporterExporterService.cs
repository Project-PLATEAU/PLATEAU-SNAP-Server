using PLATEAU.Snap.Models.Common;

namespace PLATEAU.Snap.Server.Services;

public interface IImporterExporterService
{
    Task<Stream> ExportAsync(int buildingId);

    Task<Stream> ExportAsync(string meshCode);

    Task<ExportBuildingResultParam> ExportAsync(Models.Lambda.LambdaExportBuildingRequest request);

    Task<ExportMeshResultParam> ExportAsync(Models.Lambda.LambdaExportMeshRequest request);
}
