using Microsoft.AspNetCore.Mvc;
using TaskApi.Core.Interfaces;

namespace TaskApi.Common.Exceptions.Mappers;

public sealed class BadRequestExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => 
        exception is BadHttpRequestException or FormatException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
            Title = "Bad Request",
            Detail = "The request payload was malformed or contained invalid data formats.",
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (StatusCodes.Status400BadRequest, details);
    }
}