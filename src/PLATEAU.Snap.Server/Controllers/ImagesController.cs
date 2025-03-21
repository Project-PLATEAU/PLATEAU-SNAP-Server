﻿using Microsoft.AspNetCore.Authorization;
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
public class ImagesController : ControllerBase
{
    private readonly ILogger<ImagesController> logger;

    private readonly IImageService service;

    public ImagesController(ILogger<ImagesController> logger, IImageService service)
    {
        this.logger = logger;
        this.service = service;
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
            logger.LogInformation($"{DateTime.Now}: {payload.Metadata}");

            var result = await service.CreateBuildingImageAsync(payload.ToServerParam());
            if (result.Status == StatusType.Error)
            {
                logger.LogWarning(result.Exception, $"{DateTime.Now}: Failed to create building image");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{DateTime.Now}: Failed to create building image");
            return this.HandleException(ex);
        }
    }
}
