using System;
using System.Collections.Generic;
using System.Linq;
using Arek.Contracts;

namespace Arek.Engine.Rules
{
    public class OldRequestRule : IMessageRule
    {
        private readonly int[] _oldRequestThresholdsDays;
        private readonly int _requiredVotes;

        public OldRequestRule(int[] oldRequestThresholdsDays, int requiredVotes)
        {
            _oldRequestThresholdsDays = oldRequestThresholdsDays;
            _requiredVotes = requiredVotes;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
            if (!request.IsOpened) return null;
            var businessDays = request.CreatedAt.GetBusinessDaysTo(DateTime.Now);
            var isAboveThresholdForNotAcceptedRequests = businessDays > _oldRequestThresholdsDays[0];
            var isAboveThresholdForAllRequests = businessDays > _oldRequestThresholdsDays[1];

            if ((request.Upvotes < _requiredVotes && isAboveThresholdForNotAcceptedRequests) || isAboveThresholdForAllRequests)
            {
                return new RequestMessage(
                    request,
                    $"Created {(int)businessDays} days ago" + (request.TicketDetails.Status != "" ? $"with status {request.TicketDetails.Status}" : ""), 
                    new[] { request.Author.Username });
            }

            return null;
        }

        public IEnumerable<IMessage> GetMessages(TicketDetails ticketKey, IEnumerable<IMergeRequest> ticketRequests)
        {
            return ticketRequests.Select(GetMessage);
        }

        public class Factory : IRuleFactory<OldRequestRule>
        {
            public OldRequestRule Create(IDictionary<string, string> options)
            {
                return new OldRequestRule(
                    ReadOldRequestThresholds(options),
                    options["RequiredVotesCount"].ToInt());
            }

            private static int[] ReadOldRequestThresholds(IDictionary<string, string> options) => options["OldRequestThresholdsDays"]
                .Split(',')
                .Select(s => s.ToInt())
                .ToArray();
        }
    }
}