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
public class JobsController : ControllerBase
{
    private readonly ILogger<JobsController> logger;

    private readonly IJobService service;

    public JobsController(ILogger<JobsController> logger, IJobService service)
    {
        this.logger = logger;
        this.service = service;
    }

    [HttpGet]
    [Route("jobs/{job_id}")]
    [SwaggerOperation(
        Summary = "Jobを取得します。",
        OperationId = nameof(GetJobAsync)
    )]
    [SwaggerResponse(StatusCodes.Status200OK, SwaggerResponseDescriptions.Ok, typeof(Job))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerResponseDescriptions.BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, SwaggerResponseDescriptions.Unauthorized)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerResponseDescriptions.InternalServerError)]
    public async Task<ActionResult<Job>> GetJobAsync(
        [FromRoute, SwaggerParameter("Job ID")] int job_id)
    {
        logger.LogInformation($"{DateTime.Now}: {job_id}");
        return Ok(await service.GetByIdAsync(job_id));
    }
}
