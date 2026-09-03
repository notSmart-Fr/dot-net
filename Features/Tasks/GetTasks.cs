using Microsoft.EntityFrameworkCore;
using TaskApi.Shared.Database;
using TaskApi.Shared.Interfaces;
using static TaskApi.Features.Tasks.CreateTask;

namespace TaskApi.Features.Tasks;

public static class GetTasks
{
    // 1. INPUT QUERY PARAMETERS
    public record Query(
        string? Search = null,
        bool? IsDone = null,
        int Page = 1,
        int PageSize = 10);

    // 2. OUTPUT PAGINATED RESPONSE
    public record PaginatedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize);

    // 3. HANDLER
    public class Handler(AppDbContext db)
    {
        public async Task<PaginatedResponse<TaskResponse>> ExecuteAsync(Query query, CancellationToken ct)
        {
            var dbQuery = db.Tasks.AsNoTracking();

            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize > 50 ? 50 : (query.PageSize < 1 ? 10 : query.PageSize);

            // Filter 1: Universal search across PostgreSQL via ILike
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchTerm = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(t => EF.Functions.ILike(t.Title, $"%{searchTerm}%"));
            }

            // Filter 2: Completion status
            if (query.IsDone.HasValue)
            {
                dbQuery = dbQuery.Where(t => t.Done == query.IsDone.Value);
            }

            int totalCount = await dbQuery.CountAsync(ct);

            var items = await dbQuery
                .OrderByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskResponse(t.Id, t.Title, t.Done))
                .ToListAsync(ct);

            return new PaginatedResponse<TaskResponse>(items, totalCount, page, pageSize);
        }
    }

    // 4. ENDPOINT (Implements IEndpoint for Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/tasks", HandleAsync)
               .WithName("GetTasks")
               .WithTags("Tasks")
               .Produces<PaginatedResponse<TaskResponse>>(StatusCodes.Status200OK);
        }

        private static async Task<IResult> HandleAsync(
            [AsParameters] Query query, 
            Handler handler, 
            CancellationToken ct)
        {
            var result = await handler.ExecuteAsync(query, ct);
            return TypedResults.Ok(result);
        }
    }
}