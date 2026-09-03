using TaskApi.Shared.ExceptionHandling;

namespace TaskApi.Features.Tasks.Exceptions;

public class TaskNotFoundException(object id) 
    : DomainException(
        message: $"Task with ID '{id}' was not found.", 
        statusCode: StatusCodes.Status404NotFound, 
        title: "Task Not Found",
        errorCode: "TASK_NOT_FOUND");