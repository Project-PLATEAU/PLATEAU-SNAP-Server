using PLATEAU.Snap.Models.Server;

namespace PLATEAU.Snap.Server.Services;

public interface IImageService
{
    Task<Models.Client.BuildingImageResponse> CreateBuildingImageAsync(BuildingImageRequest request);
}
