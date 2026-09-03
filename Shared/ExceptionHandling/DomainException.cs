namespace TaskApi.Shared.ExceptionHandling;
using Microsoft.AspNetCore.Http;
public abstract class DomainException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    string title = "Business Rule Violation",
    string? errorCode = null,
    string? typeUrl = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
    public string? ErrorCode { get; } = errorCode;
    public string? TypeUrl { get; } = typeUrl;
}