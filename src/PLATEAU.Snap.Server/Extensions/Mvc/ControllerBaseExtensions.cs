using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PLATEAU.Snap.Server.Extensions.Mvc;

public static class ControllerBaseExtensions
{
    public static ActionResult HandleException(this ControllerBase controller, Exception ex)
    {
        switch (ex)
        {
            case ArgumentNullException:
            case InvalidCastException:
                return controller.CreateBadRequest(ex);
            default:
                return controller.CreateInternalServerError(ex);
        }
    }

    public static BadRequestObjectResult CreateBadRequest(this ControllerBase controller, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = ex.Message,
            Instance = controller.HttpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier;

        return controller.BadRequest(problem);
    }

    public static ActionResult CreateInternalServerError(this ControllerBase controller, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = ex.Message,
            Instance = controller.HttpContext.Request.Path
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier;

        return new ObjectResult(problem) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}
