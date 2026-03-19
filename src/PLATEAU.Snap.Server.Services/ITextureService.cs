using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Models.Common;

namespace PLATEAU.Snap.Server.Services;

public interface ITextureService
{
    Task<PageData<BuildingImage>> GetBuildingsAsync(SortType sortType, int pageNumber, int pageSize);

    Task<MeshCodeResponse> GetMeshCodeAsync(int buildingId);

    Task<PageData<FaceImageInfo>> GetFacesAsync(int buildingId, SortType sortType, int pageNumber, int pageSize);

    Task<PageData<ImageInfo>> GetFaceImagesAsync(int buildingId, int faceId, SortType sortType, int pageNumber, int pageSize);

    Task<Models.Client.TransformResponse> TransformAsync(Models.Client.TransformRequest payload);

    Task<Models.Client.RoofExtractionResponse> RoofExtractionAsync(Models.Client.RoofExtractionRequest payload);

    Task<PreviewTextureResponse> PreviewTextureRequest(PreviewTextureRequest payload);

    Task ApplyTextureAsync(ApplyTextureRequest payload);

    Task<Job> ExportAsync(int buildingId, string? fileName);

    Task<Job> ExportAsync(string meshCode, string? fileName);
}
