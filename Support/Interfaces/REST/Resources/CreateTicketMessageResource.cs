namespace Moveo_backend.Support.Interfaces.REST.Resources;

public record CreateTicketMessageResource(
    int SenderId,
    string Message,
    bool IsStaffReply
);
