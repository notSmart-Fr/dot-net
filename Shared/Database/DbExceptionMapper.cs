using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskApi.Shared.Interfaces;

namespace TaskApi.Shared.Database;

public sealed class DbExceptionMapper : IExceptionMapper
{
    public bool CanHandle(Exception exception) => 
        exception is DbUpdateException or DbUpdateConcurrencyException;

    public (int StatusCode, ProblemDetails Details) Map(HttpContext context, Exception exception, string traceId)
    {
        // PostgreSQL error code 23505 represents unique_violation
        var isUniqueViolation = exception.InnerException is PostgresException postgresEx 
            && postgresEx.SqlState == PostgresErrorCodes.UniqueViolation;

        var isConflict = exception is DbUpdateConcurrencyException || isUniqueViolation;

        var statusCode = isConflict 
            ? StatusCodes.Status409Conflict 
            : StatusCodes.Status400BadRequest;

        var title = isConflict 
            ? "Database Conflict" 
            : "Database Update Error";

        var details = new ProblemDetails
        {
            Status = statusCode,
            Type = isConflict 
                ? "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10" // 409 Conflict
                : "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",  // 400 Bad Request
            Title = title,
            // Keep internal database schema details hidden from API consumers for security
            Detail = isConflict 
                ? "A record with conflicting unique values already exists." 
                : "An error occurred while attempting to persist database changes.",
            Instance = context.Request.Path
        };

        details.Extensions["traceId"] = traceId;
        return (statusCode, details);
    }
}