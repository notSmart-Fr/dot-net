using FluentValidation;
using Supabase;
using TaskApi.Common;
using TaskApi.Infrastructure;

namespace TaskApi.Features.Auth;

public static class Signup
{
    // 1. DTOs
    public record SignupRequest(string Email, string Password);
    public record SignupResponse(string Id, string Email);

    // 2. VALIDATOR
    public class SignupValidator : AbstractValidator<SignupRequest>
    {
        public SignupValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }

    // 3. HANDLER
    public class Handler(Client supabaseClient)
    {
        private readonly Client _supabaseClient = supabaseClient;

        public async Task<SignupResponse> ExecuteAsync(SignupRequest request, CancellationToken ct)
        {
            // If Supabase rejects signup (e.g. duplicate email), it throws GotrueException
            // which GotrueExceptionMapper cleanly handles as a 400 Bad Request.
            var signUpResult = await _supabaseClient.Auth.SignUp(request.Email, request.Password);
            
            if (signUpResult?.User?.Id == null || signUpResult.User.Email == null)
            {
                throw new InvalidOperationException("Sign up failed to return user metadata.");
            }

            return new SignupResponse(signUpResult.User.Id, signUpResult.User.Email);
        }
    }

    // 4. ENDPOINT ROUTE
    public static void Map(IEndpointRouteBuilder app)
    {
        // POST /auth/signup
        app.MapPost("/auth/signup", async (SignupRequest request, Handler handler, CancellationToken ct) =>
        {
            var response = await handler.ExecuteAsync(request, ct);
            return Results.Created($"/users/{response.Id}", response);
        })
        .WithName("Signup")
        .WithTags("Auth")
        .AddEndpointFilter<ValidationFilter<SignupRequest>>()
        .Produces<SignupResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}