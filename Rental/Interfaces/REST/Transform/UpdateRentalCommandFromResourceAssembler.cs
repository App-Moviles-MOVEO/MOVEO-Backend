using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class UpdateRentalCommandFromResourceAssembler
{
    public static UpdateRentalCommand ToCommandFromResource(int id, UpdateRentalResource resource)
    {
        return new UpdateRentalCommand(
            id,
            resource.VehicleId,
            resource.RenterId,
            resource.OwnerId,
            resource.StartDate,
            resource.EndDate,
            resource.TotalPrice,
            resource.Status,
            resource.PickupLocation,
            resource.ReturnLocation,
            resource.Notes,
            resource.AdventureRouteId,
            resource.VehicleRated,
            resource.VehicleRating,
            resource.AcceptedAt,
            resource.CompletedAt
        );
    }
    
    public static PatchRentalCommand ToPatchCommandFromResource(int id, PatchRentalResource resource)
    {
        return new PatchRentalCommand(
            id,
            resource.Status,
            resource.VehicleRated,
            resource.VehicleRating,
            resource.AcceptedAt,
            resource.CompletedAt
        );
    }
}