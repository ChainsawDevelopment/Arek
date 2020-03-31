using System;
using System.Collections.Generic;
using System.Linq;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    public class NotMergedRequestRule : IMessageRule
    {
        private readonly int _requiredVotes;

        public NotMergedRequestRule(int requiredVotes)
        {
            _requiredVotes = requiredVotes;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            if (!request.IsOpened) return null;
            if (request.Upvotes >= _requiredVotes && request.Downvotes == 0)
            {
                string[] notifiedUsers = GetNotifiedUsers(request);
                return new RequestMessage(
                    request,
                    $"Approved but still not merged", notifiedUsers);
            }

            return null;
        }

        private string[] GetNotifiedUsers(IMergeRequest request)
        {
            var verifier = request.CommentAuthors["verifier"]?.FirstOrDefault();

            if (string.IsNullOrEmpty(verifier))
            {
                return request.CommentAuthors["all"];
            }
            else
            {
                return new[] { verifier };
            }
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticketKey, IEnumerable<IMergeRequest> ticketRequests)
        {
            return ticketRequests.Select(GetMessage);
        }

        public class Factory : IRuleFactory<NotMergedRequestRule>
        {
            public NotMergedRequestRule Create(IDictionary<string, string> options)
            {
                return new NotMergedRequestRule(options["RequiredVotesCount"].ToInt());
            }
        }
    }
}
