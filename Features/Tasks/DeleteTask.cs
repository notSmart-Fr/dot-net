using TaskApi.Common;
using TaskApi.Infrastructure;
namespace TaskApi.Features.Tasks;
public static class DeleteTask
{
    // 1. HANDLER: Delete a task by ID
    public class Handler(AppDbContext db)
    {
        private readonly AppDbContext _db = db;

        public async Task<bool> ExecuteAsync(int id, CancellationToken ct)
        {//Business Rule: Check if the task exists before deleting
            var taskEntity = await _db.Tasks.FindAsync([id], ct) ?? throw new TaskNotFoundException(id);
            _db.Tasks.Remove(taskEntity);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }

    // 2. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/tasks/{id:int}", async (int id, Handler handler, CancellationToken ct) =>
        {
            var deleted = await handler.ExecuteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteTask")
        .WithTags("Tasks")
        .ProducesProblem(StatusCodes.Status404NotFound); // Document error status codes for Swagger UI
        
    }
}