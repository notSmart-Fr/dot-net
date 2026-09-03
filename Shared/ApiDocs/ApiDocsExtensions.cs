using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
            // Document Transformer: Defines API Info & Security Scheme definitions
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

                // REMOVED: document.Security.Add(requirement); 
                // Global assignment removed to prevent locking public endpoints.

                return Task.CompletedTask;
            });

            // Operation Transformer: Dynamically applies Auth label ONLY to protected routes
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;

                // Check if endpoint is explicitly marked [AllowAnonymous]
                bool isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
                
                // Check if endpoint has [Authorize] or .RequireAuthorization()
                bool requiresAuth = metadata.OfType<IAuthorizeData>().Any();

                if (requiresAuth && !isAnonymous)
                {
                    operation.Security ??= new List<OpenApiSecurityRequirement>();
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                    });
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }

    // 2. Middleware & Route Mapping
    public static WebApplication UseApiDocsDevelopmentUI(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference("/docs", options =>
        {
            options.WithTitle("Task API Reference")
                   .WithTheme(ScalarTheme.Moon)
                   .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
                   .WithOpenApiRoutePattern("/openapi/v1.json");
        });

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ScalarDocs");
            LogScalarDocsUrl(logger, "http://localhost:5131");
        });

        return app;
    }
}