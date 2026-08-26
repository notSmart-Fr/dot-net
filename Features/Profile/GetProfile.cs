using System.Security.Claims;

namespace TaskApi.Features.Profile;

public static class GetProfile
{
    public record ProfileResponse(string UserId, string Email);

    public class Handler
    {
        public ProfileResponse Execute(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? user.FindFirst("sub")?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value 
                        ?? user.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user identity in token.");
            }

            return new ProfileResponse(userId, email ?? "N/A");
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/protected/profile", (ClaimsPrincipal user, Handler handler) => TypedResults.Ok(handler.Execute(user)))
            .WithName("GetProfile")
            .WithTags("Protected")
            .RequireAuthorization() // Native .NET JWT Guard
            .Produces<ProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}