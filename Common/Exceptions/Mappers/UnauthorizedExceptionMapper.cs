using Microsoft.AspNetCore.Mvc;
using TaskApi.Core.Interfaces;

namespace TaskApi.Common.Exceptions.Mappers;

public class UnauthorizedExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is UnauthorizedAccessException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2",
            Title = "Unauthorized",
            // Use fallback text if exception.Message looks like a generic system error
            Detail = string.IsNullOrWhiteSpace(exception.Message) 
                ? "You are not authorized to access this resource." 
                : exception.Message,
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (StatusCodes.Status401Unauthorized, details);
    }
}