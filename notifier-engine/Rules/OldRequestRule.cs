using System;
using System.Collections.Generic;
using System.Linq;
using GitLabNotifier.VCS;

namespace GitLabNotifier
{
    public class OldRequestRule : IMessageRule
    {
        private readonly int[] _oldRequestThresholdsDays;
        private readonly int _requiredVotes;

        public static OldRequestRule ConfiguredWith(Configuration configuration)
        {
            return new OldRequestRule(configuration.OldRequestThresholdsDays, configuration.RequiredVotesCount);
        }

        public OldRequestRule(int[] oldRequestThresholdsDays, int requiredVotes)
        {
            _oldRequestThresholdsDays = oldRequestThresholdsDays;
            _requiredVotes = requiredVotes;
        }

        public RequestMessage GetMessage(IMergeRequest request)
        {
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
    }
}