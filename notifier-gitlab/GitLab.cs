using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GitLabNotifier;
using GitLabNotifier.VCS;
using NGitLab.Impl;
using NGitLab.Models;

namespace notifier_gitlab
{
    public class GitLab : IGitServer
    {
        private readonly string _gitlabUrl;
        private readonly string _gitlabApiToken;
        private readonly ProjectDetails[] _allProjects;
        private readonly WebClient _webClient;

        public GitLab(string gitlabUrl, string gitlabApiToken, ProjectDetails[] allProjects)
        {
            _gitlabUrl = gitlabUrl;
            _gitlabApiToken = gitlabApiToken;
            _allProjects = allProjects ?? new ProjectDetails[0];
            _webClient = GitLabWebClient(gitlabApiToken);
        }

        private static WebClient GitLabWebClient(string gitlabApiToken)
        {
            return new WebClient { Headers = { ["PRIVATE-TOKEN"] = gitlabApiToken } };
        }

        public Task<IMergeRequest[]> GetOpenMergeRequests(string projectId)
        {
            var api = new Api(_gitlabUrl, _gitlabApiToken, Api.ApiVersion.V4);

            return Task.Run(() => api.Get()
                .GetAll<ExtendedMergeRequest>(Project.Url + "/" + projectId + "/merge_requests?state=" + MergeRequestState.opened)
                .Select(r => new GitLabMergeRequest(projectId, r, _allProjects, _gitlabUrl))
                .Cast<IMergeRequest>()
                .ToArray());
        }

        public Dictionary<string, string[]> GetCommenters(IMergeRequest request)
        {
            var notes = GitLabWebClient(_gitlabApiToken).GetNotesForMergeRequest(request as GitLabMergeRequest);
            var awards = GitLabWebClient(_gitlabApiToken).GetAwardsForMergeRequest(request as GitLabMergeRequest);

            var markCommentAuthors = new Dictionary<string, string[]>();

            markCommentAuthors["allNotesAuthors"] =
                notes.Select(n => n["author"]["username"].ToString().ToLower()).ToArray();

            markCommentAuthors["negative"] = awards
                .Where(x => x["name"].ToString() == "-1" || x["name"].ToString() == "thumbsdown")
                .Select(x => x["user"]["username"].ToString().ToLower())
                .Distinct()
                .ToArray();

            markCommentAuthors["positive"] = awards
                .Where(x => x["name"].ToString() == "+1" || x["name"].ToString() == "thumbsup")
                .Select(x => x["user"]["username"].ToString().ToLower())
                .Distinct()
                .ToArray();

            markCommentAuthors["all"] = markCommentAuthors["positive"].Union(markCommentAuthors["negative"]).ToArray();

            markCommentAuthors["devs"] = markCommentAuthors["all"].Except(Configuration.Instance.Value.Qas).ToArray();

            markCommentAuthors["qas"] = markCommentAuthors["all"].Except(Configuration.Instance.Value.Devs).ToArray();

            return markCommentAuthors;
        }



        public static GitLab ConfiguredWith(Configuration configuration)
        {
            return new GitLab(configuration.GitLabUrl, configuration.GitlabApiToken, configuration.ProjectTeams);
        }
    }
}