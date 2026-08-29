using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Supabase;
using TaskApi.Core.Interfaces;
using TaskApi.Infrastructure.Auth;
using static Supabase.Gotrue.Constants;

namespace TaskApi.Features.Auth;

public static class Logout
{
    // 1. HANDLER
    public class Handler(Client supabaseClient, TokenRevocationService tokenRevocationService)
    {
        public async Task ExecuteAsync(ClaimsPrincipal user)
        {
            var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrWhiteSpace(jti))
            {
                await tokenRevocationService.RevokeAsync(jti, user);
            }

            // Revokes the user's session globally across Supabase Auth
            await supabaseClient.Auth.SignOut(SignOutScope.Global);
        }
    }

    // 2. ENDPOINT (Implements IEndpoint for Assembly Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/logout", HandleAsync)
               .WithName("Logout")
               .WithTags("Auth")
               .RequireAuthorization() // Native .NET JWT Guard
               .Produces(StatusCodes.Status204NoContent)
               .ProducesProblem(StatusCodes.Status401Unauthorized);
        }

        private static async Task<NoContent> HandleAsync(ClaimsPrincipal user, Handler handler)
        {
            await handler.ExecuteAsync(user);
            return TypedResults.NoContent();
        }
    }
}