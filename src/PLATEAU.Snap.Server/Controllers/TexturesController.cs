using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Models.Common;
using PLATEAU.Snap.Server.Filters;
using PLATEAU.Snap.Server.Services;
using Swashbuckle.AspNetCore.Annotations;
using static PLATEAU.Snap.Server.Constants;

namespace PLATEAU.Snap.Server.Controllers;

[Route(ApiRoute)]
[ApiController]
[Authorize]
[TypeFilter(typeof(ApiExceptionFilter))]
public class TexturesController : ControllerBase
{
    private readonly ILogger<TexturesController> logger;

    private readonly ITextureService service;

    private readonly IImporterExporterService importerExporterService;

    public TexturesController(ILogger<TexturesController> logger, ITextureService service, IImporterExporterService importerExporterService)
    {
        this.logger = logger;
        this.service = service;
        this.importerExporterService = importerExporterService;
    }

    [HttpGet]
    [Route("buildings")]
    [SwaggerOperation(
        Summary = "テクスチャを更新できる建築物モデル情報を取得します。",
        OperationId = nameof(GetBuildingsAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(PageData<BuildingImage>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<PageData<BuildingImage>>> GetBuildingsAsync(
        [FromQuery, SwaggerParameter("ソート順")] SortType sort_type = SortType.id_asc,
        [FromQuery, SwaggerParameter("ページ番号")] int page_number = 1,
        [FromQuery, SwaggerParameter("ページサイズ")] int page_size = 10)
    {
        logger.LogInformation($"{DateTime.Now}: {sort_type}, {page_number}, {page_size}");
        return Ok(await service.GetBuildingsAsync(sort_type, page_number, page_size));
    }

    [HttpGet]
    [Route("buildings/{building_id}/mesh-code")]
    [SwaggerOperation(
        Summary = "指定したIDの建物が含まれる3次メッシュコードを取得します。",
        OperationId = nameof(GetMeshCodeAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(MeshCodeResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<MeshCodeResponse>> GetMeshCodeAsync(
        [FromRoute, SwaggerParameter("建築物モデルID")] int building_id)
    {
        logger.LogInformation($"{DateTime.Now}: {building_id}");
        return Ok(await service.GetMeshCodeAsync(building_id));
    }

    [HttpGet]
    [Route("faces/{building_id}")]
    [SwaggerOperation(
        Summary = "テクスチャを更新できる建築物モデルの面情報を取得します。",
        OperationId = nameof(GetFacesAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(PageData<ImageInfo>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<PageData<ImageInfo>>> GetFacesAsync(
        [FromRoute, SwaggerParameter("建築物モデルID")] int building_id,
        [FromQuery, SwaggerParameter("ソート順")] SortType sort_type = SortType.id_asc,
        [FromQuery, SwaggerParameter("ページ番号")] int page_number = 1,
        [FromQuery, SwaggerParameter("ページサイズ")] int page_size = 10)
    {
        logger.LogInformation($"{DateTime.Now}: {sort_type}, {page_number}, {page_size}");
        return Ok(await service.GetFacesAsync(building_id, sort_type, page_number, page_size));
    }

    [HttpGet]
    [Route("images/{building_id}/{face_id}")]
    [SwaggerOperation(
        Summary = "面に紐づけられた画像一覧を取得します。",
        OperationId = nameof(GetFaceImagesAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(PageData<ImageInfo>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<PageData<ImageInfo>>> GetFaceImagesAsync(
        [FromRoute, SwaggerParameter("建築物モデルID")] int building_id,
        [FromRoute, SwaggerParameter("面ID")] int face_id,
        [FromQuery, SwaggerParameter("ソート順")] SortType sort_type = SortType.id_asc,
        [FromQuery, SwaggerParameter("ページ番号")] int page_number = 1,
        [FromQuery, SwaggerParameter("ページサイズ")] int page_size = 10)
    {
        logger.LogInformation($"{DateTime.Now}: {sort_type}, {page_number}, {page_size}");
        return Ok(await service.GetFaceImagesAsync(building_id, face_id, sort_type, page_number, page_size));
    }

    [HttpPost]
    [Route("transform")]
    [SwaggerOperation(
        Summary = "指定された画像を正射変換します。",
        OperationId = nameof(TransformAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(TransformResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<TransformResponse>> TransformAsync(
        [FromBody, SwaggerParameter("正射変換するためのパラメータ", Required = true)] TransformRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");
        return Ok(await service.TransformAsync(payload));
    }

    [HttpPost]
    [Route("roof-extraction")]
    [SwaggerOperation(
        Summary = "PLATEAU-Ortho から屋根面画像を生成します。",
        OperationId = nameof(RoofExtractionAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(RoofExtractionResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<RoofExtractionResponse>> RoofExtractionAsync(
        [FromBody, SwaggerParameter("正射変換するためのパラメータ", Required = true)] RoofExtractionRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");
        return Ok(await service.RoofExtractionAsync(payload));
    }

    // スコープから外れていたためコメントアウト
    //[HttpPost]
    //[Route("textures/preview")]
    //[SwaggerOperation(
    //    Summary = "テクスチャを適用した gltf を取得します。",
    //    OperationId = nameof(PreviewTextureAsync)
    //)]
    //[SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(PreviewTextureResponse))]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    //[SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    //[SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    //[SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    //public async Task<ActionResult<PreviewTextureResponse>> PreviewTextureAsync(
    //    [FromBody, SwaggerParameter("正射変換するためのパラメータ", Required = true)] PreviewTextureRequest payload)
    //{
    //    logger.LogInformation($"{DateTime.Now}: {payload}");
    //    return Ok(await service.PreviewTextureRequest(payload));
    //}

    [HttpPut]
    [Route("textures")]
    [SwaggerOperation(
        Summary = "テクスチャを更新します。",
        OperationId = nameof(ApplyTextureAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult> ApplyTextureAsync(
        [FromBody, SwaggerParameter("正射変換するためのパラメータ", Required = true)] ApplyTextureRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");
        await service.ApplyTextureAsync(payload);
        return Ok();
    }

    [HttpPost]
    [Route("export")]
    [SwaggerOperation(
        Summary = "指定したIDの建物をCityGMLとしてエクスポートします。",
        OperationId = nameof(Export)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, null, "application/zip")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult> Export(
        [FromBody, SwaggerParameter("エクスポートするためのパラメータ", Required = true)] ExportRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");

        var stream = await importerExporterService.ExportAsync(payload.Id);
        return File(stream, "application/zip", payload.FileName ?? $"{payload.Id}.zip");
    }

    [HttpPost]
    [Route("export-mesh")]
    [SwaggerOperation(
        Summary = "指定したメッシュコードの建物をCityGMLとしてエクスポートします。",
        OperationId = nameof(ExportMesh)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, null, "application/zip")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult> ExportMesh(
        [FromBody, SwaggerParameter("エクスポートするためのパラメータ", Required = true)] ExportMeshRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");

        var stream = await importerExporterService.ExportAsync(payload.MeshCode);
        return File(stream, "application/zip", payload.FileName ?? $"{payload.MeshCode}.zip");
    }

    [HttpPost]
    [Route("export-async")]
    [SwaggerOperation(
        Summary = "指定したIDの建物をCityGMLとしてエクスポートするように要求します。",
        OperationId = nameof(ExportAsync)
    )]
    [SwaggerResponse(StatusCodes.Status202Accepted, SwaggerResponseDescriptions.Accepted, typeof(Job))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<Job>> ExportAsync(
        [FromBody, SwaggerParameter("エクスポートするためのパラメータ", Required = true)] ExportRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");

        return Accepted(await service.ExportAsync(payload.Id, payload.FileName));
    }

    [HttpPost]
    [Route("export-mesh-async")]
    [SwaggerOperation(
        Summary = "指定したメッシュコードの建物をCityGMLとしてエクスポートするように要求します。",
        OperationId = nameof(ExportMeshAsync)
    )]
    [SwaggerResponse(StatusCodes.Status202Accepted, SwaggerResponseDescriptions.Accepted, typeof(Job))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<Job>> ExportMeshAsync(
        [FromBody, SwaggerParameter("エクスポートするためのパラメータ", Required = true)] ExportMeshRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");

        return Accepted(await service.ExportAsync(payload.MeshCode, payload.FileName));
    }
}
