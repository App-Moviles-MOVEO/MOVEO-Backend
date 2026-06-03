using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class VehicleResourceFromEntityAssembler
{
    public static VehicleResource ToResourceFromEntity(Vehicle vehicle)
    {
        return new VehicleResource(
            vehicle.Id,
            vehicle.OwnerId,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.Transmission,
            vehicle.FuelType,
            vehicle.Seats,
            vehicle.LicensePlate,
            new VehicleLocationResource(
                vehicle.Location.District,
                vehicle.Location.Address,
                vehicle.Location.Lat,
                vehicle.Location.Lng
            ),
            vehicle.DailyPrice.Amount,
            vehicle.DepositAmount?.Amount,
            vehicle.Status,
            vehicle.Description,
            vehicle.Images,
            vehicle.Features,
            vehicle.Restrictions,
            vehicle.CreatedAt,
            vehicle.UpdatedAt
        );
    }
}