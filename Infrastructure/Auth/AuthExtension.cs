using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
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

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var revocationService = context.HttpContext.RequestServices.GetRequiredService<TokenRevocationService>();
                        var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                        if (await revocationService.IsRevokedAsync(jti))
                        {
                            context.Fail("Token has been revoked.");
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