using Moveo_backend.Support.Domain.Model.Commands;

namespace Moveo_backend.Support.Domain.Model.Aggregate;

public class TicketMessage
{
    public int Id { get; private set; }
    public int TicketId { get; private set; }
    public int SenderId { get; private set; }
    public string Message { get; private set; }
    public bool IsStaffReply { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation property
    public SupportTicket? Ticket { get; private set; }

    protected TicketMessage() 
    {
        Message = string.Empty;
    }

    public TicketMessage(CreateTicketMessageCommand command)
    {
        TicketId = command.TicketId;
        SenderId = command.SenderId;
        Message = command.Message;
        IsStaffReply = command.IsStaffReply;
        CreatedAt = DateTime.UtcNow;
    }
}
