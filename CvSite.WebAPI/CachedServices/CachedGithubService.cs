using CvSite.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CvSite.WebAPI.CachedServices
{
    public class CachedGithubService : IGitHubService
    {
        private readonly IGitHubService _gitHubService;
        private readonly IMemoryCache _memoryCache;

        private const string UserPortfolioKey = "UserPortfolioKey";
        public CachedGithubService(IGitHubService gitHubService, IMemoryCache memoryCache)
        {
            _gitHubService=gitHubService;
            _memoryCache=memoryCache;
        }
        public async Task<IReadOnlyList<PortfolioRepoItem>> GetPortfolio()
        {
            if (_memoryCache.TryGetValue(UserPortfolioKey, out IReadOnlyList<PortfolioRepoItem> portfolioRepoItem))
                return portfolioRepoItem;

            var cacheOption = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30)).SetSlidingExpiration(TimeSpan.FromSeconds(10));


            portfolioRepoItem= await _gitHubService.GetPortfolio();
            _memoryCache.Set(UserPortfolioKey, portfolioRepoItem);
            return portfolioRepoItem;

        }

        public Task<IReadOnlyList<PortfolioRepoItem>> SearchRepositories(string repoName, string language, string userName)
        {
            return _gitHubService.SearchRepositories(repoName, language, userName);
        }
    }
}
