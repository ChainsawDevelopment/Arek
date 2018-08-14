using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GitLabNotifier.VCS.Bitbucket
{
    public class Bitbucket : IGitServer
    {
        private readonly ProjectDetails[] _allProjects;
        private readonly BitBucketApiClient _bitBucketApiClient;
        private readonly string _teamUuid;

        public Bitbucket(string gitUrl, string gitApiToken, ProjectDetails[] allProjects, string teamUuid)
        {
            _teamUuid = teamUuid;
            _allProjects = allProjects ?? new ProjectDetails[0];
            _bitBucketApiClient = new BitBucketApiClient(gitUrl, gitApiToken);
        }

        public async Task<IMergeRequest[]> GetOpenMergeRequests(string projectName)
        {
            Console.WriteLine($"Getting open merge requests for {projectName}");

            var project = await _bitBucketApiClient.GetAsync($"repositories/{_teamUuid}/{projectName}");
            var projectUuid = (string)project["uuid"];

            var pullrequests = await _bitBucketApiClient.GetAsync($"repositories/{_teamUuid}/{projectUuid}/pullrequests?q=state=\"OPEN\"");
            var bitBucketPullRequests = pullrequests["values"].Select(async pr => (IMergeRequest)new BitBucketPullRequest
            {
                Project = projectName,
                ProjectDetails = _allProjects.FirstOrDefault(pt => pt.Name == projectName),
                Id = (int)pr["id"],
                Iid = projectName + (int)pr["id"],
                Title = (string)pr["title"],
                Author = new BitBucketUser() { Username = (string)pr["author"]["username"] },
                CreatedAt = (DateTime)pr["created_on"],
                Participants = await GetPullRequestPariticipants(projectUuid, _teamUuid, (int)pr["id"]),
                CommentsAuthors = GetPullRequestCommentAuthors(projectUuid, _teamUuid, (int)pr["id"]),
                Url = (string)pr["links"]["html"]["href"]
            }).ToArray();

            return await Task.WhenAll(bitBucketPullRequests).ContinueWith(requests =>
            {
                Console.WriteLine($"Finished getting open merge requests for {projectName}");
                return requests.Result;
            });
        }

        private IEnumerable<string> GetPullRequestCommentAuthors(string projectUuid, string teamUuid, int prId)
        {
            var prComments = _bitBucketApiClient.GetAsync($"repositories/{teamUuid}/{projectUuid}/pullrequests/{prId}/comments").Result;
            return prComments["values"].Where(comment => comment["user"].Type != JTokenType.Null).Select(comment => (string)comment["user"]["username"]).ToArray();
        }

        private async Task<IEnumerable<Participant>> GetPullRequestPariticipants(string projectUuid, string teamUuid, int prId)
        {
            var prDetails = await _bitBucketApiClient.GetAsync($"repositories/{teamUuid}/{projectUuid}/pullrequests/{prId}");
            return prDetails["participants"].Select(participant =>
                new Participant
                {
                    Approved = (bool)participant["approved"],
                    Username = (string)participant["user"]["username"]
                }).ToArray();
        }

        public Dictionary<string, string[]> GetCommenters(IMergeRequest request)
        {
            var bitBucketPullRequest = request as BitBucketPullRequest;

            var approvals = bitBucketPullRequest?.Participants
                            .Where(p => p.Approved)
                            .Select(p => p.Username)
                            .ToArray() ?? new string[] { };

            var allCommenters = bitBucketPullRequest
                .CommentsAuthors
                .Concat(approvals)
                .Except(new[] { request.Author.Username })
                .Distinct()
                .ToArray();

            var markCommentAuthors = new Dictionary<string, string[]>
            {
                ["all"] = allCommenters.ToArray(),
                ["devs"] = allCommenters.ToArray(),
                ["qas"] = new string[] { }
            };

            markCommentAuthors["allNotesAuthors"] = allCommenters.ToArray();

            markCommentAuthors["negative"] = bitBucketPullRequest.Participants
                .Where(p => !p.Approved && bitBucketPullRequest.CommentsAuthors.Contains(p.Username) && p.Username != bitBucketPullRequest.Author.Username)
                .Select(p => p.Username)
                .ToArray();

            markCommentAuthors["positive"] = approvals.ToArray();

            return markCommentAuthors;
        }

        public static Bitbucket ConfiguredWith(Configuration configuration)
        {
            return new Bitbucket("https://api.bitbucket.org", configuration.GitlabApiToken, configuration.ProjectTeams, configuration.BitbucketTeamUuid);
        }
    }
}