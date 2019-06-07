using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Arek.Contracts;
using Newtonsoft.Json.Linq;

namespace Arek.Jira
{
    public class Jira : ITicketSystem
    {
        private readonly JiraConfig _config;

        public Jira(string jiraBaseUrl, string jiraAuthHeader)
        {
            _config = new JiraConfig()
            {
                JiraUrl = jiraBaseUrl,
                Token = jiraAuthHeader
            };
        }

        private string GetJiraId(string requestTitle)
        {
            var match = new Regex(@"^\s*(\w+-\d+).*").Match(requestTitle);
            return match.Success ? match.Groups[1].Value : null;
        }

        public async Task<TicketDetails> GetTicketStatus(IMergeRequest request)
        {

            var jiraId = GetJiraId(request.Title) ?? GetJiraId(request.SourceBranchName);

            try
            {
                if (string.IsNullOrEmpty(jiraId))
                {
                    Console.WriteLine($"Cannot find jira ticket id for {request.Title}");
                    return new TicketDetails(jiraId);
                }
                
                Console.WriteLine($"Getting JIRA {jiraId} status for {request.Title}");

                var jiraUrl = $"{_config.JiraUrl}/rest/api/latest/issue/{jiraId}?fields=status";

                string jiraResposne;
                using (var jiraWebClient = new WebClient())
                {
                    jiraWebClient.Headers[HttpRequestHeader.Authorization] = $"Basic {_config.Token}";
                    jiraResposne = await jiraWebClient.DownloadStringTaskAsync(new Uri(jiraUrl));
                }

                var parsedJiraIssue = JObject.Parse(jiraResposne);
                var ticketDetails = new TicketDetails(jiraId);
                ticketDetails.Url = $"{_config.JiraUrl}/browse/{jiraId}";
                ticketDetails.Status = parsedJiraIssue["fields"]["status"]["name"].ToString();
                if (parsedJiraIssue["fields"]["labels"] != null)
                {
                    ticketDetails.Labels = parsedJiraIssue["fields"]["labels"].Values<string>() ?? new string[] { };
                }
                else
                {
                    ticketDetails.Labels = new string[] { };
                }

                Console.WriteLine($"JIRA {jiraId} status for {request.Title} is {ticketDetails.Status}");

                return ticketDetails;
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response)?.StatusCode == HttpStatusCode.NotFound)
                {
                    return new TicketDetails(jiraId);
                }

                throw;
            }
        }

        public static ITicketSystem WithConfig(string baseUrl, string token)
        {
            return new Jira(baseUrl, token);
        }
    }
}