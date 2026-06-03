using Moveo_backend.Support.Domain.Model.Commands;
using Moveo_backend.Support.Interfaces.REST.Resources;

namespace Moveo_backend.Support.Interfaces.REST.Transform;

public static class CreateTicketMessageCommandFromResourceAssembler
{
    public static CreateTicketMessageCommand ToCommandFromResource(int ticketId, CreateTicketMessageResource resource)
    {
        return new CreateTicketMessageCommand(
            ticketId,
            resource.SenderId,
            resource.Message,
            resource.IsStaffReply
        );
    }
}
