using Microsoft.EntityFrameworkCore;
using TaskApi.Infrastructure;
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
        private readonly AppDbContext _db = db;

        public async Task<PaginatedResponse<TaskResponse>> ExecuteAsync(Query query, CancellationToken ct)
        {
            var dbQuery = _db.Tasks.AsNoTracking();

            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize > 50 ? 50 : (query.PageSize < 1 ? 10 : query.PageSize);

            // Filter 1: Universal search across all DBs
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchTerm = query.Search.Trim().ToLower();
                #pragma warning disable CA1862 // EF Core LINQ requires .ToLower() for SQL translation
                dbQuery = dbQuery.Where(t => t.Title.ToLower().Contains(searchTerm));
                #pragma warning restore CA1862
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

            // ALWAYS RETURN AT THE END OF THE METHOD
            return new PaginatedResponse<TaskResponse>(items, totalCount, page, pageSize);
        }
    }

    // 4. ROUTE MAPPER
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", async ([AsParameters] Query query, Handler handler, CancellationToken ct) =>
        {
            var result = await handler.ExecuteAsync(query, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetTasks")
        .WithTags("Tasks");
    }
}