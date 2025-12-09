using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvSite.Services
{
    public interface IGitHubService
    {
        Task<IReadOnlyList<PortfolioRepoItem>> GetPortfolio();
        Task<IReadOnlyList<PortfolioRepoItem>> SearchRepositories(string repoName, string language, string userName);
    }
}
