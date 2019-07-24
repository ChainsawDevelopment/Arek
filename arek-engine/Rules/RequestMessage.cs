using System.Collections.Generic;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    public class RequestMessage : IMessage
    {
        public IMergeRequest Request { get; }
        public string LinkText => $"{Request.Title ?? string.Empty} ({Request.ProjectDetails?.ShortName ?? "UNK"}, {Request.TicketID ?? "no id"})"
        .Trim();
        public string LinkUrl => Request.Url;
        public string Message { get; }
        public IEnumerable<string> Receipents { get; }

        public RequestMessage(IMergeRequest request, string message, IEnumerable<string> receipents)
        {
            Request = request;
            Message = message;
            Receipents = receipents;
        }
    }
}