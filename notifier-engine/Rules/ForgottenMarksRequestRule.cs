using System.Collections.Generic;
using System.Linq;
using GitLabNotifier.VCS;

namespace GitLabNotifier
{
    public class ForgottenMarksRequestRule : IMessageRule
    {
        private readonly int _requiredVotes;

        public ForgottenMarksRequestRule(int requiredVotes)
        {
            _requiredVotes = requiredVotes;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            if (!request.IsOpened) return null;
            if (request.Upvotes >= _requiredVotes) return null;

            var markReminderAuthors = request.CommentAuthors["allNotesAuthors"].Except(request.CommentAuthors["all"])
                .Except(new[] { request.Author.Username.ToLower() })
                .ToArray();

            return markReminderAuthors.Any() ? new RequestMessage(request, "Forgot mark?", markReminderAuthors) : null;
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            return ticketRequests.Select(GetMessage);
        }

        public class Factory : IRuleFactory<ForgottenMarksRequestRule>
        {
            public ForgottenMarksRequestRule Create(IDictionary<string, string> options)
            {
                return new ForgottenMarksRequestRule(options["RequiredVotesCount"].ToInt());
            }
        }
    }
}