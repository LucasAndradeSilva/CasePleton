using CasePletonNews.API.Clients;
using CasePletonNews.API.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace CasePletonNews.API.Services
{
    public class HackerNewsService(IHackerNewsApi _hackerNewsApi, IMemoryCache _cache)
    {
        private static readonly Serilog.ILogger _logger = Log.ForContext<HackerNewsService>();
        private static readonly SemaphoreSlim _semaphore = new(10);
        private readonly string _cacheKey = "HackerNewsStoryId:";

        public async Task<IEnumerable<StoryDTO>> GetBestStoriesAsync(int limit = 50, CancellationToken ct = default)
        {
            _logger.Information("Fetching best stories with limit {Limit}", limit);

            IReadOnlyList<int> ids;

            try
            {
                ids = await _hackerNewsApi.GetBestStoriesAsync(ct);
                _logger.Information("Retrieved {Count} story ids", ids.Count);            
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch story ids from HackerNews API");
                throw;
            }
            
            var tasks = ids
                .Take(limit)
                .Select(id => FetchCachedAsync(id, ct));

            var storiesResult = await Task.WhenAll(tasks);

            var stories = storiesResult.OrderByDescending(x => x!.Score)!;

            _logger.Information("Returning {Count} stories", stories.Count());

            return stories!;
        }

        private async Task<StoryDTO?> FetchCachedAsync(int id, CancellationToken ct)
        {
            if (_cache.TryGetValue($"{_cacheKey}{id}", out StoryDTO? cached))
                return cached;

            await _semaphore.WaitAsync(ct);

            try
            {
                _logger.Debug("Fetching Story {Id} from API", id);

                var storyDto = await _hackerNewsApi.GetStoryDetailsAsync(id, ct);

                if (storyDto is not null)
                    _cache.Set($"{_cacheKey}{id}", storyDto, TimeSpan.FromMinutes(5));
                else
                    _logger.Warning("Story {Id} returned null from API", id);

                return storyDto;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch StoryDetails {Id}", id);
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}