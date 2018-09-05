using System.Threading.Tasks;

namespace Arek.Contracts
{
    public interface ITicketSystem
    {
        Task<TicketDetails> GetTicketStatus(IMergeRequest request);
    }
}