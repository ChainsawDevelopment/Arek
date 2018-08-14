using System.Collections.Generic;
using System.Linq;
using GitLabNotifier.VCS;

namespace GitLabNotifier
{
    public class MissingReviewsRequestRule : IMessageRule
    {
        private readonly int _requiredVotesCount;

        public static MissingReviewsRequestRule ConfiguredWith(Configuration configuration)
        {
            return new MissingReviewsRequestRule(configuration.RequiredVotesCount);
        }

        public MissingReviewsRequestRule(int requiredVotesCount)
        {
            _requiredVotesCount = requiredVotesCount;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            var requiredVotes = _requiredVotesCount;
            if (request.Upvotes < requiredVotes && requiredVotes - request.CommentAuthors["devs"].Length - request.CommentAuthors["qas"].Length > 0)
            {
                var applicableDevs = request.Reviewers;                

                return new RequestMessage(request, 
                    $"{requiredVotes - request.CommentAuthors["devs"].Length} more review needed",
                    applicableDevs);
            }

            return null;
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            return ticketRequests.Select(GetMessage);
        }
    }
}