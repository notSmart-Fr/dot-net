namespace TaskApi.Features.System;

public static class GetRoot
{
    public record ApiInfo(string Name, string Version, string Status, string Documentation);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new ApiInfo(
            Name: "Task Management API",
            Version: "1.0.0",
            Status: "Running",
            Documentation: "/swagger"
        ))).ExcludeFromDescription(); // Excludes / from Swagger docs
    }
}