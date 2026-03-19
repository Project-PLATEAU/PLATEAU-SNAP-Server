using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Server.Services;

namespace PLATEAU.Snap.Server.Test.Fakes.Services;

internal class FakeTextureService : ITextureService
{
    public List<BuildingImage> BuildingImages { get; } = new();

    public List<FaceImageInfo> FaceImages { get; } = new();

    public List<ImageInfo> FaceImageInfos { get; } = new();

    public Task<VisibleSurfacesResponse> GetVisibleSurfacesAsync(Models.Server.VisibleSurfacesRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<BuildingImageResponse> CreateBuildingImageAsync(Models.Server.BuildingImageRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<PageData<BuildingImage>> GetBuildingsAsync(SortType sortType, int pageNumber, int pageSize)
    {
        return await Task.FromResult(PageList<BuildingImage>.ToPageList(BuildingImages.AsQueryable(), pageNumber, pageSize).CreatePageData());
    }

    public Task<MeshCodeResponse> GetMeshCodeAsync(int buildingId)
    {
        var response = new MeshCodeResponse
        {
            MeshCode = "53394509",
        };
        return Task.FromResult(response);
    }

    public Task<PageData<FaceImageInfo>> GetFacesAsync(int buildingId, SortType sortType, int pageNumber, int pageSize)
    {
        return Task.FromResult(PageList<FaceImageInfo>.ToPageList(FaceImages.AsQueryable(), pageNumber, pageSize).CreatePageData());
    }

    public Task<PageData<ImageInfo>> GetFaceImagesAsync(int buildingId, int faceId, SortType sortType, int pageNumber, int pageSize)
    {
        return Task.FromResult(PageList<ImageInfo>.ToPageList(FaceImageInfos.AsQueryable(), pageNumber, pageSize).CreatePageData());
    }

    public Task<TransformResponse> TransformAsync(TransformRequest payload)
    {
        var response = new TransformResponse
        {
            Path = "s3://temp/transform.png",
            Uri = "https://example.com/transform.png",
            Coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))",
        };
        return Task.FromResult(response);
    }

    public Task<RoofExtractionResponse> RoofExtractionAsync(RoofExtractionRequest payload)
    {
        var response = new RoofExtractionResponse
        {
            Path = "s3://temp/roof_extraction.png",
            Uri = "https://example.com/transform.png",
            Coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))",
        };
        return Task.FromResult(response);
    }

    public Task<PreviewTextureResponse> PreviewTextureRequest(PreviewTextureRequest payload)
    {
        throw new NotImplementedException();
    }

    public Task ApplyTextureAsync(ApplyTextureRequest payload)
    {
        return Task.CompletedTask;
    }

    public Task<Job> ExportAsync(int id, string? fileName)
    {
        return Task.FromResult(new Job
        {
            Id = 1
        });
    }

    public Task<Job> ExportAsync(string meshCode, string? fileName)
    {
        return Task.FromResult(new Job
        {
            Id = 1
        });
    }
}
