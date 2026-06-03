using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Interfaces.REST.Resources;

namespace Moveo_backend.Notification.Interfaces.REST.Transform;

public static class CreateNotificationCommandFromResourceAssembler
{
    public static CreateNotificationCommand ToCommandFromResource(CreateNotificationResource resource)
    {
        return new CreateNotificationCommand(
            resource.UserId,
            resource.Title,
            resource.GetBody(),
            resource.Type,
            resource.GetRelatedEntityId(),
            resource.GetRelatedEntityType(),
            resource.ActionUrl,
            resource.ActionLabel,
            resource.GetMetadataJson()
        );
    }
}
