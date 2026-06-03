using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Model.ValueObjects;
using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class UpdateVehicleCommandFromResourceAssembler
{
    public static UpdateVehicleCommand ToCommandFromResource(int id, UpdateVehicleResource resource)
    {
        return new UpdateVehicleCommand(
            id,
            resource.OwnerId,
            resource.Brand,
            resource.Model,
            resource.Year,
            resource.Color,
            resource.Transmission,
            resource.FuelType,
            resource.Seats,
            resource.LicensePlate,
            new Money(resource.DailyPrice),
            new Money(resource.DepositAmount ?? 0),
            new Location(
                resource.Location.District,
                resource.Location.Address,
                resource.Location.Lat,
                resource.Location.Lng
            ),
            resource.Status,
            resource.Description,
            resource.Features ?? new List<string>(),
            resource.Restrictions ?? new List<string>(),
            resource.Images ?? new List<string>()
        );
    }
}