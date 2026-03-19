using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Models.Exceptions;
using PLATEAU.Snap.Models.Settings;
using PLATEAU.Snap.Server.Entities.Models;
using PLATEAU.Snap.Server.Services;
using PLATEAU.Snap.Server.Test.Fakes.Repositories;
using PLATEAU.Snap.Server.Test.Fakes.Services;

namespace PLATEAU.Snap.Server.Test.Services;

public class TextureServiceTest
{
    private static readonly NetTopologySuite.Geometries.GeometryFactory geometryFactory = new NetTopologySuite.Geometries.GeometryFactory();

    private const int BuildingCount = 18;

    private const int FaceCountPerBuilding = 5;

    private const int RoofCountPerBuilding = 2;

    private const int ImagePerFace = 4;

    private const int SurfaceGeometryCount = 3;

    [Fact(DisplayName = "建物一覧")]
    [Trait("Category", "Unit")]
    public async Task ListAiModels()
    {
        var service = CreateService();

        const int page = 1;
        const int pageSize = 10;
        const int totalCount = 18;
        const int totalPage = 2;
        const int count = 10;

        var pageData = await service.GetBuildingsAsync(SortType.id_asc, page, pageSize);
        Assert.Equal(totalCount, pageData.TotalCount);
        Assert.Equal(pageSize, pageData.PageSize);
        Assert.Equal(totalPage, pageData.TotalPages);
        Assert.Equal(page, pageData.CurrentPage);
        Assert.True(pageData.HasNext);
        Assert.Equal(count, pageData.Values.Count);
    }

    [Fact(DisplayName = "建物一覧 2ページ目")]
    [Trait("Category", "Unit")]
    public async Task ListAiModelsPage2()
    {
        var service = CreateService();

        const int page = 2;
        const int pageSize = 10;
        const int totalCount = 18;
        const int totalPage = 2;
        const int count = 8;

        var pageData = await service.GetBuildingsAsync(SortType.id_asc, page, pageSize);
        Assert.Equal(totalCount, pageData.TotalCount);
        Assert.Equal(pageSize, pageData.PageSize);
        Assert.Equal(totalPage, pageData.TotalPages);
        Assert.Equal(page, pageData.CurrentPage);
        Assert.False(pageData.HasNext);
        Assert.Equal(count, pageData.Values.Count);
    }

    [Fact(DisplayName = "建物一覧 ページサイズ")]
    [Trait("Category", "Unit")]
    public async Task ListAiModelsPageSize()
    {
        var service = CreateService();

        const int page = 2;
        const int pageSize = 5;
        const int totalCount = 18;
        const int totalPage = 4;
        const int count = 5;

        var pageData = await service.GetBuildingsAsync(SortType.id_asc, page, pageSize);
        Assert.Equal(totalCount, pageData.TotalCount);
        Assert.Equal(pageSize, pageData.PageSize);
        Assert.Equal(totalPage, pageData.TotalPages);
        Assert.Equal(page, pageData.CurrentPage);
        Assert.True(pageData.HasNext);
        Assert.Equal(count, pageData.Values.Count);
    }

    [Fact(DisplayName = "建物一覧 存在しないページ")]
    [Trait("Category", "Unit")]
    public async Task ListAiModelsOverPage()
    {
        var service = CreateService();

        const int page = 100;
        const int pageSize = 10;
        const int totalCount = 18;
        const int totalPage = 2;

        var pageData = await service.GetBuildingsAsync(SortType.id_asc, page, pageSize);
        Assert.Equal(totalCount, pageData.TotalCount);
        Assert.Equal(pageSize, pageData.PageSize);
        Assert.Equal(totalPage, pageData.TotalPages);
        Assert.Equal(page, pageData.CurrentPage);
        Assert.False(pageData.HasNext);
        Assert.Empty(pageData.Values);
    }

    [Fact(DisplayName = "メッシュコード取得")]
    [Trait("Category", "Unit")]
    public async Task GetMeshCode()
    {
        var service = CreateService();

        const int buildingId = 1;

        var response = await service.GetMeshCodeAsync(buildingId);
        Assert.NotEmpty(response.MeshCode);
    }

