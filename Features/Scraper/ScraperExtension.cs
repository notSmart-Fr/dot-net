namespace TaskApi.Features.Scraper;

public static class ScraperExtension
{
    public static IServiceCollection AddScraperFeature(this IServiceCollection services)
    {
        // 1. Stage 1 Storage
        services.AddSingleton<DiskCache>();

        // 2. Stage 1 Polite HTTP Client (uses IHttpClientFactory)
        services.AddHttpClient<PoliteHttpClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // NOTE: HtmlParserEngine and BookScraperPipeline will be uncommented 
        // in Stage 2 and Stage 4 when we create those files!

        return services;
    }
}