using Microsoft.EntityFrameworkCore;

namespace TaskApi.Shared.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {   
                options.UseNpgsql(connectionString)
                        .UseSnakeCaseNamingConvention(); // <-- Translates C# "Id" to Postgres "id" automatically

                // Enables transient fault handling for PostgreSQL connection blips
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            }));

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "database", tags: ["ready"]);

        return services;
    }
}