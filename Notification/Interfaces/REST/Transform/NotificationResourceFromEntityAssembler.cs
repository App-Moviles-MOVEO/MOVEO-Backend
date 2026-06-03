using System.Text.Json;
using Moveo_backend.Notification.Interfaces.REST.Resources;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;

namespace Moveo_backend.Notification.Interfaces.REST.Transform;

public static class NotificationResourceFromEntityAssembler
{
    public static NotificationResource ToResourceFromEntity(NotificationEntity entity)
    {
        object? metadata = null;
        if (!string.IsNullOrEmpty(entity.MetadataJson))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<object>(entity.MetadataJson);
            }
            catch
            {
                metadata = null;
            }
        }
        
        return new NotificationResource(
            entity.Id,
            entity.UserId,
            entity.Title,
            entity.Body,
            entity.Type,
            entity.IsRead,
            entity.RelatedEntityId,
            entity.RelatedEntityType,
            entity.ActionUrl,
            entity.ActionLabel,
            metadata,
            entity.CreatedAt,
            entity.ReadAt
        );
    }
}
