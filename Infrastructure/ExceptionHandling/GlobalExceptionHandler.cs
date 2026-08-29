using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Core.Interfaces;

namespace TaskApi.Infrastructure.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IEnumerable<IExceptionMapper> mappers,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Guard against headers already sent to client
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning(
                "Response has already started. Skipping GlobalExceptionHandler for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
            return false;
        }

        // 2. Extract W3C Trace ID or fallback to HttpContext identifier
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        // 3. Delegate to IExceptionMapper strategy or fallback to 500
        var mapper = mappers.FirstOrDefault(m => m.CanHandle(exception));
        var (statusCode, problemDetails) = mapper != null 
            ? mapper.Map(httpContext, exception, traceId)
            : FallbackServerError(httpContext, traceId);

        // 4. Differentiate logging severity (Errors for 5xx, Warnings for 4xx)
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception, 
                "Unhandled Exception ({StatusCode}) on {Method} {Path} | TraceId: {TraceId}",
                statusCode, 
                httpContext.Request.Method, 
                httpContext.Request.Path, 
                traceId);
        }
        else
        {
            logger.LogWarning(
                "Handled Exception ({StatusCode}) on {Method} {Path} | TraceId: {TraceId} | Message: {Message}",
                statusCode, 
                httpContext.Request.Method, 
                httpContext.Request.Path, 
                traceId, 
                exception.Message);
        }

        // 5. Write response payload safely
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, ProblemDetails Details) FallbackServerError(
        HttpContext context, 
        string traceId)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request. Please refer to the trace ID for support.",
            Instance = context.Request.Path
        };
        
        details.Extensions["traceId"] = traceId;
        
        return (StatusCodes.Status500InternalServerError, details);
    }
}