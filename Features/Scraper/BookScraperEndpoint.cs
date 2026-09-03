

using TaskApi.Shared.Interfaces;

namespace TaskApi.Features.Scraper;

public class BookScraperEndpoint : IEndpoint
{
    public record Stage1Response(string Url, bool IsCacheHit, int ByteCount);
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Stage 1 Endpoint: Triggers fetch & cache for a single URL
        app.MapPost("/api/scraper/stage1", HandleStage1Async)
           .WithName("ScraperStage1")
           .WithTags("Scraper")
           .Produces<Stage1Response>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleStage1Async(
        PoliteHttpClient client,
        CancellationToken ct)
    {
        const string targetUrl = "https://books.toscrape.com/catalogue/page-1.html";

        var (content, isCacheHit) = await client.FetchWithCacheAsync(targetUrl, ct);

        return TypedResults.Ok(new Stage1Response(
            Url: targetUrl,
            IsCacheHit: isCacheHit,
            ByteCount: content.Length
        ));
    }
}

