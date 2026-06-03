using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.Support.Domain.Model.Aggregate;

namespace Moveo_backend.Support.Domain.Repositories;

public interface ITicketMessageRepository : IBaseRepository<TicketMessage>
{
    Task<IEnumerable<TicketMessage>> FindByTicketIdAsync(int ticketId);
}
