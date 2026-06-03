namespace Moveo_backend.Notification.Interfaces.REST.Resources;

public record NotificationResource(
    int Id,
    int UserId,
    string Title,
    string Body,
    string Type,
    bool Read,
    int? RelatedId,
    string? RelatedType,
    string? ActionUrl,
    string? ActionLabel,
    object? Metadata,
    DateTime CreatedAt,
    DateTime? ReadAt
);
