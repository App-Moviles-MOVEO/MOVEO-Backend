using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Domain.Model.Commands;

namespace Moveo_backend.Support.Domain.Services;

public interface ISupportTicketCommandService
{
    Task<SupportTicket?> Handle(CreateSupportTicketCommand command);
    Task<SupportTicket?> Handle(UpdateSupportTicketCommand command);
    Task<SupportTicket?> Handle(CloseSupportTicketCommand command);
    Task<bool> Handle(DeleteSupportTicketCommand command);
}
