using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Supabase;
using TaskApi.Shared.Interfaces;
using static Supabase.Gotrue.Constants;

namespace TaskApi.Features.Auth;

public static class Logout
{
    // 1. HANDLER
    public class Handler(Client supabaseClient, TokenRevocationService tokenRevocationService)
{
    public async Task ExecuteAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        // 1. Blacklist the session_id in Redis
        await tokenRevocationService.RevokeAsync(user, ct);

        // 2. Revoke Supabase refresh token
        await supabaseClient.Auth.SignOut(SignOutScope.Local);
    }
}

    // 2. ENDPOINT (Auto-mapped via IEndpoint scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/logout", HandleAsync)
               .WithName("Logout")
               .WithTags("Auth")
               .RequireAuthorization()
               .Produces(StatusCodes.Status204NoContent)
               .ProducesProblem(StatusCodes.Status401Unauthorized);
        }

        private static async Task<NoContent> HandleAsync(ClaimsPrincipal user, Handler handler, CancellationToken ct)
        {
            await handler.ExecuteAsync(user, ct);
            return TypedResults.NoContent();
        }
    }
}