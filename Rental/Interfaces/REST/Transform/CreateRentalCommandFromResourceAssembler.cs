using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class CreateRentalCommandFromResourceAssembler
{
    public static CreateRentalCommand ToCommandFromResource(CreateRentalResource resource)
    {
        return new CreateRentalCommand(
            resource.VehicleId,
            resource.RenterId,
            resource.OwnerId,
            resource.StartDate,
            resource.EndDate,
            resource.TotalPrice,
            resource.PickupLocation,
            resource.ReturnLocation,
            resource.Notes,
            resource.AdventureRouteId
        );
    }
}