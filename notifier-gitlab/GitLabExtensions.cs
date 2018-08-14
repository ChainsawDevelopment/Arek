using System.Linq;
using System.Net;
using GitLabNotifier;
using Newtonsoft.Json.Linq;

namespace notifier_gitlab
{
    public static class GitLabExtensions
    {

        public static JArray GetAwardsForMergeRequest(this WebClient gitLabWebClient, GitLabMergeRequest request)
        {
            return JArray.Parse(gitLabWebClient.DownloadString($"{Configuration.Instance.Value.GitLabUrl}/api/v4/projects/{request.ProjectId}/merge_requests/{request.Iid}/award_emoji?per_page=50"));
        }

        public static JArray GetNotesForMergeRequest(this WebClient gitLabWebClient, GitLabMergeRequest request)
        {
            var url = $"{Configuration.Instance.Value.GitLabUrl}/api/v4/projects/{Configuration.Instance.Value.GitProjects.First()}/merge_requests/{request.Id}/notes?per_page=50";
            return JArray.Parse(gitLabWebClient.DownloadString($"{Configuration.Instance.Value.GitLabUrl}/api/v4/projects/{request.ProjectId}/merge_requests/{request.Iid}/notes?per_page=50"));
        }
    }
}