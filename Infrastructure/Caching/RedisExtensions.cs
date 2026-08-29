using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace TaskApi.Infrastructure.Caching;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? configuration["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis connection string 'ConnectionStrings:Redis' or 'Redis:ConnectionString' is missing.");

        // 1. Register ConnectionMultiplexer as a Singleton
        var redisMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);

        // 2. Register IDistributedCache using Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "TaskApi:";
        });

        // 3. Persist ASP.NET Core Data Protection keys in Redis
        services.AddDataProtection()
            .SetApplicationName("TaskApi")
            .PersistKeysToStackExchangeRedis(redisMultiplexer, "TaskApi-DataProtection-Keys");

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck("redis", () =>
            {
                return redisMultiplexer.IsConnected
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy("Redis is not connected.");
            }, tags: ["ready"]);

        return services;
    }
    // Modern, non-blocking startup connection probe
    public static async Task VerifyRedisConnectionAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IConnectionMultiplexer>>();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var redisConnectionString = configuration.GetConnectionString("Redis") 
            ?? configuration["Redis:ConnectionString"] 
            ?? "localhost:6379";

        var latency = await redis.GetDatabase().PingAsync();

        // High-performance compile-time [LoggerMessage] execution
        logger.RedisConnected(redisConnectionString, latency.TotalMilliseconds);
    }
}