using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Server.Extensions.Mvc;
using PLATEAU.Snap.Server.Services;
using Swashbuckle.AspNetCore.Annotations;
using static PLATEAU.Snap.Server.Constants;

namespace PLATEAU.Snap.Server.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class SurfacesController : ControllerBase
{
    private readonly ILogger<SurfacesController> logger;

    private readonly ISurfaceGeometryService service;

    public SurfacesController(ILogger<SurfacesController> logger, ISurfaceGeometryService service)
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
        try
        {
            return Ok(await service.GetVisibleSurfacesAsync(payload.ToServerParam()));
        }
        catch (Exception ex)
        {
            return this.HandleException(ex);
        }
    }
}
