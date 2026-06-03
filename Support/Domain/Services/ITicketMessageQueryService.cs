using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Queries;

namespace Moveo_backend.Support.Domain.Services;

public interface ITicketMessageQueryService
{
    Task<IEnumerable<TicketMessage>> Handle(GetTicketMessagesQuery query);
}
