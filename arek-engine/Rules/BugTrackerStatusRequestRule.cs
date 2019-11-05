using System.Collections.Generic;
using System.Linq;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    public class BugTrackerStatusRequestRule : IMessageRule
    {
        private readonly int _requiredVotesCount;
        private readonly HashSet<string> _acceptableStatuses;
        private readonly HashSet<string> _closedStatuses;

        public BugTrackerStatusRequestRule(int requiredVotesCount, string[] acceptableStatuses, string[] closedStatuses)
        {
            _requiredVotesCount = requiredVotesCount;
            _acceptableStatuses = new HashSet<string>(acceptableStatuses);
            _closedStatuses = new HashSet<string>(closedStatuses);
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            if (!request.IsOpened)
            {
                if (!string.IsNullOrEmpty(request.TicketDetails.Status) &&! _closedStatuses.Contains(request.TicketDetails.Status))
                {
                    return new RequestMessage(request, $"Merge request closed but Issue has status \"{request.TicketDetails.Status}\"?", new[] { request.Author.Username });
                }
                else
                {
                    return null;
                }
            }

            var markCommentAuthors = request.CommentAuthors;

            if (string.IsNullOrEmpty(request.TicketDetails.Status))
            {
                return new RequestMessage(request, "Missing ticket status", new[] { request.Author.Username });
            }

            if (_acceptableStatuses.Contains(request.TicketDetails.Status))
            {
                return null;
            }

            
            if (_closedStatuses.Contains(request.TicketDetails.Status) )
            {
                return new RequestMessage(request, "Issue closed but request not merged?", new[] { request.Author.Username }.Concat(markCommentAuthors["all"]));
            }

            if (request.Upvotes < _requiredVotesCount || request.Downvotes > 0)
            {
                return new RequestMessage(request, $"Review in progress but issue has status \"{request.TicketDetails.Status}\"?", new[] { request.Author.Username }.Concat(markCommentAuthors["all"]));
            }

            return null;
        }

        private TicketMessage GetAllRequestsApprovedMessage(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            var mergeRequests = ticketRequests as IMergeRequest[] ?? ticketRequests.ToArray();
            if (mergeRequests.All(request =>
                _requiredVotesCount - request.Upvotes <= 0 && _acceptableStatuses.Contains(request.TicketDetails.Status)))
            {
                return new TicketMessage(mergeRequests.First(), $"All requests approved but still in {ticket.Status}?",
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
                return new BugTrackerStatusRequestRule(
                    options["RequiredVotesCount"].ToInt(),
                    options["AcceptableStatuses"].Split(','),
                    options["ClosedStatuses"].Split(',')
                    );
            }
        }
    }
}