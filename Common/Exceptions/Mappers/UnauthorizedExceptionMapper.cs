using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Common.Exceptions.Mappers;

public class UnauthorizedExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is UnauthorizedAccessException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            Title = "Unauthorized",
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (StatusCodes.Status401Unauthorized, details);
    }
}