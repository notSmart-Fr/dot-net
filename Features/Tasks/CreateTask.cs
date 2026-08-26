using FluentValidation;
using TaskApi.Common;
using TaskApi.Domain;
using TaskApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TaskApi.Common.Exceptions;

namespace TaskApi.Features.Tasks;

public static class CreateTask
{
    // 1. DTOs
    public record CreateTaskRequest(string Title, bool Done = false);
    // Response DTO
    public record TaskResponse(int Id, string Title, bool Done);

    // 2. VALIDATOR
    public class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100);
        }
    }

    // 3. HANDLER (Class inside the same file for clean separation of DB logic)
    public class Handler(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        public async Task<TaskResponse> ExecuteAsync(CreateTaskRequest request, CancellationToken ct)
        {
            // Business Rule: Check for duplicate titles
            var existingTask = await _db.Tasks
                .AnyAsync(t => t.Title == request.Title, ct);
            if (existingTask)
            {
                throw new DuplicateTaskException(request.Title);
            }
            
            var taskEntity = new TaskEntity
            {
                Title = request.Title,
                Done = request.Done
            };

            _db.Tasks.Add(taskEntity);
            await _db.SaveChangesAsync(ct);

            return new TaskResponse(taskEntity.Id, taskEntity.Title, taskEntity.Done);
        }
    }

    // 4. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
    {   // POST /tasks 
        app.MapPost("/tasks", async (CreateTaskRequest request, Handler handler, CancellationToken ct) =>
        {
            var response = await handler.ExecuteAsync(request, ct);
            // Return 201 Created with the new task's location
            return TypedResults.Created($"/tasks/{response.Id}", response);
        })
        .WithName("CreateTask")
        .WithTags("Tasks")
        .AddEndpointFilter<ValidationFilter<CreateTaskRequest>>()
        .ProducesValidationProblem(StatusCodes.Status400BadRequest) // FluentValidation
        .ProducesProblem(StatusCodes.Status409Conflict);            // DuplicateTaskException
    }
}