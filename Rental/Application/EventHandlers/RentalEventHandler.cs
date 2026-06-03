using Moveo_backend.Rental.Domain.Model.Events;

namespace Moveo_backend.Rental.Application.EventHandlers;

public class RentalEventHandler
{
    public void Handle(RentalCreatedEvent @event)
    {
        Console.WriteLine(
            $"[EVENT] Rental created: {@event.RentalId}\n" +
            $"Vehicle: {@event.VehicleId}, Renter: {@event.RenterId}, Owner: {@event.OwnerId}\n" +
            $"From {@event.StartDate:d} to {@event.EndDate:d}, Total: {@event.TotalPrice:C}\n" +
            $"Pickup: {@event.PickupLocation}, Return: {@event.ReturnLocation}"
        );
    }

    public void Handle(RentalAcceptedEvent @event)
    {
        Console.WriteLine(
            $"[EVENT] Rental accepted: {@event.RentalId} by owner {@event.OwnerId} on {@event.AcceptedAt:g}"
        );
    }

    public void Handle(RentalCancelledEvent @event)
    {
        Console.WriteLine(
            $"[EVENT] Rental cancelled: {@event.RentalId} at {@event.CancelledAt:g}\nReason: {@event.Reason ?? "No reason provided"}"
        );
    }

    public void Handle(RentalFinishedEvent @event)
    {
        Console.WriteLine(
            $"[EVENT] Rental finished: {@event.RentalId}, Vehicle: {@event.VehicleId}, Completed at {@event.FinishedAt:g}"
        );
    }
}