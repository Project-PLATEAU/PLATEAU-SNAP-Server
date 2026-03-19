using NetTopologySuite.Geometries;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Entities.Models;

namespace PLATEAU.Snap.Server.Repositories;

public interface ISurfaceGeometryRepository
{
    Task<List<PolygonInfo>> GetPolygonInfoAsync(VisibleSurfacesRequest request, int srid);

    Task<CameraInfo> GetCameraInfoAsync(VisibleSurfacesRequest request, int srid);

    Task<PageList<BuildingImage>> GetBuildingsAsync(SortType sortType, int pageNumber, int pageSize);

    Task<PageList<BuildingFace>> GetFacesAsync(int buildingId, SortType sortType, int pageNumber, int pageSize);

    Task<PageList<BuildingFace>> GetFaceImagesAsync(int buildingId, int faceId, SortType sortType, int pageNumber, int pageSize);

    Task<string?> GetFaceWktAsync(int faceId);

    Task<SurfaceImage?> GetSurfaceImageAsync(int buildingId, int faceId, long imageId);

    Task<RoofSurface?> GetRoofSurfaceAsync(int buildingId, int faceId);

    Task<bool> ExistsAsync(int buildingId);

    Task<bool> ExistsAsync(int buildingId, int faceId);

    Task<Geometry?> GetEnvelopeGeometryAsync(int buildingId);

    Task<Geometry?> GetRoofprintAsync(int buildingId);

    Task<List<int>> GetIntersectionsAsync(Polygon envelope);

    Task<List<GeometryInfo>> GetNotContainsAsync(Polygon envelope, int[] includeIds);
}
