using TaskApi.Core.Exceptions;
using TaskApi.Core.Interfaces;
using TaskApi.Infrastructure.Database;

namespace TaskApi.Features.Tasks;

public static class DeleteTask
{
    // 1. HANDLER
    public class Handler(AppDbContext db)
    {
        public async Task ExecuteAsync(int id, CancellationToken ct)
        {
            var taskEntity = await db.Tasks.FindAsync([id], ct) 
                ?? throw new TaskNotFoundException(id);

            db.Tasks.Remove(taskEntity);
            await db.SaveChangesAsync(ct);
        }
    }

    // 2. ENDPOINT (Implements IEndpoint for Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/tasks/{id:int}", HandleAsync)
               .WithName("DeleteTask")
               .WithTags("Tasks")
               .Produces(StatusCodes.Status204NoContent)
               .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> HandleAsync(
            int id, 
            Handler handler, 
            CancellationToken ct)
        {
            await handler.ExecuteAsync(id, ct);
            return TypedResults.NoContent();
        }
    }
}