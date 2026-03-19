using PLATEAU.Snap.Models.Lambda;

namespace PLATEAU.Snap.Server.Services;

public interface IInvokerService
{
    Task<LambdaTransformResponse> TransformAsync(LambdaTransformRequest request);

    Task<LambdaRoofExtractionResponse> RoofExtractionAsync(LambdaRoofExtractionRequest request);

    Task<LambdaApplyTextureResponse> ApplyTextureAsync(LambdaApplyTextureRequest request);

    Task ExportBuildingAsync(LambdaExportBuildingRequest request);

    Task ExportMeshAsync(LambdaExportMeshRequest request);
}
