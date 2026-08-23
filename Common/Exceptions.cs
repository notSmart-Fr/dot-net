namespace TaskApi.Common;

// 404 Not Found
public class TaskNotFoundException(int id) 
    : DomainException(
        message: $"Task with ID '{id}' was not found.", 
        statusCode: StatusCodes.Status404NotFound, 
        title: "Task Not Found",
        errorCode: "TASK_NOT_FOUND");

// 409 Conflict (Duplicate Entry)
public class DuplicateTaskException(string title) 
    : DomainException(
        message: $"A task titled '{title}' already exists.", 
        statusCode: StatusCodes.Status409Conflict, 
        title: "Duplicate Task Title",
        errorCode: "DUPLICATE_TASK_TITLE");