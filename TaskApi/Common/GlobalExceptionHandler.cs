using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Guard clause: If response headers were already sent to client, we cannot write a ProblemDetails payload
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning("Response has already started. Skipping GlobalExceptionHandler for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
            return false;
        }

        // Standardize RFC 7807 Content-Type
        httpContext.Response.ContentType = "application/problem+json";

        // Capture Distributed Tracing ID for log correlation
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Map exception to appropriate HTTP Status Code and ProblemDetails payload
        var (statusCode, problemDetails) = MapException(httpContext, exception, traceId);

        // Set response HTTP status code
        httpContext.Response.StatusCode = statusCode;

        // Write RFC 7807 JSON response
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception safely handled
    }

    private (int StatusCode, ProblemDetails Details) MapException(
        HttpContext httpContext, 
        Exception exception, 
        string traceId)
    {
        return exception switch
        {
            // 1. Client-Side Malformed Payload Errors (400)
            BadHttpRequestException badRequestEx => HandleBadRequest(httpContext, badRequestEx, traceId),

            // 2. Custom Domain / Business Rule Exceptions (e.g., 404 Not Found, 409 Conflict)
            DomainException domainEx => HandleDomainException(httpContext, domainEx, traceId),

            // 3. Unhandled Server Faults (500)
            _ => HandleUnhandledServerError(httpContext, exception, traceId)
        };
    }

    private (int, ProblemDetails) HandleBadRequest(
        HttpContext httpContext, 
        BadHttpRequestException ex, 
        string traceId)
    {
        var firstLineReason = ex.InnerException?.Message ?? ex.Message;
        firstLineReason = firstLineReason.Split(["\r\n", "\n"], StringSplitOptions.None)[0];

        // Structured Logging (Indexed parameters, no string interpolation)
        logger.LogWarning(ex, "Bad Request on {Method} {Path} | TraceId: {TraceId} | Reason: {Reason}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceId,
            firstLineReason);

        var details = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = "Invalid JSON Payload",
            Detail = "The request payload contains invalid JSON syntax or missing required fields.",
            Instance = httpContext.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (StatusCodes.Status400BadRequest, details);
    }

    private (int, ProblemDetails) HandleDomainException(
        HttpContext httpContext, 
        DomainException ex, 
        string traceId)
    {
        logger.LogWarning(ex, "Domain Exception ({StatusCode}) on {Method} {Path} | TraceId: {TraceId}",
            ex.StatusCode,
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceId);

        var details = new ProblemDetails
        {
            Status = ex.StatusCode,
            Type = ex.TypeUrl ?? $"https://httpstatuses.com/{ex.StatusCode}",
            Title = ex.Title,
            Detail = ex.Message,
            Instance = httpContext.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        if (ex.ErrorCode != null)
        {
            details.Extensions["errorCode"] = ex.ErrorCode;
        }

        return (ex.StatusCode, details);
    }

    private (int, ProblemDetails) HandleUnhandledServerError(
        HttpContext httpContext, 
        Exception ex, 
        string traceId)
    {
        // Full stack trace logged internally for developer debugging
        logger.LogError(ex, "Unhandled Exception on {Method} {Path} | TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceId);

        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request. Use the provided traceId to contact support.",
            Instance = httpContext.Request.Path
        };

        // Sanitized output for production security: Only return traceId to client
        details.Extensions["traceId"] = traceId;
        return (StatusCodes.Status500InternalServerError, details);
    }
}

// Base Custom Domain Exception for Business Rules
public abstract class DomainException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    string title = "Business Rule Violation",
    string? errorCode = null,
    string? typeUrl = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
    public string? ErrorCode { get; } = errorCode;
    public string? TypeUrl { get; } = typeUrl;
}