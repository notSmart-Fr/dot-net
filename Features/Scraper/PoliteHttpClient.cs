using System.Net;

namespace TaskApi.Features.Scraper;

public class PoliteHttpClient(HttpClient httpClient, DiskCache cache, ILogger<PoliteHttpClient> logger)
{
    public async Task<(string Content, bool IsCacheHit)> FetchWithCacheAsync(string url, CancellationToken ct)
    {
        // 1. Check disk cache first
        var cached = await cache.GetAsync(url, ct);
        if (cached is not null)
        {
            logger.LogCacheHit(url, cached.Length);
            return (cached, true);
        }

        logger.LogFetch(url);

        // 2. Politeness delay (500ms before making a real outbound request)
        await Task.Delay(500, ct);

        // 3. Configure request with honest User-Agent
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("FlyRankInternshipA9/1.0 (+https://github.com/yourusername/repo)");

        // 4. Set a 5-second timeout policy
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        // 5. Send request and verify HTTP status
        var response = await httpClient.SendAsync(request, cts.Token);
        
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new ScraperRateLimitException();

        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(ct);

        // 6. Save downloaded HTML to disk cache
        await cache.SaveAsync(url, html, ct);

        return (html, false);
    }
}