using System;
using System.Collections.Generic;
using System.Linq;

namespace Arek.Contracts
{
    public interface IMergeRequest
    {
        int Id { get; }
        string Iid { get; }
        string Title { get; }
        string Project { get;  }
        IUser Author { get; }
        IEnumerable<string> Reviewers { get; set; }
        DateTime CreatedAt { get; }
        int Downvotes { get; }
        int Upvotes { get; }
        string Url { get; }
        string TicketID { get; set; }
        string GetPrefferedAssignment(List<string> except);
        TicketDetails TicketDetails { get; set; }
        Dictionary<string, string[]> CommentAuthors { get; set; }
        string HeadHash { get; }
        bool IsOpened { get; }

        ProjectDetails ProjectDetails { get; set; }
        List<IMessageRule> Rules { get; }
    }

    public static class MergeRequestExtensions
    {
        public static void ApplyAdditionalDetails(this IMergeRequest request, AdditionalProjectDetails additionalProjectDetails)
        {
            if (additionalProjectDetails == null)
            {
                return;
            }

            request.ProjectDetails = request.ProjectDetails.TypedClone();
            request.ProjectDetails.AddDataFrom(additionalProjectDetails);

            var newRules = additionalProjectDetails.Rules?.Select(kvp => Rule.For(kvp.Key, kvp.Value))?.ToList() ?? new List<IMessageRule>();
            if (newRules.Any())
            {
                request.SetRules(newRules);
            }
        }

        public static void SetRules(this IMergeRequest request, List<IMessageRule> rules)
        {
            request.Rules.Clear();
            request.Rules.AddRange(rules);
        }

        public static IEnumerable<IMessage> GenerateMessages(this IEnumerable<IMergeRequest> ticketRequests)
        {
            var mergeRequests = ticketRequests as IMergeRequest[] ?? ticketRequests.ToArray();
            return mergeRequests
                .SelectMany(request => request.GenereteMessages(mergeRequests));
        }

        public static IEnumerable<IMessage> GenereteMessages(this IMergeRequest request, IEnumerable<IMergeRequest> allTicketRequests)
        {
            return request.Rules.SelectMany(rule => rule.GetMessages(request.TicketDetails, allTicketRequests));
        }
    }
}