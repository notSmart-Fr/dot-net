namespace TaskApi.Features.Public;

public static class GetPublicInfo
{
    public record PublicInfoResponse(string Message);

    public class Handler
    {
        public PublicInfoResponse Execute() => new("Welcome stranger! This info is public.");
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/public/info", (Handler handler) => TypedResults.Ok(handler.Execute()))
            .WithName("GetPublicInfo")
            .WithTags("Public")
            .Produces<PublicInfoResponse>(StatusCodes.Status200OK);
    }
}