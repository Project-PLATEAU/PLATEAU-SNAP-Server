using PLATEAU.Snap.Models.Extensions.Numerics;
using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Repositories;

namespace PLATEAU.Snap.Server.Services;

internal class SurfaceGeometryService : ISurfaceGeometryService
{
    private readonly ISurfaceGeometryRepository repository;

    private readonly ICityBoundaryRepository cityBoundaryRepository;

    public SurfaceGeometryService(ISurfaceGeometryRepository repository, ICityBoundaryRepository cityBoundaryRepository)
    {
        this.repository = repository;
        this.cityBoundaryRepository = cityBoundaryRepository;
    }

    public async Task<Models.Client.VisibleSurfacesResponse> GetVisibleSurfacesAsync(VisibleSurfacesRequest request)
    {
        // sridを取得
        var srid = await this.cityBoundaryRepository.GetSrid(request.From);

        // 候補の面を取得
        var polygons = await this.repository.GetPolygonInfoAsync(request, srid);
        if (polygons.Count == 0)
        {
            return new Models.Client.VisibleSurfacesResponse();
        }

        // カメラの情報を取得
        var cameraInfo = await this.repository.GetCameraInfoAsync(request, srid);

        // カメラの視野角内に入ってカメラの方を向いている面を取得
        var facingPolygons = GetFacingPolygons(polygons, cameraInfo);
        if (facingPolygons.Count == 0)
        {
            return new Models.Client.VisibleSurfacesResponse();
        }

        var response = new Models.Client.VisibleSurfacesResponse();
        response.Surfaces.AddRange(facingPolygons.Select(p => new Models.Client.Surface(p.Gmlid, p.Polygon)));

        return response;
    }

    private List<PolygonInfo> GetFacingPolygons(List<PolygonInfo> polygons, CameraInfo cameraInfo)
    {
        // 候補の地物を充分に絞り込んでいるため、Listでよいと思われる
        // あまり数が多いならLinkedListの使用を検討する
        var list = new List<PolygonInfo>();
        foreach (var polygonInfo in polygons)
        {
            // 各面の法線ベクトルとカメラの方向ベクトルとのなす角を計算し、なす角が120度から180度程度の範囲に入る面を絞り込む
            var degrees = cameraInfo.Direction.Degrees(polygonInfo.Plane.Normal);
            if (degrees >= 120 && degrees <= 180)
            {
                list.Add(polygonInfo);
            }
        }

        return list;
    }
}
