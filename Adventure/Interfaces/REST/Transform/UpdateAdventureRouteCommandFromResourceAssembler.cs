using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Interfaces.REST.Resources;

namespace Moveo_backend.Adventure.Interfaces.REST.Transform;

public static class UpdateAdventureRouteCommandFromResourceAssembler
{
    public static UpdateAdventureRouteCommand ToCommandFromResource(int id, UpdateAdventureRouteResource resource)
    {
        return new UpdateAdventureRouteCommand(
            id,
            resource.Name,
            resource.Title,
            resource.Description,
            resource.StartLocation,
            resource.EndLocation,
            resource.Type,
            resource.Duration,
            resource.Difficulty,
            resource.EstimatedCost,
            resource.VehicleName,
            resource.ImageUrl,
            resource.Tags,
            resource.Featured,
            resource.MaxCapacity,
            resource.DepartureDate,
            resource.DepartureTime,
            resource.SeatsTotal,
            resource.SeatsAvailable,
            resource.PricePerSeat,
            resource.OnlyWomen,
            resource.Community,
            resource.Lat,
            resource.Lng,
            resource.Status
        );
    }
}
