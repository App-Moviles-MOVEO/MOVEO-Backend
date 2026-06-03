namespace Moveo_backend.Support.Interfaces.REST.Resources;

public record TicketMessageResource(
    int Id,
    int TicketId,
    int SenderId,
    string Message,
    bool IsStaffReply,
    DateTime CreatedAt
);
