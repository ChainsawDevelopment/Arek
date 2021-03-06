﻿using System;
using System.Collections.Generic;
using System.Linq;
using Arek.Contracts;
using NGitLab.Models;

namespace Arek.GitLab
{
    public class GitLabMergeRequest : IMergeRequest
    {
        public string Project { get; }
        private readonly ExtendedMergeRequest _wrapped;
        private readonly string _baseUrl;

        public GitLabMergeRequest(string project, ExtendedMergeRequest wrapped, ProjectDetails[] allProjects, string baseUrl)
        {
            Project = project;
            _wrapped = wrapped;
            _baseUrl = baseUrl;
            ProjectDetails = allProjects.FirstOrDefault(p => p.Name == wrapped.ProjectId.ToString()) ?? new ProjectDetails();
        }

        public int Id => _wrapped.Id;
        public string Iid => _wrapped.Iid.ToString();
        public string Title => _wrapped.Title;
        public IUser Assignee => new GitLabUser(_wrapped.Assignee);
        public IUser Author => new GitLabUser(_wrapped.Author);
        public IEnumerable<string> Reviewers { get; set; }
        public DateTime CreatedAt => _wrapped.CreatedAt;
        public string Description => _wrapped.Description;
        public int Downvotes => _wrapped.Downvotes;
        public int Upvotes => _wrapped.Upvotes;
        public string Url => _wrapped.WebUrl;
        public string TicketID { get; set; }
        public string GetPrefferedAssignment(List<string> applicableUsers) => null;
        public TicketDetails TicketDetails { get; set; }
        public Dictionary<string, string[]> CommentAuthors { get; set; }
        public ProjectDetails ProjectDetails { get; set; }
        public List<IMessageRule> Rules { get; } = new List<IMessageRule>();
        public string HeadHash => _wrapped.Sha;

        public bool IsOpened => _wrapped.State == MergeRequestState.opened || _wrapped.State == MergeRequestState.reopened;

        public string TargetBranch => _wrapped.TargetBranch;
        public string SourceBranch => _wrapped.SourceBranch;
        public int ProjectId => _wrapped.ProjectId;
        public int SourceProjectId => _wrapped.SourceProjectId;
        public int TargetProjectId => _wrapped.TargetProjectId;

        public string SourceBranchName => SourceBranch;

        protected bool Equals(GitLabMergeRequest other)
        {
            return string.Equals(Url, other.Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GitLabMergeRequest) obj);
        }

        public override int GetHashCode()
        {
            return (Url != null ? Url.GetHashCode() : 0);
        }
    }
}