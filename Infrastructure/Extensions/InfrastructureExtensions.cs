using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TaskApi.Common.Exceptions;
using TaskApi.Common.Exceptions.Mappers;

namespace TaskApi.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void AddCustomLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);
    }

    public static IServiceCollection AddGlobalExceptionInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IExceptionMapper, BadRequestExceptionMapper>();
        services.AddSingleton<IExceptionMapper, GotrueExceptionMapper>();
        services.AddSingleton<IExceptionMapper, UnauthorizedExceptionMapper>();
        services.AddSingleton<IExceptionMapper, DomainExceptionMapper>();
        services.AddSingleton<IExceptionMapper, EntityExceptionMapper>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static IServiceCollection AddDatabaseAndCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration.GetConnectionString("Default");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        var redisConnectionString = configuration["Redis:Connection"] 
            ?? configuration["REDIS_CONNECTION"] 
            ?? "redis:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectTimeout = 5000;
        redisOptions.ConnectRetry = 5;

        var redisConnection = ConnectionMultiplexer.Connect(redisOptions);
        services.AddSingleton<IConnectionMultiplexer>(redisConnection);

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "database", tags: ["ready"])
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"]);

        return services;
    }

    public static async Task VerifyInfrastructureConnectionsAsync(this IServiceProvider services, IConfiguration configuration)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var redis = services.GetRequiredService<IConnectionMultiplexer>();
        var redisConnectionString = configuration["Redis:Connection"]
            ?? configuration["REDIS_CONNECTION"]
            ?? "redis:6379";

        var latency = await redis.GetDatabase().PingAsync();
        logger.RedisConnected(redisConnectionString, latency.TotalMilliseconds);
        logger.LogInformation("🚀 Scalar API reference available at: http://localhost:5131/scalar/v1");
    }
}