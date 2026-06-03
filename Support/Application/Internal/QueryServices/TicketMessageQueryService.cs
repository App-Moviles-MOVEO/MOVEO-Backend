using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Queries;
using Moveo_backend.Support.Domain.Repositories;
using Moveo_backend.Support.Domain.Services;

namespace Moveo_backend.Support.Application.Internal.QueryServices;

public class TicketMessageQueryService(ITicketMessageRepository ticketMessageRepository) : ITicketMessageQueryService
{
    public async Task<IEnumerable<TicketMessage>> Handle(GetTicketMessagesQuery query)
    {
        return await ticketMessageRepository.FindByTicketIdAsync(query.TicketId);
    }
}
