using CvSite.Services;
using Microsoft.AspNetCore.Mvc;

namespace CvSite.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;

        public PortfolioController(IGitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PortfolioRepoItem>>> Get()
        {
            var portfolio = await _gitHubService.GetPortfolio();
            return Ok(portfolio);
        }
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<PortfolioRepoItem>>> Search(
        [FromQuery] string? repoName=null,
        [FromQuery] string? language = null,
        [FromQuery] string? userName = null)
        {
            var searchResults = await _gitHubService.SearchRepositories(repoName, language, userName);
            return Ok(searchResults);
        }
    }
}
