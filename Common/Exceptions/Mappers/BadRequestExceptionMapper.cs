using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Common.Exceptions.Mappers;

public class BadRequestExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is BadHttpRequestException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var ex = (BadHttpRequestException)exception;
        var firstLineReason = ex.InnerException?.Message ?? ex.Message;
        _ = firstLineReason.Split(["\r\n", "\n"], StringSplitOptions.None)[0];

        var details = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = "Invalid JSON Payload",
            Detail = "The request payload contains invalid JSON syntax or missing required fields.",
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (StatusCodes.Status400BadRequest, details);
    }
}