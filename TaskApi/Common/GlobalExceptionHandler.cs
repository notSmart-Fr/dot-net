using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Handle Client-Side Payload Errors (Bad JSON syntax, missing body parameters)
        if (exception is BadHttpRequestException badRequestEx)
        {
            var firstLineReason = badRequestEx.InnerException?.Message ?? badRequestEx.Message;
            firstLineReason = firstLineReason.Split(["\r\n", "\n"], StringSplitOptions.None)[0];

            _logger.LogWarning("Bad Request on {Method} {Path} | Reason: {Reason}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                firstLineReason);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid JSON Payload",
                Detail = "The request payload contains invalid JSON syntax or missing required fields.",
                Instance = httpContext.Request.Path
            }, cancellationToken);

            return true; // Exception handled cleanly
        }

        // 2. Handle Unexpected Server Errors (Database drops, Null references, 500s)
        _logger.LogError(exception, "Unhandled Exception on {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request.",
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true; // Exception handled cleanly
    }
}