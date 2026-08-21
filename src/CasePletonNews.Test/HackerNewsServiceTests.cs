using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
using CasePletonNews.API.Clients;
using CasePletonNews.API.Services;
using CasePletonNews.API.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using CasePletonNews.API.Settings;

namespace CasePletonNews.Test;

public class HackerNewsServiceTests
{
    [Fact]
    public async Task GetBestStoriesAsync_ReturnsStoriesOrderedByScore()
    {
        // Arrange
        var mockApi = new Mock<IHackerNewsApi>();
        mockApi.Setup(x => x.GetBestStoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 2, 3 } as IReadOnlyList<int>);

        mockApi.Setup(x => x.GetStoryDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoryDTO { Id = 1, Score = 10 });
        mockApi.Setup(x => x.GetStoryDetailsAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoryDTO { Id = 2, Score = 30 });
        mockApi.Setup(x => x.GetStoryDetailsAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoryDTO { Id = 3, Score = 20 });

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new HackerNewsSettings { CacheMinutes = 5 });

        var service = new HackerNewsService(mockApi.Object, memoryCache, options);

        // Act
        var result = (await service.GetBestStoriesAsync(3)).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(2, result[0].Id);
        Assert.Equal(3, result[1].Id);
        Assert.Equal(1, result[2].Id);
    }

    [Fact]
    public async Task GetBestStoriesAsync_CachesStoryDetails()
    {
        // Arrange
        var mockApi = new Mock<IHackerNewsApi>();
        mockApi.Setup(x => x.GetBestStoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 } as IReadOnlyList<int>);

        mockApi.Setup(x => x.GetStoryDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoryDTO { Id = 1, Score = 5 });

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new HackerNewsSettings { CacheMinutes = 5 });

        var service = new HackerNewsService(mockApi.Object, memoryCache, options);

        // Act
        var first = (await service.GetBestStoriesAsync(1)).ToList();
        var second = (await service.GetBestStoriesAsync(1)).ToList();

        // Assert
        mockApi.Verify(x => x.GetStoryDetailsAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal(1, first[0].Id);
    }
}
