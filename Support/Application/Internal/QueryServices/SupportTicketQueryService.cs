using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Queries;
using Moveo_backend.Support.Domain.Repositories;
using Moveo_backend.Support.Domain.Services;

namespace Moveo_backend.Support.Application.Internal.QueryServices;

public class SupportTicketQueryService(ISupportTicketRepository supportTicketRepository) : ISupportTicketQueryService
{
    public async Task<SupportTicket?> Handle(GetSupportTicketByIdQuery query)
    {
        return await supportTicketRepository.FindByIdWithMessagesAsync(query.Id);
    }

    public async Task<IEnumerable<SupportTicket>> Handle(GetAllSupportTicketsQuery query)
    {
        return await supportTicketRepository.ListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> Handle(GetSupportTicketsByUserIdQuery query)
    {
        return await supportTicketRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<IEnumerable<SupportTicket>> Handle(GetSupportTicketsByStatusQuery query)
    {
        return await supportTicketRepository.FindByStatusAsync(query.Status);
    }
}
