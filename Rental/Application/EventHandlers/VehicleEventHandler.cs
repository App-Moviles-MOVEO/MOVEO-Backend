using Moveo_backend.Rental.Domain.Model.Events;

namespace Moveo_backend.Rental.Application.EventHandlers;

public class VehicleEventHandler
{
    public void Handle(VehicleCreatedEvent @event)
    {
        Console.WriteLine($"[EVENT] Vehicle created: {@event.VehicleId}, Owner: {@event.OwnerId}");
    }

    public void Handle(VehicleUpdatedEvent @event)
    {
        Console.WriteLine($"[EVENT] Vehicle updated: {@event.VehicleId}");
    }

    public void Handle(VehicleDeletedEvent @event)
    {
        Console.WriteLine($"[EVENT] Vehicle deleted: {@event.VehicleId}");
    }
}