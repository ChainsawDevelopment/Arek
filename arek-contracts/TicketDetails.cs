using System;
using System.Collections.Generic;

namespace Arek.Contracts
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
    
    public interface IProjectDetails
    {
        string PrimaryReviewers { get; set; }
        string SecondaryReviewers { get; set; }
    }

    public class AdditionalProjectDetails : IProjectDetails
    {
        public string PrimaryReviewers { get; set; }
        public string SecondaryReviewers { get; set; }
        public Dictionary<string, Dictionary<string, string>> Rules { get; set; }
    }

    public class ProjectDetails : ICloneable
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string[] PrimaryReviewers { get; set; }
        public string[] SecondaryReviewers { get; set; }

        public void AddDataFrom(IProjectDetails otherProjectDetails)
        {
            if (otherProjectDetails == null) return;

            PrimaryReviewers = NullIfEmpty(otherProjectDetails.PrimaryReviewers)?.Split(',') ?? this.PrimaryReviewers;
            SecondaryReviewers = NullIfEmpty(otherProjectDetails.SecondaryReviewers)?.Split(',') ?? this.SecondaryReviewers;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        
        public ProjectDetails TypedClone()
        {
            return MemberwiseClone() as ProjectDetails;
        }

        private string NullIfEmpty(string str) => string.IsNullOrEmpty(str) ? null : str;
    }
}