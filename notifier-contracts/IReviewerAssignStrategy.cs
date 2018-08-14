using GitLabNotifier.VCS;

namespace GitLabNotifier
{
    public interface IReviewerAssignStrategy
    {
        void AssignReviewers(IMergeRequest[] mergeRequests);
    }
}