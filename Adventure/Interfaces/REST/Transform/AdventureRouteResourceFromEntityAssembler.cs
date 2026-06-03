using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Adventure.Interfaces.REST.Resources;

namespace Moveo_backend.Adventure.Interfaces.REST.Transform;

public static class AdventureRouteResourceFromEntityAssembler
{
    public static AdventureRouteResource ToResourceFromEntity(AdventureRoute entity)
    {
        return new AdventureRouteResource(
            entity.Id,
            entity.OwnerId,
            entity.Name,
            entity.Title,
            entity.Description,
            entity.StartLocation,
            entity.EndLocation,
            entity.Type,
            entity.Duration,
            entity.Difficulty,
            entity.EstimatedCost,
            entity.VehicleName,
            entity.ImageUrl,
            entity.Tags,
            entity.Featured,
            entity.MaxCapacity,
            entity.Rating,
            entity.ReviewsCount,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
