using Moveo_backend.UserManagement.Domain.Model.Events;

namespace Moveo_backend.UserManagement.Application.EventHandlers;

public class UserEventHandler
{
    public void Handle(UserCreatedEvent @event)
    {
        Console.WriteLine($"[EVENT] User created: {@event.UserId}");
    }

    public void Handle(UserDeletedEvent @event)
    {
        Console.WriteLine($"[EVENT] User deleted: {@event.UserId}");
    }

    public void Handle(UserUpdatedEvent @event)
    {
        Console.WriteLine($"[EVENT] User updated: {@event.UserId}");
    }
}