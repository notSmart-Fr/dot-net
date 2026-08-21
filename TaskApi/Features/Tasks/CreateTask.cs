using FluentValidation;
using TaskApi.Common;

namespace TaskApi.Features.Tasks;

public static class CreateTask
{
    // 1. DTOs
    public record CreateTaskRequest(string Title, bool Done = false);
    public record CreateTaskResponse(int Id, string Title, bool Done);

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

        public async Task<CreateTaskResponse> ExecuteAsync(CreateTaskRequest request, CancellationToken ct)
        {
            var taskEntity = new TaskEntity
            {
                Title = request.Title,
                Done = request.Done
            };

            _db.Tasks.Add(taskEntity);
            await _db.SaveChangesAsync(ct);

            return new CreateTaskResponse(taskEntity.Id, taskEntity.Title, taskEntity.Done);
        }
    }

    // 4. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/tasks", async (CreateTaskRequest request, Handler handler, CancellationToken ct) =>
        {
            var response = await handler.ExecuteAsync(request, ct);
            return Results.Created($"/tasks/{response.Id}", response);
        })
        .WithName("CreateTask")
        .WithTags("Tasks")
        .AddEndpointFilter<ValidationFilter<CreateTaskRequest>>();
    }
}