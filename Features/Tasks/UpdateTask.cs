using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskApi.Common.Filters;
using TaskApi.Core.Exceptions;
using TaskApi.Core.Interfaces;
using TaskApi.Infrastructure.Database;

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
        public async Task<UpdateTaskResponse> ExecuteAsync(int id, UpdateTaskRequest request, CancellationToken ct)
        {
            // Business Rule: Check if the task exists before updating
            var taskEntity = await db.Tasks.FindAsync([id], ct) 
                ?? throw new TaskNotFoundException(id);

            taskEntity.Title = request.Title;
            taskEntity.Done = request.Done;

            await db.SaveChangesAsync(ct);

            return new UpdateTaskResponse(taskEntity.Id, taskEntity.Title, taskEntity.Done);
        }
    }

    // 4. ENDPOINT (Implements IEndpoint for Assembly Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/tasks/{id:int}", HandleAsync)
               .WithName("UpdateTask")
               .WithTags("Tasks")
               .AddEndpointFilter<ValidationFilter<UpdateTaskRequest>>()
               .Produces<UpdateTaskResponse>(StatusCodes.Status200OK)
               .ProducesValidationProblem(StatusCodes.Status400BadRequest)
               .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> HandleAsync(
            int id, 
            UpdateTaskRequest request, 
            Handler handler, 
            CancellationToken ct)
        {
            var response = await handler.ExecuteAsync(id, request, ct);
            return TypedResults.Ok(response);
        }
    }
}