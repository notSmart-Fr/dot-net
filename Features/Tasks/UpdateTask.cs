using FluentValidation;
using TaskApi.Common;
using TaskApi.Common.Exceptions;
using TaskApi.Infrastructure;

namespace TaskApi.Features.Tasks;

public static class UpdateTask
{
    // 1. DTOs
    public record UpdateTaskRequest(string Title, bool Done);
    public record UpdateTaskResponse(int Id, string Title, bool Done);

    // 2. VALIDATOR
    public class Validator : AbstractValidator<UpdateTaskRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.");
        }
    }

    // 3. HANDLER
    public class Handler(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        public async Task<UpdateTaskResponse> ExecuteAsync(int id, UpdateTaskRequest request, CancellationToken ct)
        {
            // Business Rule: Check if the task exists before updating
            var taskEntity = await _db.Tasks.FindAsync([id], ct) ?? throw new TaskNotFoundException(id);

            taskEntity.Title = request.Title;
            taskEntity.Done = request.Done;

            await _db.SaveChangesAsync(ct);

            return new UpdateTaskResponse(taskEntity.Id, taskEntity.Title, taskEntity.Done);
        }
    }
        // 4. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
{
    app.MapPut("/tasks/{id:int}", async (int id, UpdateTaskRequest request, Handler handler, CancellationToken ct) =>
    {
        var response = await handler.ExecuteAsync(id, request, ct);
        return TypedResults.Ok(response);
    })
    .WithName("UpdateTask")
    .WithTags("Tasks")
    .AddEndpointFilter<ValidationFilter<UpdateTaskRequest>>()
    // Document error status codes for Swagger UI
    .ProducesValidationProblem(StatusCodes.Status400BadRequest) // FluentValidation
    .ProducesProblem(StatusCodes.Status404NotFound);            // TaskNotFoundException
}
}