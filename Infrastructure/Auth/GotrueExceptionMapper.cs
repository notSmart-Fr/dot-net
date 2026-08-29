using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Supabase.Gotrue.Exceptions;
using TaskApi.Core.Interfaces;

namespace TaskApi.Infrastructure.Auth;

public sealed class GotrueExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is GotrueException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var gotrueEx = (GotrueException)exception;

        // Determine HTTP status based on Supabase error response or default to 400 Bad Request
        var statusCode = gotrueEx.StatusCode switch
        {
            400 => StatusCodes.Status400BadRequest,
            401 => StatusCodes.Status401Unauthorized,
            403 => StatusCodes.Status403Forbidden,
            404 => StatusCodes.Status404NotFound,
            422 => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        var details = new ProblemDetails
        {
            Status = statusCode,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
            Title = "Authentication Error",
            Detail = gotrueEx.Message,
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (statusCode, details);
    }
}