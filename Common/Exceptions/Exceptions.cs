namespace TaskApi.Common.Exceptions;

// 1. BASE DOMAIN EXCEPTION
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

// 2. TYPED DOMAIN EXCEPTION: 404 Not Found
public class TaskNotFoundException(int id) 
    : DomainException(
        message: $"Task with ID '{id}' was not found.", 
        statusCode: StatusCodes.Status404NotFound, 
        title: "Task Not Found",
        errorCode: "TASK_NOT_FOUND");

// 3. TYPED DOMAIN EXCEPTION: 409 Conflict
public class DuplicateTaskException(string title) 
    : DomainException(
        message: $"A task titled '{title}' already exists.", 
        statusCode: StatusCodes.Status409Conflict, 
        title: "Duplicate Task Title",
        errorCode: "DUPLICATE_TASK_TITLE");