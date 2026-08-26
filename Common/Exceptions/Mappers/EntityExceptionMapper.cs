using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TaskApi.Common.Exceptions.Mappers;

public class EntityExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => exception is DbUpdateException or DbUpdateConcurrencyException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        var isConflict = exception is DbUpdateConcurrencyException || 
                         (exception.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ?? false) ||
                         (exception.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ?? false);

        var statusCode = isConflict ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        var title = isConflict ? "Database Conflict" : "Database Update Error";

        var details = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            // Sanitize raw SQL messages so database schema details aren't exposed to the client
            Detail = isConflict 
                ? "A record with conflicting unique fields already exists." 
                : "An error occurred while saving changes to the database.",
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (statusCode, details);
    }
}