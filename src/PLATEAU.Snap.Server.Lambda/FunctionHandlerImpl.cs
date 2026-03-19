using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Lambda;
using PLATEAU.Snap.Server.Services;

namespace PLATEAU.Snap.Server.Lambda;

internal class FunctionHandlerImpl
{
    private readonly ILogger<FunctionHandlerImpl> logger;

    private readonly IImporterExporterService service;

    public FunctionHandlerImpl(ILogger<FunctionHandlerImpl> logger, IImporterExporterService service)
    {
        this.logger = logger;
        this.service = service;
    }

    public async Task<ExportBuildingResultParam> ExportBuildingAsync(LambdaExportBuildingRequest request, ILambdaContext context)
    {
        return await service.ExportAsync(request);
    }

    public async Task<ExportMeshResultParam> ExportMeshAsync(LambdaExportMeshRequest request, ILambdaContext context)
    {
        return await service.ExportAsync(request);
    }
}
