using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Supabase;

namespace TaskApi.Infrastructure.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var supabaseUrl = configuration["Supabase:Url"] 
            ?? throw new InvalidOperationException("Configuration setting 'Supabase:Url' is missing.");
            
        var supabaseKey = configuration["Supabase:PublishableKey"]
            ?? configuration["Supabase:AnonKey"]
            ?? throw new InvalidOperationException("Configuration setting 'Supabase:PublishableKey' or 'AnonKey' is missing.");

        services.AddSingleton<TokenRevocationService>();

        // 1. Configure ASP.NET Core JWT Bearer Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {   // Preserves raw JWT claim names ("jti", "exp", "sub") without mapping them to XML URIs
                options.MapInboundClaims = false;
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
                // 2. THIS IS THE GUARD: Intercept every request to check Redis
                 options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        if (principal is null) return;

                        var id = TokenRevocationService.ExtractIdentifier(principal);
                        if (string.IsNullOrEmpty(id)) return;

                        var revocationService = context.HttpContext.RequestServices
                            .GetRequiredService<TokenRevocationService>();

                        if (await revocationService.IsRevokedAsync(id, context.HttpContext.RequestAborted))
                        {
                            context.Fail("This token session has been revoked.");
                        }
                    }
                }; 
            }); 

        services.AddAuthorization();

        // 2. Register Supabase SDK Client
        services.AddScoped(_ => new Client(supabaseUrl, supabaseKey, new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        }));

        return services;
    }
}