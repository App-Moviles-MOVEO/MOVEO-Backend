namespace Moveo_backend.Notification.Domain.Model.Commands;

public record CreateNotificationCommand(
    int UserId,
    string Title,
    string Body,
    string Type,
    int? RelatedEntityId,
    string? RelatedEntityType,
    string? ActionUrl,
    string? ActionLabel,
    string? MetadataJson
);
