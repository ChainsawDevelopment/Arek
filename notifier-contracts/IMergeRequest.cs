using System;
using System.Collections.Generic;

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
        ProjectDetails ProjectDetails { get; }
        string HeadHash { get; }
        void ApplyAdditionalDetails(AdditionalProjectDetails additionalProjectDetails);
    }
}