using System.Collections.Generic;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    class TicketMessage : IMessage
    {
        public string LinkUrl => Ticket.Url;
        public string LinkText => Ticket.Id;

        public TicketMessage(TicketDetails ticket, string message, IEnumerable<string> receipents)
        {
            Ticket = ticket;
            Message = message;
            Receipents = receipents;
        }
        
        public TicketDetails Ticket { get; }
        public string Message { get; }
        public IEnumerable<string> Receipents { get; }
    }
}