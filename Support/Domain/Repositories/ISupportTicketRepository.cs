using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.Support.Domain.Model.Aggregate;

namespace Moveo_backend.Support.Domain.Repositories;

public interface ISupportTicketRepository : IBaseRepository<SupportTicket>
{
    Task<IEnumerable<SupportTicket>> FindByUserIdAsync(int userId);
    Task<IEnumerable<SupportTicket>> FindByStatusAsync(string status);
    Task<SupportTicket?> FindByIdWithMessagesAsync(int id);
}
