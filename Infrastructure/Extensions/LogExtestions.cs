namespace TaskApi.Infrastructure.Extensions;

public static partial class LogExtensions
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "[REDIS SUCCESS] Connected to Redis at {Host}! Ping latency: {Latency} ms")]
    public static partial void RedisConnected(this ILogger logger, string host, double latency);
}