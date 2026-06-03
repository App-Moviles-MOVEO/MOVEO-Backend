using Moveo_backend.Notification.Domain.Model.Commands;

namespace Moveo_backend.Notification.Domain.Model.Aggregate;

public class Notification
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty; // rental_request, rental_accepted, payment_received, etc.
    public bool IsRead { get; private set; }
    public int? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; } // rental, payment, review, etc.
    public string? ActionUrl { get; private set; }
    public string? ActionLabel { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    protected Notification() { }

    public Notification(CreateNotificationCommand command)
    {
        UserId = command.UserId;
        Title = command.Title;
        Body = command.Body;
        Type = command.Type;
        IsRead = false;
        RelatedEntityId = command.RelatedEntityId;
        RelatedEntityType = command.RelatedEntityType;
        ActionUrl = command.ActionUrl;
        ActionLabel = command.ActionLabel;
        MetadataJson = command.MetadataJson;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        ReadAt = null;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
