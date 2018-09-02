using System.Collections.Generic;
using System.Linq;
using GitLabNotifier.VCS;

namespace GitLabNotifier
{
    public class BugTrackerStatusRequestRule : IMessageRule
    {
        private readonly int _requiredVotesCount;
        private readonly string _codeReviewStatus;
        
        public BugTrackerStatusRequestRule(int requiredVotesCount, string codeReviewStatus)
        {
            _requiredVotesCount = requiredVotesCount;
            _codeReviewStatus = codeReviewStatus;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            var markCommentAuthors = request.CommentAuthors;

            if (string.IsNullOrEmpty(request.TicketDetails.Status))
            {
                return new RequestMessage(request, "Missing ticket status", new[] {request.Author.Username});
            }

            switch (request.TicketDetails.Status)
            {
                case "Code Review":
                    break;
                case "In Progress":
                    break;
                case "Testing":
                    break;
                case "Done":
                    return new RequestMessage(request, "Issue closed but request not merged?", new[] { request.Author.Username }.Concat(markCommentAuthors["all"]));
                case "":
                    break;
                default:
                    if (request.Upvotes < _requiredVotesCount || request.Downvotes > 0)
                    {
                        return new RequestMessage(request, $"Not reviewed but {request.TicketDetails.Status}?", new[] { request.Author.Username }.Concat(markCommentAuthors["all"]));
                    }
                    break;
            }

            return null;
        }

        private TicketMessage GetAllRequestsApprovedMessage(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            var mergeRequests = ticketRequests as IMergeRequest[] ?? ticketRequests.ToArray();
            if (mergeRequests.All(request =>
                _requiredVotesCount - request.Upvotes <= 0 && request.TicketDetails.Status == _codeReviewStatus))
            {
                return new TicketMessage(ticket, $"All requests approved but still in {_codeReviewStatus}",
                    mergeRequests.Select(request => request.Author.Username).Distinct());
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            var mergeRequests = ticketRequests as IMergeRequest[] ?? ticketRequests.ToArray();
            return mergeRequests.Select(GetMessage).Concat(new[]
            {
                GetAllRequestsApprovedMessage(ticket, mergeRequests) as IMessage
            });
        }

        public IEnumerable<IMessage> GetMessages(IMergeRequest request)
        {
            return new IMessage[]
            {
                GetMessage(request),
                GetAllRequestsApprovedMessage(request.TicketDetails, new[] {request})
            };
        }

        public class Factory : IRuleFactory<BugTrackerStatusRequestRule>
        {
            public BugTrackerStatusRequestRule Create(IDictionary<string, string> options)
            {
                return new BugTrackerStatusRequestRule(options["RequiredVotesCount"].ToInt(), "Code Review");
            }
        }
    }
}