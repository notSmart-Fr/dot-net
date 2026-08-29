using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace TaskApi.Infrastructure.ApiDocs;

public static partial class ApiDocsExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "🚀 Scalar Documentation available at: {Url}/docs")]
    private static partial void LogScalarDocsUrl(ILogger logger, string url);

    // 1. DI Service Registration
    public static IServiceCollection AddApiDocsInfrastructure(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Task API",
                    Version = "v1",
                    Description = "Enterprise Task Management API with Supabase Authentication"
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your Supabase Access Token"
                };

                var requirement = new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                };

                document.Security ??= [];
                document.Security.Add(requirement);

                return Task.CompletedTask;
            });
        });

        return services;
    }

    // 2. Middleware & Route Mapping
    public static WebApplication UseApiDocsDevelopmentUI(this WebApplication app)
    {
        // 1. Map OpenAPI spec endpoint (serves /openapi/v1.json)
        app.MapOpenApi();

        // 2. Map Scalar UI at /docs and point it to the actual generated OpenAPI spec.
        // ASP.NET OpenAPI default document name is "v1", not "indexhtml".
        app.MapScalarApiReference("/docs", options =>
        {
            options.WithTitle("Task API Reference")
                   .WithTheme(ScalarTheme.Moon)
                   .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
                   .WithOpenApiRoutePattern("/openapi/v1.json");
        });

        // 3. Log startup path safely
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ScalarDocs");

            var addresses = app.Services.GetService<Microsoft.AspNetCore.Hosting.Server.IServer>()
                               ?.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()
                               ?.Addresses;

            // Use the actual local development endpoint for the app, not the container hostname.
            LogScalarDocsUrl(logger, "http://localhost:5131");
        });

        return app;
    }
}