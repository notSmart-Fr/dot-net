using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Core.Interfaces;

public interface IExceptionMapper
{
    // Determines if this mapper can handle the thrown exception type
    bool CanHandle(Exception exception);

    // Maps the exception to an HTTP Status Code and ProblemDetails payload
    (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId);
}