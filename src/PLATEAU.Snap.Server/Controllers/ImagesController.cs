using Microsoft.AspNetCore.Mvc;
using PLATEAU.Snap.Models.Client;
using PLATEAU.Snap.Server.Extensions.Mvc;
using PLATEAU.Snap.Server.Services;
using Swashbuckle.AspNetCore.Annotations;
using static PLATEAU.Snap.Server.Constants;

namespace PLATEAU.Snap.Server.Controllers;

[Route("api")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly ILogger<ImagesController> _logger;

    private readonly IImageService _service;

    public ImagesController(ILogger<ImagesController> logger, IImageService service)
    {
        _logger = logger;
        _service = service;
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
        try
        {
            return Ok(await _service.CreateBuildingImageAsync(payload.ToServerParam()));
        }
        catch (Exception ex)
        {
            return this.HandleException(ex);
        }
    }
}
