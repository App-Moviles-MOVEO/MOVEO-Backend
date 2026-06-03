using Moveo_backend.Adventure.Domain.Model.Commands;
using Moveo_backend.Adventure.Interfaces.REST.Resources;

namespace Moveo_backend.Adventure.Interfaces.REST.Transform;

public static class CreateAdventureRouteCommandFromResourceAssembler
{
    public static CreateAdventureRouteCommand ToCommandFromResource(CreateAdventureRouteResource resource)
    {
        return new CreateAdventureRouteCommand(
            resource.OwnerId,
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
            resource.MaxCapacity
        );
    }
}
