namespace Moveo_backend.Support.Domain.Model.Commands;

public record CreateTicketMessageCommand(
    int TicketId,
    int SenderId,
    string Message,
    bool IsStaffReply
);
