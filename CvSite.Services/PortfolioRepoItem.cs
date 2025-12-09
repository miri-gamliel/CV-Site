using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CvSite.Services
{
    public class PortfolioRepoItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int Stars { get; set; }
        public int PullRequests { get; set; }
        public DateTimeOffset LastCommitDate { get; set; }
        public IReadOnlyList<string> Languages { get; set; }
        public string HomepageUrl { get; set; }
    }
}
