namespace Arek.Contracts
{
    public interface IReviewerAssignStrategy
    {
        void AssignReviewers(IMergeRequest[] mergeRequests);
    }
}