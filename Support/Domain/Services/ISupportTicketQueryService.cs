using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Queries;

namespace Moveo_backend.Support.Domain.Services;

public interface ISupportTicketQueryService
{
    Task<SupportTicket?> Handle(GetSupportTicketByIdQuery query);
    Task<IEnumerable<SupportTicket>> Handle(GetAllSupportTicketsQuery query);
    Task<IEnumerable<SupportTicket>> Handle(GetSupportTicketsByUserIdQuery query);
    Task<IEnumerable<SupportTicket>> Handle(GetSupportTicketsByStatusQuery query);
}
