using Supabase;

namespace TaskApi.Infrastructure.Filters;

public class SupabaseAuthFilter(Client supabaseClient) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var authHeader = httpContext.Request.Headers.Authorization.ToString();

        // 1. Check for missing or malformed header
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Json(new { error = "Access token required" }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var token = authHeader["Bearer ".Length..].Trim();

        try
        {
            // 2. Online verification with Supabase Auth Server
            var user = await supabaseClient.Auth.GetUser(token);

            if (user?.Id == null)
            {
                return Results.Json(new { error = "Invalid or expired token" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            // 3. Attach user to HttpContext for downstream endpoints
            httpContext.Items["User"] = user;
            httpContext.Items["AccessToken"] = token;
        }
        catch
        {
            // If Supabase throws an exception (token expired, tampered, invalid signature)
            return Results.Json(new { error = "Invalid or expired token" }, statusCode: StatusCodes.Status401Unauthorized);
        }

        return await next(context);
    }
}