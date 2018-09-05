using System.Collections.Generic;

namespace Arek.Contracts
{
    public interface IMessageRule
    {
        IEnumerable<IMessage> GetMessages(TicketDetails ticket, IEnumerable<IMergeRequest> ticketRequests);
    }
}