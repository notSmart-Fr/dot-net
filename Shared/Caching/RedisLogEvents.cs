namespace TaskApi.Infrastructure.Caching;

public static partial class RedisLogEvents
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "[REDIS SUCCESS] Connected to Redis at {Host}! Ping latency: {Latency} ms")]
    public static partial void RedisConnected(this ILogger logger, string host, double latency);
}