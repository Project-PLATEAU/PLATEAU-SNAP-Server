using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PLATEAU.Snap.Models.Exceptions;
using System.Diagnostics;

namespace PLATEAU.Snap.Server.Filters;

public class ApiExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilter> logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        this.logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        var methodName = context.ActionDescriptor.DisplayName;

        logger.LogError(ex, $"Failed to execute {methodName}");

        context.Result = HandleException(context.HttpContext, ex);

        context.ExceptionHandled = true;
    }

    public static ActionResult HandleException(HttpContext httpContext, Exception ex)
    {
        switch (ex)
        {
            case ArgumentException:
            case InvalidCastException:
            case InvalidOperationException:
                return CreateBadRequest(httpContext, ex);
            case NotFoundException:
                return CreateNotFound(httpContext, ex);
            case TaskCanceledException:
                return CreateClientClosedRequest(httpContext, ex);
            case LambdaOperationException:
                return CreateInternalServerErrorForLambda(httpContext, ex);
            default:
                return CreateInternalServerError(httpContext, ex);
        }
    }

    public static BadRequestObjectResult CreateBadRequest(HttpContext httpContext, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = ex.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem);
    }

    public static UnauthorizedObjectResult CreateUnauthorized(HttpContext httpContext, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = ex.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new UnauthorizedObjectResult(problem);
    }

    public static NotFoundObjectResult CreateNotFound(HttpContext httpContext, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = ex.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new NotFoundObjectResult(problem);
    }

    public static ConflictObjectResult CreateConflict(HttpContext httpContext, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Title = "Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = ex.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new ConflictObjectResult(problem);
    }

    public static ActionResult CreateClientClosedRequest(HttpContext httpContext, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://http-statuscode.com/ja/code/4XX/499-unofficial",
            Title = "Client Closed Request",
            Status = StatusCodes.Status499ClientClosedRequest,
            Detail = ex.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new ObjectResult(problem) { StatusCode = StatusCodes.Status499ClientClosedRequest };
    }

    public static ActionResult CreateInternalServerErrorForLambda(HttpContext httpContext, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error (Lambda)",
            Status = StatusCodes.Status500InternalServerError,
            Detail = ex.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new ObjectResult(problem) { StatusCode = StatusCodes.Status500InternalServerError };
    }

    public static ActionResult CreateInternalServerError(HttpContext httpContext, Exception ex)
    {
        var problem = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = ex.Message,
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        return new ObjectResult(problem) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}
