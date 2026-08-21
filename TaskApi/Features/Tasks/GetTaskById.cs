using TaskApi.Common;
using Microsoft.EntityFrameworkCore;
namespace TaskApi.Features.Tasks;
public static class GetTaskById
{
    // 1. DTO
    public record TaskDto(int Id, string Title, bool Done);

    // 2. HANDLER: Fetch a task by ID
    public class Handler(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        public async Task<TaskDto?> ExecuteAsync(int id, CancellationToken ct)
        {
            var taskEntity = await _db.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (taskEntity is null)
            {
                return null;
            }

            return new TaskDto(taskEntity.Id, taskEntity.Title, taskEntity.Done);
        }
    }

    // 3. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks/{id:int}", async (int id, Handler handler, CancellationToken ct) =>
        {
            var task = await handler.ExecuteAsync(id, ct);
            return task is not null ? Results.Ok(task) : Results.NotFound();
        })
        .WithName("GetTaskById")
        .WithTags("Tasks");
    }
}