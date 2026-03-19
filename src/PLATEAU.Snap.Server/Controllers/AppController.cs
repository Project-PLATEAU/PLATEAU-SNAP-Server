using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Server.Filters;
using PLATEAU.Snap.Server.Services;
using Swashbuckle.AspNetCore.Annotations;
using static PLATEAU.Snap.Server.Constants;

namespace PLATEAU.Snap.Server.Controllers;

[Route(ApiRoute)]
[ApiController]
[Authorize]
[TypeFilter(typeof(ApiExceptionFilter))]
public class AppController : ControllerBase
{
    private readonly ILogger<AppController> logger;

    private readonly IAppService service;

    public AppController(ILogger<AppController> logger, IAppService service)
    {
        this.logger = logger;
        this.service = service;
    }

    [HttpPost]
    [Route("visible-surfaces")]
    [SwaggerOperation(
        Summary = "現在の位置で撮影可能な面の情報を取得します。",
        OperationId = nameof(GetVisibleSurfacesAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(VisibleSurfacesResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<VisibleSurfacesResponse>> GetVisibleSurfacesAsync(
        [FromBody, SwaggerParameter("取得する面を絞り込むためのパラメータ", Required = true)] VisibleSurfacesRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload}");
        return Ok(await service.GetVisibleSurfacesAsync(payload.ToServerParam()));
    }

    [HttpPost]
    [Route("building-image")]
    [SwaggerOperation(
        Summary = "撮影した建物面の画像を登録します。",
        OperationId = nameof(CreateBuildingImageAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(BuildingImageResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerResponseDescriptions.NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<VisibleSurfacesResponse>> CreateBuildingImageAsync(
        [FromForm, SwaggerParameter("建物面の画像を登録するためのパラメータ", Required = true)] BuildingImageRequest payload)
    {
        logger.LogInformation($"{DateTime.Now}: {payload.Metadata}");

        var result = await service.CreateBuildingImageAsync(payload.ToServerParam());
        if (result.Status == StatusType.Error)
        {
            logger.LogWarning(result.Exception, $"{DateTime.Now}: Failed to create building image");
        }

        return Ok(result);
    }
}
