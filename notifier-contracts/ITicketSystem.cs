using System.Threading.Tasks;

namespace GitLabNotifier.VCS
{
    public interface ITicketSystem
    {
        Task<TicketDetails> GetTicketStatus(IMergeRequest request);
    }
}