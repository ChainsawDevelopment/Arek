using System;
using System.Collections.Generic;
using System.Linq;

namespace GitLabNotifier.VCS
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
        
        ProjectDetails ProjectDetails { get; set; }
        List<IMessageRule> Rules { get; }
    }

    public static class MergeRequestExtensions
    {
        public static void ApplyAdditionalDetails(this IMergeRequest request, AdditionalProjectDetails additionalProjectDetails)
        {
            request.ProjectDetails = request.ProjectDetails.TypedClone();
            request.ProjectDetails.AddDataFrom(additionalProjectDetails);
        }

        public static void FillDefaultValues(this IMergeRequest request, List<IMessageRule> rules)
        {
            request.Rules.Clear();
            request.Rules.AddRange(rules);
        }

        public static IEnumerable<IMessage> GenerateMessages(this IEnumerable<IMergeRequest> ticketRequests)
        {
            var mergeRequests = ticketRequests as IMergeRequest[] ?? ticketRequests.ToArray();
            return mergeRequests
                .SelectMany(request => request.GenereteMessages(mergeRequests))
                .Distinct();
        }

        public static IEnumerable<IMessage> GenereteMessages(this IMergeRequest request, IEnumerable<IMergeRequest> allTicketRequests)
        {
            return request.Rules.SelectMany(rule => rule.GetMessages(request.TicketDetails, allTicketRequests));
        }
    }
}