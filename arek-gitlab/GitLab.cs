using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Arek.Contracts;
using Arek.Engine;
using NGitLab.Impl;
using NGitLab.Models;

namespace Arek.GitLab
{
    public class GitLab : IGitServer
    {
        private readonly string _gitlabUrl;
        private readonly string _gitlabApiToken;
        private readonly ProjectDetails[] _allProjects;

        public GitLab(string gitlabUrl, string gitlabApiToken, ProjectDetails[] allProjects)
        {
            _gitlabUrl = gitlabUrl;
            _gitlabApiToken = gitlabApiToken;
            _allProjects = allProjects ?? new ProjectDetails[0];
        }

        private static WebClient GitLabWebClient(string gitlabApiToken)
        {
            return new WebClient { Headers = { ["PRIVATE-TOKEN"] = gitlabApiToken } };
        }

        public async Task<IMergeRequest[]> GetOpenMergeRequests(string projectId)
        {
            var api = new Api(_gitlabUrl, _gitlabApiToken, Api.ApiVersion.V4);

            var updateAfterTime=DateTime.UtcNow.AddDays(-7).ToString("o");

            var recentlyChanged = await Task.Run(() => api.Get()
                .GetAll<ExtendedMergeRequest>($"{Project.Url}/{projectId}/merge_requests?updated_after={updateAfterTime}")
                .Select(r => new GitLabMergeRequest(projectId, r, _allProjects, _gitlabUrl))
                .Cast<IMergeRequest>()
                .ToArray());

            var openeded = await Task.Run(() => api.Get()
                .GetAll<ExtendedMergeRequest>(Project.Url + "/" + projectId + "/merge_requests?state=" + MergeRequestState.opened)
                .Select(r => new GitLabMergeRequest(projectId, r, _allProjects, _gitlabUrl))
                .Cast<IMergeRequest>()
                .ToArray());

            var relevantMergeRequests = recentlyChanged.Concat(openeded).Distinct().ToArray();
            return relevantMergeRequests;
        }

        public Dictionary<string, string[]> GetCommenters(IMergeRequest request)
        {
            var webClient = GitLabWebClient(_gitlabApiToken);
            var notes = webClient.GetNotesForMergeRequest(request as GitLabMergeRequest);
            var awards = webClient.GetAwardsForMergeRequest(request as GitLabMergeRequest);

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

            markCommentAuthors["devs"] = markCommentAuthors["all"]; //.Except(Configuration.Instance.Value.Qas).ToArray();

            return markCommentAuthors;
        }

        public async Task<AdditionalProjectDetails> RetrieveAdditionalProjectDetails(string projectId, string commitHash)
        {
            var details = await GetFileContent<AdditionalProjectDetails>(projectId, commitHash, "Arekfile.json");
            return details;
        }

        public async Task<T> GetFileContent<T>(string projectId, string commitHash, string file)
        {
            var api = new Api(_gitlabUrl, _gitlabApiToken, Api.ApiVersion.V4);
            try
            {
                var fileUrl = $"/projects/{projectId}/repository/files/{Uri.EscapeDataString(file)}/raw?ref={commitHash}";
                return await Task.Run(() =>
                {
                    try
                    {
                        return api.Get().To<T>(fileUrl);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"File {file} not found for {projectId}, ref {commitHash}");
                        return default;
                    }
                });
            }
            catch (Exception)
            {
                Console.WriteLine($"File {file} not found for {projectId}, ref {commitHash}");
                return default;
            }
        }

        public static GitLab ConfiguredWith(Configuration configuration)
        {
            return new GitLab(configuration.GitLabUrl, configuration.GitlabApiToken, configuration.ProjectTeams);
        }
    }
}