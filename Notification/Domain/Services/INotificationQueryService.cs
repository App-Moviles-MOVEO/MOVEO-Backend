using Moveo_backend.Notification.Domain.Model.Queries;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;

namespace Moveo_backend.Notification.Domain.Services;

public interface INotificationQueryService
{
    Task<NotificationEntity?> Handle(GetNotificationByIdQuery query);
    Task<IEnumerable<NotificationEntity>> Handle(GetAllNotificationsQuery query);
    Task<IEnumerable<NotificationEntity>> Handle(GetNotificationsByUserIdQuery query);
    Task<IEnumerable<NotificationEntity>> Handle(GetUnreadNotificationsByUserIdQuery query);
}
