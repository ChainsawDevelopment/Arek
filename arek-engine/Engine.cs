using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Arek.Contracts;

namespace Arek.Engine
{
    class PhonyTicketSystem : ITicketSystem
    {
        public Task<TicketDetails> GetTicketStatus(IMergeRequest request)
        {
            return Task.FromResult(new TicketDetails(request.Title)
            {
                Labels = new string[] { },
                Status = "",
                Url = ""
            });
        }
    }

    public class Engine
    {
        private ITicketSystem _ticketSystem = new PhonyTicketSystem();
        private readonly List<IGitServer> _gitServers = new List<IGitServer>();
        private readonly List<IMessageRule> _defaultRules = new List<IMessageRule>();
        private readonly List<IReviewerAssignStrategy> _reviewerAssigners = new List<IReviewerAssignStrategy>();

        static Engine()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        public async Task UpdateMergeRequestsWithArekfile(IMergeRequest[] mergeRequests)
        {
            foreach (var mergeRequest in mergeRequests)
            {
                var additionalDetails = await _gitServers.First().RetrieveAdditionalProjectDetails(mergeRequest.Project, mergeRequest.HeadHash);
                mergeRequest.ApplyAdditionalDetails(additionalDetails);
            }
        }

        public async Task<IEnumerable<IMessage>> GenerateMessages(string gitlabToken = null)
        {
            var configuration = Configuration.Instance.Value;
            configuration.ReloadDevs();

            IMergeRequest[] mergeRequests = await LoadMergeRequests();

            // Fill in defaults
            foreach (var mergeRequest in mergeRequests)
            {
                mergeRequest.SetRules(_defaultRules);
            }

            await UpdateMergeRequestsWithArekfile(mergeRequests);

            foreach (var reviewerAssigner in _reviewerAssigners)
            {
                reviewerAssigner.AssignReviewers(mergeRequests);
            }

            var outputMessages = mergeRequests.GenerateMessages()
                    .Where(message => message != null)
                    .OrderBy(x => x.Request.Project)
                    .ThenBy(x => x.Request.TicketDetails.Id)
                    .ToArray();

            return outputMessages;
        }

        public Engine Using(IGitServer gitServer)
        {
            _gitServers.Add(gitServer);
            return this;
        }

        public Engine Using(ITicketSystem ticketSystem)
        {
            _ticketSystem = ticketSystem;
            return this;
        }

        public Engine Using(IMessageRule rule)
        {
            _defaultRules.Add(rule);
            return this;
        }

        public Engine Using(params IMessageRule[] rules)
        {
            _defaultRules.AddRange(rules);
            return this;
        }

        public Engine Using(IReviewerAssignStrategy reviewerAssignStrategy)
        {
            _reviewerAssigners.Add(reviewerAssignStrategy);
            return this;
        }

        private async Task<IMergeRequest[]> LoadMergeRequests()
        {
            var gitServer = _gitServers.First();

            var mergeRequestsTasks = Configuration.Instance.Value.GitProjects
                .Select(project => gitServer.GetOpenMergeRequests(project))
                .ToArray();

            var ticketDetailsTasks = (await Task.WhenAll(mergeRequestsTasks))
                .SelectMany(requests => requests)
                .Select(mr => _ticketSystem.GetTicketStatus(mr)
                    .ContinueWith(
                        ticketDetails =>
                        {
                            mr.TicketDetails = ticketDetails.Result;
                            mr.TicketID = ticketDetails.Result.Id;
                            mr.CommentAuthors = gitServer.GetCommenters(mr);
                            return mr;
                        }));

            var mergeRequests = (await Task.WhenAll(ticketDetailsTasks))
                .OrderBy(request => request.TicketID)
                .ToArray();

            return mergeRequests;
        }
    }
}
