using CasePletonNews.API.Clients;
using CasePletonNews.API.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace CasePletonNews.API.Services
{
    public class HackerNewsService(IHackerNewsApi _hackerNewsApi, IMemoryCache _cache)
    {
        private static readonly SemaphoreSlim _semaphore = new(10);
        private readonly string _cacheKey = "HackerNewsStoryId:";

        public async Task<IEnumerable<StoryDTO>> GetBestStoriesAsync(int limit = 50, CancellationToken ct = default)
        {
            var ids = await _hackerNewsApi.GetBestStoriesAsync(ct);

            var tasks = ids
                .Take(limit)
                .Select(id => FetchCachedAsync(id, ct));

            var stories = await Task.WhenAll(tasks);

            return stories.OrderByDescending(x => x!.Score)!;
        }

        private async Task<StoryDTO?> FetchCachedAsync(int id, CancellationToken ct)
        {
            if (_cache.TryGetValue($"{_cacheKey}{id}", out StoryDTO? cached))
                return cached;

            await _semaphore.WaitAsync(ct);

            try
            {
                var storyDto = await _hackerNewsApi.GetStoryDetailsAsync(id, ct);

                if (storyDto is not null)
                    _cache.Set($"{_cacheKey}{id}", storyDto, TimeSpan.FromMinutes(5));

                return storyDto;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}