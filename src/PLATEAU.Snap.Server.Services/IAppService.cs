using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Server;

namespace PLATEAU.Snap.Server.Services;

public interface IAppService
{
    Task<Models.Client.VisibleSurfacesResponse> GetVisibleSurfacesAsync(VisibleSurfacesRequest request);

    Task<Models.Client.BuildingImageResponse> CreateBuildingImageAsync(BuildingImageRequest request);
}
