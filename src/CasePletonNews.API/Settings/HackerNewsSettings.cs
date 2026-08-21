namespace CasePletonNews.API.Settings
{
    public class HackerNewsSettings
    {
        public const string SectionName = "HackerNews";

        public string BaseUrl { get; init; } = string.Empty;
        public int TimeoutSeconds { get; init; } = 10;
        public int CacheMinutes { get; init; } = 5;        
    }
}