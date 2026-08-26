using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Supabase;

namespace TaskApi.Infrastructure.Extensions;

public static class AuthAndApiDocsExtensions
{
    public static IServiceCollection AddSupabaseAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var supabaseUrl = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL missing");
        var supabaseKey = configuration["Supabase:PublishableKey"]
            ?? configuration["Supabase:AnonKey"]
            ?? throw new InvalidOperationException("Supabase PublishableKey missing");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{supabaseUrl}/auth/v1";
                options.Audience = "authenticated";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"{supabaseUrl}/auth/v1",
                    ValidateAudience = true,
                    ValidAudience = "authenticated",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddScoped(_ => new Client(supabaseUrl, supabaseKey, new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        }));

        return services;
    }

    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
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
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                };

                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(requirement);

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseDevelopmentApiDocs(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("Task API Reference")
                   .WithTheme(ScalarTheme.Moon)
                   .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
        });

        app.MapGet("/bug", _ => throw new Exception("Database connection failed!"));
        return app;
    }
}