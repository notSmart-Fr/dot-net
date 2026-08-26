using FluentValidation;
using Supabase;
using TaskApi.Common;
using TaskApi.Infrastructure;

namespace TaskApi.Features.Auth;

public static class Login
{
    // 1. DTOs
    public record Request(string Email, string Password);
    
    // Named to match standard OAuth2 / assignment JWT response keys
    public record Response(
        string AccessToken, 
        string RefreshToken, 
        string TokenType, 
        long ExpiresIn);

    // 2. VALIDATOR
    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }

    // 3. HANDLER
    public class Handler(Client supabaseClient)
    {
        private readonly Client _supabaseClient = supabaseClient;

        public async Task<Response> ExecuteAsync(Request request, CancellationToken ct)
        {
            // Note: If credentials are wrong, Supabase throws GotrueException.
            // GotrueExceptionMapper intercepts it and returns a clean 401 Unauthorized.
            var session = await _supabaseClient.Auth.SignIn(request.Email, request.Password);

            if (session?.AccessToken == null)
            {
                throw new UnauthorizedAccessException("Invalid login credentials.");
            }

            return new Response(
                AccessToken: session.AccessToken,
                RefreshToken: session.RefreshToken ?? string.Empty,
                TokenType: "Bearer",
                ExpiresIn: session.ExpiresIn
            );
        }
    }

    // 4. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
    {
        // POST /auth/login
        app.MapPost("/auth/login", async (Request request, Handler handler, CancellationToken ct) =>
        {
            var response = await handler.ExecuteAsync(request, ct);
            return Results.Ok(response);
        })
        .WithName("Login")
        .WithTags("Auth")
        .AddEndpointFilter<ValidationFilter<Request>>()
        .Produces<Response>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}