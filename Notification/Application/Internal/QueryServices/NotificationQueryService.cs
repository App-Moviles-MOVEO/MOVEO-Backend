using Moveo_backend.Notification.Domain.Model.Queries;
using Moveo_backend.Notification.Domain.Repositories;
using Moveo_backend.Notification.Domain.Services;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;

namespace Moveo_backend.Notification.Application.Internal.QueryServices;

public class NotificationQueryService(INotificationRepository notificationRepository) : INotificationQueryService
{
    public async Task<NotificationEntity?> Handle(GetNotificationByIdQuery query)
    {
        return await notificationRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<NotificationEntity>> Handle(GetAllNotificationsQuery query)
    {
        return await notificationRepository.ListAsync();
    }

    public async Task<IEnumerable<NotificationEntity>> Handle(GetNotificationsByUserIdQuery query)
    {
        return await notificationRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<IEnumerable<NotificationEntity>> Handle(GetUnreadNotificationsByUserIdQuery query)
    {
        return await notificationRepository.FindUnreadByUserIdAsync(query.UserId);
    }
}
