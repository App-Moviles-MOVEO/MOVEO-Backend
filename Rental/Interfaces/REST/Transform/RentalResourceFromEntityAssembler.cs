using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class RentalResourceFromEntityAssembler
{
    public static RentalResource ToResourceFromEntity(Domain.Model.Aggregates.Rental rental)
    {
        return new RentalResource(
            rental.Id,
            rental.VehicleId,
            rental.RenterId,
            rental.OwnerId,
            rental.StartDate,
            rental.EndDate,
            rental.TotalPrice,
            rental.Status,
            rental.PickupLocation,
            rental.ReturnLocation,
            rental.Notes,
            rental.AdventureRouteId,
            rental.VehicleRated,
            rental.VehicleRating,
            rental.CreatedAt,
            rental.AcceptedAt,
            rental.CompletedAt
        );
    }
}