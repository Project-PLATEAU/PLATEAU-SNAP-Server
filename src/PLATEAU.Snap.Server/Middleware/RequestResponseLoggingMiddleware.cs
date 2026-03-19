using System.Text;

namespace PLATEAU.Snap.Server.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<RequestResponseLoggingMiddleware> logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestBody = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        logger.LogInformation("Request {method} {url}: {body}",
            context.Request.Method,
            context.Request.Path,
            requestBody);

        string responseText = string.Empty;
        if (context.Response.ContentType != "application/zip" && context.Response.ContentType != "application/octet-stream")
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            await responseBody.CopyToAsync(originalBodyStream);
        }
        else
        {
            responseText = "<binary data>";
        }

        logger.LogInformation("Response {statusCode}: {body}",
            context.Response.StatusCode,
            responseText);
    }
}
