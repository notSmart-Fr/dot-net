using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Common.Exceptions.Mappers;

public class DomainExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is DomainException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var ex = (DomainException)exception;
        var details = new ProblemDetails
        {
            Status = ex.StatusCode,
            Type = ex.TypeUrl ?? $"https://httpstatuses.com/{ex.StatusCode}",
            Title = ex.Title,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        if (ex.ErrorCode != null)
        {
            details.Extensions["errorCode"] = ex.ErrorCode;
        }

        return (ex.StatusCode, details);
    }
}