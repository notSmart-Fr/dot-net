using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskApi.Core.Interfaces;

namespace TaskApi.Features.System;

public static class GetRoot
{
    // 1. DTO
    public record ApiInfo(string Name, string Version, string Status, string Documentation);

    // 2. ENDPOINT (Implements IEndpoint for Assembly Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/", Handle)
               .WithName("GetRoot")
               .WithTags("System")
               .Produces<ApiInfo>(StatusCodes.Status200OK)
               .ExcludeFromDescription(); // Excludes / from Swagger/Scalar docs
        }

        private static IResult Handle()
        {
            return Results.Ok(new ApiInfo(
                Name: "Task Management API",
                Version: "1.0.0",
                Status: "Running",
                Documentation: "/scalar/v1" // Or "/swagger" depending on your setup
            ));
        }
    }
}