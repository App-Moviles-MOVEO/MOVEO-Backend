using Cortex.Mediator.Notifications;
using Moveo_backend.Shared.Domain.Events;

namespace Moveo_backend.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
    
}