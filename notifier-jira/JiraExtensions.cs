using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitLabNotifier.VCS;
using Newtonsoft.Json.Linq;

namespace GitLabNotifier
{
    public class JiraConfig
    {
        public string JiraUrl { get; set; }
        public string Token { get; set; }

        public string ProjectKey { get; set; }
    }

    public class Jira : ITicketSystem
    {
        private readonly JiraConfig _config;

        public Jira()
        {
            _config = new JiraConfig()
            {
                JiraUrl = Configuration.Instance.Value.JiraBaseUrl,
                Token = Configuration.Instance.Value.JiraToken
            };
        }

        private string GetJiraId(string requestTitle)
        {
            var match = new Regex(@"^\s*(\w+-\d+).*").Match(requestTitle);
            return match.Success ? match.Groups[1].Value : null;
        }

        public async Task<TicketDetails> GetTicketStatus(IMergeRequest request)
        {
            Console.WriteLine($"Getting JIRA status for {request.Title}");

            try
            {
                var jiraId = GetJiraId(request.Title);
                if (string.IsNullOrEmpty(jiraId))
                {
                    return new TicketDetails(jiraId);
                }

                var jiraUrl = $"{_config.JiraUrl}/rest/api/latest/issue/{jiraId}?fields=status";

                string jiraResposne;
                using (var jiraWebClient = new WebClient())
                {
                    jiraWebClient.Headers[HttpRequestHeader.Authorization] = $"Basic {_config.Token}";
                    jiraResposne = await jiraWebClient.DownloadStringTaskAsync(new Uri(jiraUrl));
                }

                var parsedJiraIssue = JObject.Parse(jiraResposne);
                var ticketDetails = new TicketDetails(jiraId);
                ticketDetails.Url = $"{jiraUrl}/browse/{jiraId}";
                ticketDetails.Status = parsedJiraIssue["fields"]["status"]["name"].ToString();
                if (parsedJiraIssue["fields"]["labels"] != null)
                {
                    ticketDetails.Labels = parsedJiraIssue["fields"]["labels"].Values<string>() ?? new string[] { };
                }
                else
                {
                    ticketDetails.Labels = new string[] { };
                }

                return ticketDetails;
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    return new TicketDetails(GetJiraId(request.Title));
                }

                throw;
            }
        }

        public static ITicketSystem WithToken(string token)
        {
            return new Jira();
        }
    }

    static class JiraExtensions
    {
        private const string ProjectKey = "SPA";

        public static string GetJiraId(this string requestTitle, string key=ProjectKey)
        {
            var match = new Regex($@".*({ProjectKey}-\d+).*").Match(requestTitle);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}