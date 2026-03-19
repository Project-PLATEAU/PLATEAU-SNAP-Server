using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Entities.Models;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Test.Fakes.Repositories;

internal class FakeSurfaceGeometryRepository : ISurfaceGeometryRepository
{
    private GeometryFactory geometryFactory = new();

    public List<BuildingFace> BuildingFaces { get; } = new();

    public bool IsFaceWktNull { get; set; }

    public bool IsRoofprintNull { get; set; }

    public Task<List<PolygonInfo>> GetPolygonInfoAsync(VisibleSurfacesRequest request, int srid)
    {
        throw new NotImplementedException();
    }

    public Task<CameraInfo> GetCameraInfoAsync(VisibleSurfacesRequest request, int srid)
    {
        throw new NotImplementedException();
    }

    public async Task<PageList<BuildingImage>> GetBuildingsAsync(SortType sortType, int pageNumber, int pageSize)
    {
        var query = BuildingFaces.AsQueryable().Where(x => !x.IsOrtho!.Value).DistinctBy(b => b.BuildingId);
        query = sortType switch
        {
            SortType.id_asc => query.OrderBy(b => b.BuildingId),
            SortType.id_desc => query.OrderByDescending(b => b.BuildingId),
            _ => query
        };

        return await Task.FromResult(PageList<BuildingFace>.ToPageListWithSelect(
            query,
            (x) => new BuildingImage() { Id = x.BuildingId!.Value, Gmlid = x.Gmlid!, ThumbnailBytes = x.Thumbnail!, Address = "東京都中央区八重洲２丁目" },
            pageNumber,
            pageSize));
    }

    public async Task<PageList<BuildingFace>> GetFacesAsync(int buildingId, SortType sortType, int pageNumber, int pageSize)
    {
        var query = BuildingFaces.AsQueryable().Where(x => x.BuildingId == buildingId).DistinctBy(x => x.FaceId);
        query = sortType switch
        {
            SortType.id_asc => query.OrderBy(b => b.FaceId),
            SortType.id_desc => query.OrderByDescending(b => b.FaceId),
            _ => query
        };

        return await Task.FromResult(PageList<BuildingFace>.ToPageList(query, pageNumber, pageSize));
    }

    public async Task<PageList<BuildingFace>> GetFaceImagesAsync(int buildingId, int faceId, SortType sortType, int pageNumber, int pageSize)
    {
        var query = BuildingFaces.AsQueryable().Where(x => x.BuildingId == buildingId && x.FaceId == faceId);
        query = sortType switch
        {
            SortType.id_asc => query.OrderBy(b => b.ImageId),
            SortType.id_desc => query.OrderByDescending(b => b.ImageId),
            _ => query
        };

        return await Task.FromResult(PageList<BuildingFace>.ToPageList(query, pageNumber, pageSize));
    }

    public async Task<string?> GetFaceWktAsync(int faceId)
    {
        if (IsFaceWktNull)
        {
            return null;
        }

        var surfaceImage = BuildingFaces.FirstOrDefault(x => x.FaceId == faceId);
        if (surfaceImage == null)
        {
            return null;
        }

        var writer = new WKTWriter();
        return await Task.FromResult(writer.Write(surfaceImage.Coordinates));
    }

    public async Task<SurfaceImage?> GetSurfaceImageAsync(int buildingId, int faceId, long imageId)
    {
        return await Task.FromResult(BuildingFaces.Where(x => x.BuildingId == buildingId && x.FaceId == faceId && x.ImageId == imageId && !x.IsOrtho!.Value).Select(x => new SurfaceImage()
        {
            BuildingId = x.BuildingId!.Value,
            FaceId = x.FaceId,
            ImageId = x.ImageId,
            Gmlid = x.Gmlid,
            Thumbnail = x.Thumbnail,
            Coordinates = x.Coordinates as Polygon,
            Timestamp = x.Timestamp,
            Uri = $"s3://{x.BuildingId!.Value}/{x.FaceId}/{x.ImageId}.png",
            Center = x.Coordinates?.Centroid
        }).FirstOrDefault());
    }

    public async Task<RoofSurface?> GetRoofSurfaceAsync(int buildingId, int faceId)
    {
        return await Task.FromResult(BuildingFaces.Where(x => x.BuildingId == buildingId && x.FaceId == faceId && x.IsOrtho!.Value).Select(x => new RoofSurface()
        {
            BuildingId = x.BuildingId!.Value,
            FaceId = x.FaceId,
            Gmlid = x.Gmlid,
            Geom = x.Coordinates
        }).FirstOrDefault());
    }

    public async Task<bool> ExistsAsync(int buildingId)
    {
        return await Task.FromResult(BuildingFaces.Any(x => x.BuildingId == buildingId));
    }

    public async Task<bool> ExistsAsync(int buildingId, int faceId)
    {
        return await Task.FromResult(BuildingFaces.Any(x => x.BuildingId == buildingId && x.FaceId == faceId));
    }

    public async Task<Geometry?> GetEnvelopeGeometryAsync(int buildingId)
    {
        var geometry = geometryFactory.CreateLineString(
        [
            new CoordinateZ(139.77269201771884, 35.64980103144675, 0),
            new CoordinateZ(139.77342995177577, 35.650073717982856, 5.14),
        ]);
        return await Task.FromResult(geometry);
    }

    public Task<Geometry?> GetRoofprintAsync(int buildingId)
    {
        if (IsRoofprintNull)
        {
            return Task.FromResult<Geometry?>(null);
        }

        var geometry = geometryFactory.CreatePolygon(
        [
            new Coordinate(139.77269201771884, 35.64980103144675),
            new Coordinate(139.77342995177577, 35.650073717982856),
            new Coordinate(139.77269201771884, 35.64980103144675),
        ]);
        return Task.FromResult<Geometry?>(geometry);
    }

    public Task<List<int>> GetIntersectionsAsync(Polygon envelope)
    {
        throw new NotImplementedException();
    }

    public Task<List<GeometryInfo>> GetNotContainsAsync(Polygon envelope, int[] includeIds)
    {
        throw new NotImplementedException();
    }
}
