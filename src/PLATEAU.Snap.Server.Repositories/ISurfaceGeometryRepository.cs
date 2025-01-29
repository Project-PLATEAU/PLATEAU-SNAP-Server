using PLATEAU.Snap.Models.Server;

namespace PLATEAU.Snap.Server.Repositories;

public interface ISurfaceGeometryRepository
{
    Task<List<PolygonInfo>> GetPolygonInfoAsync(VisibleSurfacesRequest request, int srid);

    Task<CameraInfo> GetCameraInfoAsync(VisibleSurfacesRequest request, int srid);
}
