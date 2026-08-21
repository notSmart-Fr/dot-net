using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using TaskApi.Common;

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

        public async Task<UpdateTaskResponse?> ExecuteAsync(int id, UpdateTaskRequest request, CancellationToken ct)
        {
            var taskEntity = await _db.Tasks.FindAsync([id], ct);
            if (taskEntity is null) return null;

            taskEntity.Title = request.Title;
            taskEntity.Done = request.Done;

            await _db.SaveChangesAsync(ct);

            return new UpdateTaskResponse(taskEntity.Id, taskEntity.Title, taskEntity.Done);
        }
    }

    // 4. ROUTE MAPPER
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/tasks/{id:int}", async Task<Results<Ok<UpdateTaskResponse>, NotFound, ValidationProblem>> (
            int id,
            UpdateTaskRequest request,
            IValidator<UpdateTaskRequest> validator,
            Handler handler,
            CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            var response = await handler.ExecuteAsync(id, request, ct);
            if (response is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        })
        .WithName("UpdateTask")
        .WithTags("Tasks");
    }
}