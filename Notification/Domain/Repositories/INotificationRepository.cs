using Moveo_backend.Shared.Domain.Repositories;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;

namespace Moveo_backend.Notification.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<NotificationEntity>
{
    Task<IEnumerable<NotificationEntity>> FindByUserIdAsync(int userId);
    Task<IEnumerable<NotificationEntity>> FindUnreadByUserIdAsync(int userId);
    Task MarkAllAsReadByUserIdAsync(int userId);
}
