using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arek.Contracts
{
    public interface IGitServer
    {
        Task<AdditionalProjectDetails> RetrieveAdditionalProjectDetails(string projectId, string commitHash);
        Task<IMergeRequest[]> GetOpenMergeRequests(string projectId);
        Dictionary<string, string[]> GetCommenters(IMergeRequest request);
    }
}