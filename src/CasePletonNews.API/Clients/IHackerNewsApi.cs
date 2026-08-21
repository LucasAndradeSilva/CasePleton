using CasePletonNews.API.DTOs;
using Refit;

namespace CasePletonNews.API.Clients
{
    public interface IHackerNewsApi
    {
        [Get("/item/{id}.json")]
        Task<StoryDTO?> GetStoryDetailsAsync(int id, CancellationToken ct = default);

        [Get("/beststories.json")]
        Task<IReadOnlyList<int>> GetBestStoriesAsync(CancellationToken ct = default);
    }
}
