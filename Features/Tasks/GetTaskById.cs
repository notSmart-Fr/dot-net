using Microsoft.EntityFrameworkCore;
using TaskApi.Features.Tasks.Exceptions;
using TaskApi.Shared.Database;
using TaskApi.Shared.Interfaces;

namespace TaskApi.Features.Tasks;

public static class GetTaskById
{
    // 1. DTO
    public record TaskDto(int Id, string Title, bool Done);

    // 2. HANDLER
    public class Handler(AppDbContext db)
    {
        public async Task<TaskDto> ExecuteAsync(int id, CancellationToken ct)
        {
            return await db.Tasks
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TaskDto(t.Id, t.Title, t.Done))
                .FirstOrDefaultAsync(ct) ?? throw new TaskNotFoundException(id);
        }
    }

    // 3. ENDPOINT (Implements IEndpoint for Assembly Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/tasks/{id:int}", HandleAsync)
               .WithName("GetTaskById")
               .WithTags("Tasks")
               .Produces<TaskDto>(StatusCodes.Status200OK)
               .ProducesProblem(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> HandleAsync(int id, Handler handler, CancellationToken ct)
        {
            var task = await handler.ExecuteAsync(id, ct);
            return TypedResults.Ok(task);
        }
    }
}