using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskApi.Core.Entities;
using TaskApi.Core.Exceptions;
using TaskApi.Core.Interfaces;
using TaskApi.Infrastructure.Database;
using TaskApi.Infrastructure.Filters;

namespace TaskApi.Features.Tasks;

public static class CreateTask
{
    // 1. DTOs
    public record CreateTaskRequest(string Title, bool Done = false);
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

    // 3. HANDLER
    public class Handler(AppDbContext db)
    {
        public async Task<TaskResponse> ExecuteAsync(CreateTaskRequest request, CancellationToken ct)
        {
            // Business Rule: Check for duplicate titles
            var existingTask = await db.Tasks
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

            db.Tasks.Add(taskEntity);
            await db.SaveChangesAsync(ct);

            return new TaskResponse(taskEntity.Id, taskEntity.Title, taskEntity.Done);
        }
    }

    // 4. ENDPOINT (Implements IEndpoint for Assembly Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/tasks", HandleAsync)
               .WithName("CreateTask")
               .WithTags("Tasks")
               .AddEndpointFilter<ValidationFilter<CreateTaskRequest>>()
               .Produces<TaskResponse>(StatusCodes.Status201Created)
               .ProducesValidationProblem(StatusCodes.Status400BadRequest)
               .ProducesProblem(StatusCodes.Status409Conflict);
        }

        private static async Task<IResult> HandleAsync(
            CreateTaskRequest request, 
            Handler handler, 
            CancellationToken ct)
        {
            var response = await handler.ExecuteAsync(request, ct);
            return TypedResults.Created($"/tasks/{response.Id}", response);
        }
    }
}