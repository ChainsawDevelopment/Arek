using System.Collections.Generic;

namespace GitLabNotifier.VCS
{
    public class TicketDetails
    {
        public string Status { get; set; }
        public IEnumerable<string> Labels { get; set; } = new string[] { };
        public string Url { get; set; }
        public string Id { get; set; }

        public TicketDetails(string id)
        {
            Id = id;
        }

        protected bool Equals(TicketDetails other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TicketDetails) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }

    public class ProjectDetails
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string[] PrimaryReviewers { get; set; }
        public string[] SecondaryReviewers { get; set; }
    }
}