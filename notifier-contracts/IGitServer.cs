using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitLabNotifier.VCS
{
    public interface IGitServer
    {
        Task<IMergeRequest[]> GetOpenMergeRequests(string projectId);
        Dictionary<string, string[]> GetCommenters(IMergeRequest request);
    }
}