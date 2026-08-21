using CasePletonNews.API.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CasePletonNews.API.Endpoints
{
    public static class StoriesEndpoint
    {
        private static readonly Serilog.ILogger _logger = Log.ForContext(typeof(StoriesEndpoint));

        public static void Map(WebApplication app)
        {
            app.MapGet("/api/stories/best", async (HackerNewsService service, [FromQuery] int limit = 50, CancellationToken ct = default) =>
            {
                _logger.Information("GET /api/stories/best called with limit {Limit}", limit);

                if (limit <= 0)
                {
                    _logger.Warning("Invalid limit value {Limit}", limit);
                    return Results.Problem(
                        title: "Invalid parameter",
                        detail: "Limit must be a positive integer.",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var stories = await service.GetBestStoriesAsync(limit, ct);

                if (!stories.Any())
                {
                    _logger.Warning("No stories returned for limit {Limit}", limit);
                    return Results.Problem(
                        title: "No content",
                        detail: "No stories were found.",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

                _logger.Information("Returning {Count} stories", stories.Count());
                return Results.Ok(stories);
            })
            .WithName("GetBestStories")
            .WithSummary("Returns the best stories from Hacker News")
            .WithTags("Stories")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        }
    }
}