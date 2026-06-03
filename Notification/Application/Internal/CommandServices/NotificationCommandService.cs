using Moveo_backend.Notification.Domain.Model.Commands;
using Moveo_backend.Notification.Domain.Repositories;
using Moveo_backend.Notification.Domain.Services;
using Moveo_backend.Shared.Domain.Repositories;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;

namespace Moveo_backend.Notification.Application.Internal.CommandServices;

public class NotificationCommandService(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork) : INotificationCommandService
{
    public async Task<NotificationEntity?> Handle(CreateNotificationCommand command)
    {
        var notification = new NotificationEntity(command);
        await notificationRepository.AddAsync(notification);
        await unitOfWork.CompleteAsync();
        return notification;
    }

    public async Task<NotificationEntity?> Handle(MarkNotificationAsReadCommand command)
    {
        var notification = await notificationRepository.FindByIdAsync(command.Id);
        if (notification is null) return null;

        notification.MarkAsRead();
        notificationRepository.Update(notification);
        await unitOfWork.CompleteAsync();
        return notification;
    }

    public async Task<bool> Handle(MarkAllNotificationsAsReadCommand command)
    {
        await notificationRepository.MarkAllAsReadByUserIdAsync(command.UserId);
        await unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> Handle(DeleteNotificationCommand command)
    {
        var notification = await notificationRepository.FindByIdAsync(command.Id);
        if (notification is null) return false;

        notificationRepository.Remove(notification);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
