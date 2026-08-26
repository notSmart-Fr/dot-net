using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Common.Exceptions;

public class GlobalExceptionHandler(
    IEnumerable<IExceptionMapper> mappers,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly List<IExceptionMapper> _mappers = mappers.ToList();

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning("Response has already started. Skipping GlobalExceptionHandler for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
            return false;
        }

        httpContext.Response.ContentType = "application/problem+json";
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Find matching mapper strategy, or fallback to default 500 handler
        var mapper = _mappers.FirstOrDefault(m => m.CanHandle(exception));
        var (statusCode, problemDetails) = mapper != null 
            ? mapper.Map(httpContext, exception, traceId)
            : FallbackServerError(httpContext, exception, traceId);

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Unhandled Exception ({StatusCode}) on {Method} {Path} | TraceId: {TraceId}",
                statusCode, httpContext.Request.Method, httpContext.Request.Path, traceId);
        }
        else
        {
            logger.LogWarning("Handled Exception ({StatusCode}) on {Method} {Path} | TraceId: {TraceId} | Message: {Message}",
                statusCode, httpContext.Request.Method, httpContext.Request.Path, traceId, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int, ProblemDetails) FallbackServerError(HttpContext context, Exception exception, string traceId)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request. Use the provided traceId to contact support.",
            Instance = context.Request.Path
        };
        details.Extensions["traceId"] = traceId;
        return (StatusCodes.Status500InternalServerError, details);
    }
}