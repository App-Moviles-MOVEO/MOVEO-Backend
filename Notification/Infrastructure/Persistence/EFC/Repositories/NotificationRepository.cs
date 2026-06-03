using Microsoft.EntityFrameworkCore;
using Moveo_backend.Notification.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;

namespace Moveo_backend.Notification.Infrastructure.Persistence.EFC.Repositories;

public class NotificationRepository(AppDbContext context) 
    : BaseRepository<NotificationEntity>(context), INotificationRepository
{
    public async Task<IEnumerable<NotificationEntity>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<NotificationEntity>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationEntity>> FindUnreadByUserIdAsync(int userId)
    {
        return await Context.Set<NotificationEntity>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAllAsReadByUserIdAsync(int userId)
    {
        var notifications = await Context.Set<NotificationEntity>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }
    }
}
