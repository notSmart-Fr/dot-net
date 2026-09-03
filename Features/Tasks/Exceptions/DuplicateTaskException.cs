// DuplicateTaskException represents an exception thrown when a task with the same identifier already exists in the system.
using TaskApi.Shared.ExceptionHandling;

namespace TaskApi.Features.Tasks.Exceptions;
public class DuplicateTaskException(object id) 
    : DomainException(
        message: $"Task with ID '{id}' already exists.", 
        statusCode: StatusCodes.Status409Conflict, 
        title: "Duplicate Task",
        errorCode: "DUPLICATE_TASK");