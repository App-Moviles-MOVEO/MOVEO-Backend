using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Commands;

namespace Moveo_backend.Support.Domain.Services;

public interface ITicketMessageCommandService
{
    Task<TicketMessage?> Handle(CreateTicketMessageCommand command);
}
