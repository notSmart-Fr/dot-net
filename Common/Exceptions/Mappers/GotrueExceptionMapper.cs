using Microsoft.AspNetCore.Mvc;
using Supabase.Gotrue.Exceptions;

namespace TaskApi.Common.Exceptions.Mappers;

public class GotrueExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is GotrueException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var ex = (GotrueException)exception;
        var isInvalidCredentials = ex.Message.Contains("Invalid login credentials", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("invalid_grant", StringComparison.OrdinalIgnoreCase);

        var statusCode = isInvalidCredentials ? StatusCodes.Status401Unauthorized : StatusCodes.Status400BadRequest;
        var rfcType = isInvalidCredentials 
            ? "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1"
            : "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";

        var details = new ProblemDetails
        {
            Status = statusCode,
            Type = rfcType,
            Title = isInvalidCredentials ? "Authentication Failed" : "Auth Request Error",
            Detail = isInvalidCredentials ? "Invalid login credentials" : ex.Message,
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (statusCode, details);
    }
}