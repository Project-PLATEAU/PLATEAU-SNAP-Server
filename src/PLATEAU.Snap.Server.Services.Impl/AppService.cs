using PLATEAU.Snap.Models.Exceptions;
using PLATEAU.Snap.Models.Extensions.Numerics;
using PLATEAU.Snap.Models.Server;
using PLATEAU.Snap.Server.Geoid;
using PLATEAU.Snap.Server.Repositories;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace PLATEAU.Snap.Server.Services;

internal class AppService : IAppService
{
    private const int ThumbnailWidth = 300;

    private const int ThumbnailHeight = 300;

    private readonly ISurfaceGeometryRepository repository;

    private readonly ICityBoundaryRepository cityBoundaryRepository;

    private readonly IImageRepository imageRepository;

    private readonly Grid grid;

    public AppService(ISurfaceGeometryRepository repository, ICityBoundaryRepository cityBoundaryRepository, IImageRepository imageRepository, Grid grid)
    {
        this.repository = repository;
        this.cityBoundaryRepository = cityBoundaryRepository;
        this.imageRepository = imageRepository;
        this.grid = grid;
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

        // 撮影地点のジオイド高を取得
        var geoidHeight = this.grid.GetGeoidHeight(request.From.X, request.From.Y);

        var response = new Models.Client.VisibleSurfacesResponse();
        response.Surfaces.AddRange(facingPolygons.Select(p => new Models.Client.Surface(p.Gmlid, p.Polygon, geoidHeight)));

        return response;
    }

    public async Task<Models.Client.BuildingImageResponse> CreateBuildingImageAsync(BuildingImageRequest request)
    {
        try
        {
            using var stream = request.File.OpenReadStream();
            var thumbnailBytes = CreateThumbnailAsBytes(stream, ThumbnailWidth, ThumbnailHeight);

            var entity = await this.imageRepository.CreateAsync(new Entities.Models.Image(request.Metadata, thumbnailBytes), stream);
            return new Models.Client.BuildingImageResponse()
            {
                Status = Models.Client.StatusType.Success,
                Id = entity.Id
            };
        }
        catch (SnapServerException ex)
        {
            return new Models.Client.BuildingImageResponse()
            {
                Status = Models.Client.StatusType.Error,
                Message = "File upload failed. Please try again.",
                Exception = ex
            };
        }
    }

    private static byte[] CreateThumbnailAsBytes(Stream inputStream, int width, int height)
    {
        inputStream.Position = 0;

        using var image = SixLabors.ImageSharp.Image.Load(inputStream);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new SixLabors.ImageSharp.Size(width, height)
        }));

        using var ms = new MemoryStream();
        image.Save(ms, new JpegEncoder());

        inputStream.Position = 0;

        return ms.ToArray();
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
