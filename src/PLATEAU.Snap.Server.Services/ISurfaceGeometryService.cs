using PLATEAU.Snap.Models.Server;

namespace PLATEAU.Snap.Server.Services;

public interface ISurfaceGeometryService
{
    public Task<Models.Client.VisibleSurfacesResponse> GetVisibleSurfacesAsync(VisibleSurfacesRequest request);
}
