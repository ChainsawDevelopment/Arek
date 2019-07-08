using System.Collections.Generic;
using System.Linq;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    public class MissingReviewsRequestRule : IMessageRule
    {
        private readonly int _requiredVotesCount;

        public MissingReviewsRequestRule(int requiredVotesCount)
        {
            _requiredVotesCount = requiredVotesCount;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            if (!request.IsOpened) return null;
            if (request.Upvotes < _requiredVotesCount && _requiredVotesCount - request.CommentAuthors["devs"].Length > 0)
            {
                return new RequestMessage(request, 
                    $"{_requiredVotesCount - request.CommentAuthors["devs"].Length} more review needed",
                    request.Reviewers);
            }

            return null;
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            return ticketRequests.Select(GetMessage);
        }

        public class Factory : IRuleFactory<MissingReviewsRequestRule>
        {
            public MissingReviewsRequestRule Create(IDictionary<string, string> options)
            {
                return new MissingReviewsRequestRule(options["RequiredVotesCount"].ToInt());
            }
        }
    }
}