    [Fact(DisplayName = "メッシュコード取得 指定されたIDが存在しない")]
    [Trait("Category", "Unit")]
    public async Task GetMeshCodeIdNotExists()
    {
        var surfaceGeometryRepository = new FakeSurfaceGeometryRepository();
        surfaceGeometryRepository.IsRoofprintNull = true;
        var service = CreateService(surfaceGeometryRepository);

        const int buildingId = 1000;

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.GetMeshCodeAsync(buildingId);
        });
        Assert.Equal(typeof(NotFoundException), exception.GetType());
    }

    [Fact(DisplayName = "面一覧")]
    [Trait("Category", "Unit")]
    public async Task GetFaces()
    {
        var service = CreateService();

        const int buildingId = 1;
        const int page = 1;
        const int pageSize = 10;
        const int totalCount = 7;
        const int totalPage = 1;
        const int count = 7;

        var pageData = await service.GetFacesAsync(buildingId, SortType.id_asc, page, pageSize);
        Assert.Equal(totalCount, pageData.TotalCount);
        Assert.Equal(pageSize, pageData.PageSize);
        Assert.Equal(totalPage, pageData.TotalPages);
        Assert.Equal(page, pageData.CurrentPage);
        Assert.False(pageData.HasNext);
        Assert.Equal(count, pageData.Values.Count);
    }

    [Fact(DisplayName = "面一覧 指定されたIDが存在しない")]
    [Trait("Category", "Unit")]
    public async Task GetFacesIdNotExists()
    {
        var service = CreateService();

        const int buildingId = 1000;
        const int page = 1;
        const int pageSize = 10;

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.GetFacesAsync(buildingId, SortType.id_asc, page, pageSize);
        });
        Assert.Equal(typeof(NotFoundException), exception.GetType());
    }

    [Fact(DisplayName = "画像一覧")]
    [Trait("Category", "Unit")]
    public async Task GetFaceImages()
    {
        var service = CreateService();

        const int buildingId = 1;
        const int faceId = 1;
        const int page = 1;
        const int pageSize = 10;
        const int totalCount = 4;
        const int totalPage = 1;
        const int count = 4;

        var pageData = await service.GetFaceImagesAsync(buildingId, faceId, SortType.id_asc, page, pageSize);
        Assert.Equal(totalCount, pageData.TotalCount);
        Assert.Equal(pageSize, pageData.PageSize);
        Assert.Equal(totalPage, pageData.TotalPages);
        Assert.Equal(page, pageData.CurrentPage);
        Assert.False(pageData.HasNext);
        Assert.Equal(count, pageData.Values.Count);
    }

    [Fact(DisplayName = "画像一覧 指定されたIDが存在しない")]
    [Trait("Category", "Unit")]
    public async Task GetFaceImagesIdNotExists()
    {
        var service = CreateService();

        const int buildingId = 1000;
        const int faceId = 1000;
        const int page = 1;
        const int pageSize = 10;

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.GetFaceImagesAsync(buildingId, faceId, SortType.id_asc, page, pageSize);
        });
        Assert.Equal(typeof(NotFoundException), exception.GetType());
    }

    [Fact(DisplayName = "正射変換")]
    [Trait("Category", "Unit")]
    public async Task Transform()
    {
        var service = CreateService();

        const int buildingId = 1;
        const int faceId = 1;
        const int imageId = 3;

        var request = new TransformRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            ImageId = imageId,
        };

        var response = await service.TransformAsync(request);
        Assert.NotNull(response.Path);
        Assert.NotNull(response.Coordinates);
        Assert.NotNull(response.Uri);
    }

    [Fact(DisplayName = "正射変換 指定されたIDが存在しない")]
    [Trait("Category", "Unit")]
    public async Task TransformIdNotExists()
    {
        var service = CreateService();

        const int buildingId = 1000;
        const int faceId = 1000;
        const int imageId = 1000;

        var request = new TransformRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            ImageId = imageId,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(NotFoundException), exception.GetType());
    }

    [Fact(DisplayName = "正射変換 WKT取得失敗")]
    [Trait("Category", "Unit")]
    public async Task TransformWktFail()
    {
        var surfaceGeometryRepository = new FakeSurfaceGeometryRepository();
        surfaceGeometryRepository.IsFaceWktNull = true;
        var service = CreateService(surfaceGeometryRepository);

        const int buildingId = 1;
        const int faceId = 1;
        const int imageId = 3;

        var request = new TransformRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            ImageId = imageId,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.TransformAsync(request);
        });
        Assert.Equal(typeof(InvalidOperationException), exception.GetType());
    }

    [Fact(DisplayName = "屋根面生成")]
    [Trait("Category", "Unit")]
    public async Task RoofExtraction()
    {
        var service = CreateService();

        const int buildingId = 1;
        const int faceId = 6;

        var request = new RoofExtractionRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
        };

        var response = await service.RoofExtractionAsync(request);
        Assert.NotNull(response.Path);
        Assert.NotNull(response.Coordinates);
        Assert.NotNull(response.Uri);
    }

    [Fact(DisplayName = "屋根面生成 指定されたIDが存在しない")]
    [Trait("Category", "Unit")]
    public async Task RoofExtractionIdNotExists()
    {
        var service = CreateService();

        const int buildingId = 1000;
        const int faceId = 1000;

        var request = new RoofExtractionRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.RoofExtractionAsync(request);
        });
        Assert.Equal(typeof(NotFoundException), exception.GetType());
    }

    [Fact(DisplayName = "テクスチャ更新")]
    [Trait("Category", "Unit")]
    public async Task ApplyTexture()
    {
        var service = CreateService();

        const int buildingId = 1;
        const int faceId = 1;
        const string path = "s3://temp/transform.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new ApplyTextureRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            Path = path,
            Coordinates = coordinates,
        };

        await service.ApplyTextureAsync(request);
    }

    [Fact(DisplayName = "テクスチャ更新 紐づくSurfaceDataが複数")]
    [Trait("Category", "Unit")]
    public async Task ApplyTextureRelationSurfaceDataCount2()
    {
        var imageRepository = new FakeImageRepository();
        imageRepository.RelationSurfaceDataCount = 2;
        var service = CreateService(imageRepository);

        const int buildingId = 1;
        const int faceId = 1;
        const string path = "s3://temp/transform.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new ApplyTextureRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            Path = path,
            Coordinates = coordinates,
        };

        await service.ApplyTextureAsync(request);
    }

    [Fact(DisplayName = "テクスチャ更新 紐づくTextureparamが存在しない")]
    [Trait("Category", "Unit")]
    public async Task ApplyTextureTextureparamNull()
    {
        var imageRepository = new FakeImageRepository();
        imageRepository.IsTextureparamNull = true;
        var service = CreateService(imageRepository);

        const int buildingId = 1;
        const int faceId = 1;
        const string path = "s3://temp/transform.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new ApplyTextureRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            Path = path,
            Coordinates = coordinates,
        };

        await service.ApplyTextureAsync(request);
    }

    [Fact(DisplayName = "テクスチャ更新 指定されたIDが存在しない")]
    [Trait("Category", "Unit")]
    public async Task ApplyTextureIdNotExists()
    {
        var imageRepository = new FakeImageRepository();
        imageRepository.HasFace = false;
        var service = CreateService(imageRepository);

        const int buildingId = 1000;
        const int faceId = 1000;
        const string path = "s3://temp/transform.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new ApplyTextureRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            Path = path,
            Coordinates = coordinates,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.ApplyTextureAsync(request);
        });
        Assert.Equal(typeof(NotFoundException), exception.GetType());
    }

    [Fact(DisplayName = "テクスチャ更新 TexImageがnull")]
    [Trait("Category", "Unit")]
    public async Task ApplyTextureTexImageNull()
    {
        var imageRepository = new FakeImageRepository();
        imageRepository.IsTexImageNull = true;
        var service = CreateService(imageRepository);

        const int buildingId = 1;
        const int faceId = 1;
        const string path = "s3://temp/transform.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";

        var request = new ApplyTextureRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
            Path = path,
            Coordinates = coordinates,
        };

        var exception = await Record.ExceptionAsync(async () =>
        {
            await service.ApplyTextureAsync(request);
        });
        Assert.Equal(typeof(InvalidOperationException), exception.GetType());
    }

    private static TextureService CreateService()
    {
        var imageRepository = new FakeImageRepository();
        var jobRepository = new FakeJobRepository();
        var surfaceGeometryRepository = new FakeSurfaceGeometryRepository();
        var invokerService = new FakeInvokerService();
        var appSettings = new AppSettings();
        var databaseSettings = new DatabaseSettings();

        SeedFakeData(imageRepository);
        SeedFakeData(surfaceGeometryRepository);

        return new TextureService(imageRepository, jobRepository, invokerService, surfaceGeometryRepository, appSettings, databaseSettings);
    }

    private static TextureService CreateService(FakeImageRepository imageRepository)
    {
        var jobRepository = new FakeJobRepository();
        var surfaceGeometryRepository = new FakeSurfaceGeometryRepository();
        var invokerService = new FakeInvokerService();
        var appSettings = new AppSettings();
        var databaseSettings = new DatabaseSettings();

        SeedFakeData(imageRepository);
        SeedFakeData(surfaceGeometryRepository);

        return new TextureService(imageRepository, jobRepository, invokerService, surfaceGeometryRepository, appSettings, databaseSettings);
    }

    private static TextureService CreateService(FakeSurfaceGeometryRepository surfaceGeometryRepository)
    {
        var imageRepository = new FakeImageRepository();
        var jobRepository = new FakeJobRepository();
        var invokerService = new FakeInvokerService();
        var appSettings = new AppSettings();
        var databaseSettings = new DatabaseSettings();

        SeedFakeData(imageRepository);
        SeedFakeData(surfaceGeometryRepository);

        return new TextureService(imageRepository, jobRepository, invokerService, surfaceGeometryRepository, appSettings, databaseSettings);
    }

    private static void SeedFakeData(FakeImageRepository repository)
    {
        var wktWriter = new NetTopologySuite.IO.WKTWriter();
        for (var surfaceGeometryId = 1; surfaceGeometryId <= SurfaceGeometryCount; surfaceGeometryId++)
        {
            var textureparam = new Textureparam
            {
                SurfaceGeometryId = surfaceGeometryId,
                IsTextureParametrization = 1,
                TextureCoordinates = geometryFactory.CreatePolygon(
                [
                    new NetTopologySuite.Geometries.Coordinate(10, 10),
                    new NetTopologySuite.Geometries.Coordinate(10, 20),
                    new NetTopologySuite.Geometries.Coordinate(20, 20),
                    new NetTopologySuite.Geometries.Coordinate(20, 10),
                    new NetTopologySuite.Geometries.Coordinate(10, 10),
                ]),
                SurfaceData = new SurfaceDatum
                {
                    TexImage = new TexImage()
                    {
                    }
                },
            };
            repository.Textureparams.Add(textureparam);
        }
    }

    private static void SeedFakeData(FakeSurfaceGeometryRepository repository)
    {
        var faceId = 1;
        var imageId = 1;
        for (var buildingId = 1; buildingId <= BuildingCount; buildingId++)
        {
            var endFaceId = faceId + FaceCountPerBuilding;
            for (; faceId < endFaceId; faceId++)
            {
                var endImageId = imageId + ImagePerFace;
                for (; imageId < endImageId; imageId++)
                {
                    var face = new BuildingFace
                    {
                        BuildingId = buildingId,
                        FaceId = faceId,
                        ImageId = imageId,
                        Gmlid = $"gmlid_{buildingId}_{faceId}",
                        IsOrtho = false,
                        Thumbnail = new byte[] { 0x01, 0x02, 0x03 },
                        Coordinates = geometryFactory.CreatePolygon(
                        [
                            new NetTopologySuite.Geometries.Coordinate(139.767052, 35.681167),
                            new NetTopologySuite.Geometries.Coordinate(139.767052, 35.681267),
                            new NetTopologySuite.Geometries.Coordinate(139.767152, 35.681267),
                            new NetTopologySuite.Geometries.Coordinate(139.767152, 35.681167),
                            new NetTopologySuite.Geometries.Coordinate(139.767052, 35.681167),
                        ]),
                        Timestamp = DateTime.UtcNow,
                    };
                    repository.BuildingFaces.Add(face);
                }
            }

            var endRoofId = faceId + RoofCountPerBuilding;
            for (; faceId < endRoofId; faceId++)
            {
                var roof = new BuildingFace
                {
                    BuildingId = buildingId,
                    FaceId = faceId,
                    Gmlid = $"roof_gmlid_{buildingId}_{faceId}",
                    IsOrtho = true,
                    Thumbnail = new byte[] { 0x04, 0x05, 0x06 },
                    Coordinates = geometryFactory.CreatePolygon(
                    [
                        new NetTopologySuite.Geometries.Coordinate(139.767052, 35.681167),
                        new NetTopologySuite.Geometries.Coordinate(139.767052, 35.681267),
                        new NetTopologySuite.Geometries.Coordinate(139.767152, 35.681267),
                        new NetTopologySuite.Geometries.Coordinate(139.767152, 35.681167),
                        new NetTopologySuite.Geometries.Coordinate(139.767052, 35.681167),
                    ]),
                    Timestamp = DateTime.UtcNow,
                };
                repository.BuildingFaces.Add(roof);
            }
        }
    }
}
