using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvSite.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private readonly GitHubSettings _settings;

        public GitHubService(IOptions<GitHubSettings> options)
        {
            _settings = options.Value;
            _client = new GitHubClient(new ProductHeaderValue("CvSite-App"));
            var tokenAuth = new Credentials(_settings.Token);
            _client.Credentials = tokenAuth;
        }

        public async Task<IReadOnlyList<PortfolioRepoItem>> GetPortfolio()
        {
            var repositories = await _client.Repository.GetAllForCurrent();

            var tasks = repositories
                .Select(async repo =>
                {
                    // משיכה אסינכרונית של נתונים נוספים
                    var languagesTask = _client.Repository.GetAllLanguages(repo.Id);
                    var commitsTask = _client.Repository.Commit.GetAll(repo.Id, new ApiOptions { PageSize = 1, PageCount = 1 });

                    await Task.WhenAll(languagesTask, commitsTask);

                    var languages = await languagesTask;
                    var commits = await commitsTask;

                    return new PortfolioRepoItem
                    {
                        Name = repo.Name,
                        Description = repo.Description,
                        Url = repo.HtmlUrl,
                        Stars = repo.StargazersCount,
                        PullRequests = repo.OpenIssuesCount,
                        LastCommitDate = commits.Any() ? commits.First().Commit.Author.Date : repo.UpdatedAt,
                        // Octokit מחזיר רשימת שפות עם גודל הקוד. אנחנו צריכים רק את השמות.
                        Languages = languages.Select(l => l.Name).ToList(),
                        HomepageUrl = repo.Homepage,
                    };
                })
                .ToList();

            var portfolio = await Task.WhenAll(tasks);

            return portfolio.ToList();
        }

        public async Task<IReadOnlyList<PortfolioRepoItem>> SearchRepositories(string repoName, string language, string userName)
        {
            // 1. בדיקה: אם אין אף קריטריון, מחזירים רשימה ריקה
            if (string.IsNullOrEmpty(repoName) && string.IsNullOrEmpty(language) && string.IsNullOrEmpty(userName))
            {
                return new List<PortfolioRepoItem>();
            }

            // 2. בניית מחרוזת שאילתה ל-GitHub (לסינון ראשוני)
            var searchTerms = new List<string>();

            if (!string.IsNullOrEmpty(repoName))
            {
                // אם מכיל רווחים, מוסיפים מרכאות
                searchTerms.Add(repoName.Contains(' ') ? $"\"{repoName}\"" : repoName);
            }
            if (!string.IsNullOrEmpty(language))
            {
                // הוספת השפה כאופרטור לחיפוש רחב
                searchTerms.Add($"language:{language}");
            }
            if (!string.IsNullOrEmpty(userName))
            {
                // הוספת המשתמש כאופרטור לחיפוש רחב
                searchTerms.Add($"user:{userName}");
            }

            var finalQuery = string.Join(" ", searchTerms);
            var request = new SearchRepositoriesRequest(finalQuery);

            // 3. שליחה ל-GitHub וקבלת תוצאות ראשוניות
            var result = await _client.Search.SearchRepo(request);

            // 4. סינון בתוך C# (כדי לכפות AND לוגי מדויק על כל הפרמטרים שהוזנו)
            var filteredResults = result.Items.AsEnumerable();

            // סינון 1: לפי שם משתמש (User) - אם הוזן, חייב להתאים בדיוק
            if (!string.IsNullOrEmpty(userName))
            {
                filteredResults = filteredResults.Where(repo =>
                    repo.Owner != null &&
                    repo.Owner.Login.Equals(userName, System.StringComparison.OrdinalIgnoreCase)
                );
            }

            // סינון 2: לפי שפה (Language) - אם הוזן, חייב להתאים בדיוק
            if (!string.IsNullOrEmpty(language))
            {
                // Octokit מחזיר את השפה הראשית כ-string בתוך repo.Language
                filteredResults = filteredResults.Where(repo =>
                    !string.IsNullOrEmpty(repo.Language) &&
                    repo.Language.Equals(language, System.StringComparison.OrdinalIgnoreCase)
                );
            }

            // 5. המרת התוצאות המסוננות למודל PortfolioRepoItem
            var items = filteredResults
                .Select(repo => new PortfolioRepoItem
                {
                    Name = repo.Name,
                    Description = repo.Description,
                    Url = repo.HtmlUrl,
                    Stars = repo.StargazersCount,
                    // נשארים עם התיקון הקודם שעבד:
                    PullRequests = repo.OpenIssuesCount,
                    LastCommitDate = repo.UpdatedAt,
                    Languages = new List<string> { repo.Language },
                    HomepageUrl = repo.Homepage,
                })
                .ToList();

            return items;
        }





    }
}