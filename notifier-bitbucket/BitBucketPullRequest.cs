using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GitLabNotifier.VCS.Bitbucket
{
    public class BitBucketPullRequest : IMergeRequest
    {
        public int Id { get; set; }
        public string Iid { get; set; }
        public string Title { get; set; }
        public string Project { get; set;  }
        public IUser Author { get; set; }
        public IEnumerable<string> Reviewers { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Downvotes => Participants.Count(p => !p.Approved && CommentsAuthors.Contains(p.Username) && p.Username != Author.Username);
        public int Upvotes => Participants.Count(p => p.Approved);
        public IEnumerable<Participant> Participants { get; set; }
        public string Url { get; set; }
        public string TicketID { get; set; }
        public string GetPrefferedAssignment(List<string> applicableUsers) => Participants.Where(p => !p.Approved)
            .Select(p => p.Username)
            .Except(applicableUsers)
            .Randomize()
            .FirstOrDefault();

        public TicketDetails TicketDetails { get; set; }
        public Dictionary<string, string[]> CommentAuthors { get; set; }

        public IEnumerable<string> CommentsAuthors { get; set; }
        public ProjectDetails ProjectDetails { get; set; }
        public string HeadHash { get; set; }
        public void ApplyAdditionalDetails(AdditionalProjectDetails additionalProjectDetails)
        {
            throw new NotImplementedException();
        }
    }

    static class EnumerableHelper
    {
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            return source.OrderBy(_ => random.Next());
        }
    }
}