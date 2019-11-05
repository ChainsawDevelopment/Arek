using System.Collections.Generic;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    class TicketMessage : IMessage
    {
        public string LinkUrl => Ticket.Url;
        public string LinkText => Ticket.Id;

        public TicketMessage(IMergeRequest request, string message, IEnumerable<string> recipients)
        {
            Request = request;
            Ticket = request.TicketDetails;
            Message = message;
            Recipients = recipients;
        }
        
        public TicketDetails Ticket { get; }
        public string Message { get; }
        public IEnumerable<string> Recipients { get; }
        public IMergeRequest Request { get; }
    }
}