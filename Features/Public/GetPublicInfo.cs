using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TaskApi.Core.Interfaces;

namespace TaskApi.Features.Public;

public static class GetPublicInfo
{
    // 1. DTO
    public record PublicInfoResponse(string Message);

    // 2. HANDLER
    public class Handler
    {
        public PublicInfoResponse Execute() => new("Welcome stranger! This info is public.");
    }

    // 3. ENDPOINT (Implements IEndpoint for Assembly Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/public/info", Handle)
               .WithName("GetPublicInfo")
               .WithTags("Public")
               .Produces<PublicInfoResponse>(StatusCodes.Status200OK);
        }

        private static Ok<PublicInfoResponse> Handle(Handler handler)
        {
            return TypedResults.Ok(handler.Execute());
        }
    }
}