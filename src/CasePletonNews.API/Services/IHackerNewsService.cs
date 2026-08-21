using CasePletonNews.API.DTOs;

namespace CasePletonNews.API.Services
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<StoryDTO>> GetBestStoriesAsync(int limit, CancellationToken ct = default);
    }
}