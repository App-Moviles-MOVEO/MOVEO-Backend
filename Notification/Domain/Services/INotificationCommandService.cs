using Moveo_backend.Notification.Domain.Model.Commands;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;

namespace Moveo_backend.Notification.Domain.Services;

public interface INotificationCommandService
{
    Task<NotificationEntity?> Handle(CreateNotificationCommand command);
    Task<NotificationEntity?> Handle(MarkNotificationAsReadCommand command);
    Task<bool> Handle(MarkAllNotificationsAsReadCommand command);
    Task<bool> Handle(DeleteNotificationCommand command);
}
