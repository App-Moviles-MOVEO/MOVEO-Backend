using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Interfaces.REST.Resources;

namespace Moveo_backend.Support.Interfaces.REST.Transform;

public static class TicketMessageResourceFromEntityAssembler
{
    public static TicketMessageResource ToResourceFromEntity(TicketMessage entity)
    {
        return new TicketMessageResource(
            entity.Id,
            entity.TicketId,
            entity.SenderId,
            entity.Message,
            entity.IsStaffReply,
            entity.CreatedAt
        );
    }
}
