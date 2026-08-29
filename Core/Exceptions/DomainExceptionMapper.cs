using Microsoft.AspNetCore.Mvc;
using TaskApi.Core.Interfaces;

namespace TaskApi.Core.Exceptions;

public class DomainExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is DomainException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        if (exception is not DomainException ex)
        {
            throw new ArgumentException($"Expected {nameof(DomainException)} but received {exception.GetType().Name}", nameof(exception));
        }

        var statusCode = ex.StatusCode > 0 ? ex.StatusCode : StatusCodes.Status400BadRequest;

        var details = new ProblemDetails
        {
            Status = statusCode,
            Type = ex.TypeUrl ?? $"https://datatracker.ietf.org/doc/html/rfc9110#section-15.5",
            Title = string.IsNullOrWhiteSpace(ex.Title) ? "Domain Rule Violation" : ex.Title,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;

        if (!string.IsNullOrWhiteSpace(ex.ErrorCode))
        {
            details.Extensions["errorCode"] = ex.ErrorCode;
        }

        return (statusCode, details);
    }
}