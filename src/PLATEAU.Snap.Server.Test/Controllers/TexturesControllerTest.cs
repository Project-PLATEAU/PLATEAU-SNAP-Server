using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Server.Controllers;
using PLATEAU.Snap.Server.Test.Fixture;

namespace PLATEAU.Snap.Server.Test.Controllers;

public class TexturesControllerTest(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact(DisplayName = "建物一覧")]
    [Trait("Category", "Unit")]
    public async Task GetBuildings()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const SortType sortType = SortType.id_asc;
        const int pageNumber = 1;
        const int pageSize = 10;

        var actionResult = await controller.GetBuildingsAsync(sortType, pageNumber, pageSize);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact(DisplayName = "メッシュコード取得")]
    [Trait("Category", "Unit")]
    public async Task GetMeshCode()
    {
        var controller = fixture.GetRequiredService<TexturesController>();
        const int buildingId = 1;

        var actionResult = await controller.GetMeshCodeAsync(buildingId);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact(DisplayName = "面一覧")]
    [Trait("Category", "Unit")]
    public async Task GetFaces()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const int buildingId = 1;
        const SortType sortType = SortType.id_asc;
        const int pageNumber = 1;
        const int pageSize = 10;

        var actionResult = await controller.GetFacesAsync(buildingId, sortType, pageNumber, pageSize);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact(DisplayName = "画像一覧")]
    [Trait("Category", "Unit")]
    public async Task GetFaceImages()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const int buildingId = 1;
        const int faceId = 1;
        const SortType sortType = SortType.id_asc;
        const int pageNumber = 1;
        const int pageSize = 10;

        var actionResult = await controller.GetFaceImagesAsync(buildingId, faceId, sortType, pageNumber, pageSize);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact(DisplayName = "正射変換")]
    [Trait("Category", "Unit")]
    public async Task Transform()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const int buildingId = 1;
        const int faceId = 1;
        const int imageId = 1;
        var request = new TransformRequest()
        {
            BuildingId = buildingId,
            FaceId = faceId,
            ImageId = imageId
        };

        var actionResult = await controller.TransformAsync(request);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact(DisplayName = "屋根面生成")]
    [Trait("Category", "Unit")]
    public async Task RoofExtraction()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const int buildingId = 1;
        const int faceId = 1;
        var request = new RoofExtractionRequest
        {
            BuildingId = buildingId,
            FaceId = faceId,
        };

        var actionResult = await controller.RoofExtractionAsync(request);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact(DisplayName = "テクスチャ更新")]
    [Trait("Category", "Unit")]
    public async Task ApplyTexture()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const int buildingId = 1;
        const int faceId = 1;
        const string path = "s3://temp/transform.png";
        const string coordinates = "POLYGON((10 10, 10 20, 20 20, 20 10, 10 10))";
        var request = new ApplyTextureRequest()
        {
            BuildingId = buildingId,
            FaceId = faceId,
            Path = path,
            Coordinates = coordinates
        };

        var actionResult = await controller.ApplyTextureAsync(request);

        var statusCodeResult = actionResult as StatusCodeResult;
        Assert.NotNull(statusCodeResult);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);
    }

    [Fact(DisplayName = "CityGML出力")]
    [Trait("Category", "Unit")]
    public async Task Export()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const int id = 1;
        const string fileName = "citygml.zip";
        var request = new ExportRequest()
        {
            Id = id,
            FileName = fileName
        };

        var actionResult = await controller.ExportAsync(request);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status202Accepted, objectResult.StatusCode);
    }

    [Fact(DisplayName = "CityGML出力 (メッシュ)")]
    [Trait("Category", "Unit")]
    public async Task ExportMesh()
    {
        var controller = fixture.GetRequiredService<TexturesController>();

        const string meshCode = "53394611";
        const string fileName = "citygml.zip";
        var request = new ExportMeshRequest()
        {
            MeshCode = meshCode,
            FileName = fileName
        };

        var actionResult = await controller.ExportMeshAsync(request);

        var objectResult = actionResult.Result as IStatusCodeActionResult;
        Assert.NotNull(objectResult);
        Assert.Equal(StatusCodes.Status202Accepted, objectResult.StatusCode);
    }
}
