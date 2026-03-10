namespace StoryShare.Api.Health;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/version", () => Results.Ok(new { version = "1.0.14"}));
    }
}
