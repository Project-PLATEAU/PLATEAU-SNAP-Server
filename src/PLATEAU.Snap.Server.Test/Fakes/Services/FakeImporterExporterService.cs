using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Lambda;
using PLATEAU.Snap.Server.Services;

namespace PLATEAU.Snap.Server.Test.Fakes.Services;

internal class FakeImporterExporterService : IImporterExporterService
{
    public Task<Stream> ExportAsync(int buildingId)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> ExportAsync(string meshCode)
    {
        throw new NotImplementedException();
    }

    public Task<ExportBuildingResultParam> ExportAsync(LambdaExportBuildingRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ExportMeshResultParam> ExportAsync(LambdaExportMeshRequest request)
    {
        throw new NotImplementedException();
    }
}
