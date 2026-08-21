using Microsoft.EntityFrameworkCore;
using TaskApi.Common;

namespace TaskApi.Features.Tasks;

public static class GetTasks
{
    // 1. DTO
    public record TaskDto(int Id, string Title, bool Done);

    // 2. HANDLER: Fetch all tasks
    public class Handler(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        public async Task<List<TaskDto>> ExecuteAsync(CancellationToken ct)
        {
            return await _db.Tasks
                .AsNoTracking() // Performance boost for read-only queries
                .Select(t => new TaskDto(t.Id, t.Title, t.Done))
                .ToListAsync(ct);
        }
    }

    // 3. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", async (Handler handler, CancellationToken ct) =>
        {
            var tasks = await handler.ExecuteAsync(ct);
            return Results.Ok(tasks);
        })
        .WithName("GetTasks")
        .WithTags("Tasks");
    }
}