using CasePletonNews.API.Services;

namespace CasePletonNews.API.Endpoints
{
    public static class StoriesEndpoint
    {
        public static void Map(WebApplication app)
        {
            app.MapGet("/api/stories/best", async (
                HackerNewsService service,
                int limit = 50,
                CancellationToken ct = default) =>
            {
                var stories = await service.GetBestStoriesAsync(limit, ct);
                return Results.Ok(stories);
            });
        }
    }
}