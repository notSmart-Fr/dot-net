namespace TaskApi.Features.Scraper;

public static partial class LogExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "CACHE HIT: {Url} ({Size} bytes)")]
    public static partial void LogCacheHit(this ILogger logger, string url, int size);

    [LoggerMessage(Level = LogLevel.Information, Message = "FETCH: {Url}")]
    public static partial void LogFetch(this ILogger logger, string url);
}