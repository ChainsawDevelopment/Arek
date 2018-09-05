using System.Collections.Generic;
using System.Linq;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    public class StillDownvotedRequestRule : IMessageRule
    {
        public RequestMessage GetMessage(IMergeRequest request)
        {
            if (!request.IsOpened) return null;
            if (request.Downvotes > 0)
            {
                return new RequestMessage(request,
                    "Still :-1:?", 
                    new[] { request.Author.Username }.Concat(request.CommentAuthors["negative"]));
            }

            return null;
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests)
        {
            return ticketRequests.Select(GetMessage);
        }

        public class Factory : IRuleFactory<StillDownvotedRequestRule>
        {
            public StillDownvotedRequestRule Create(IDictionary<string, string> options) => new StillDownvotedRequestRule();
        }
    }
}