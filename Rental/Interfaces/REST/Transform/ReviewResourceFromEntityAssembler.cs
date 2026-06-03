using Moveo_backend.Rental.Domain.Model.Aggregates;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class ReviewResourceFromEntityAssembler
{
    public static Resources.ReviewResource ToResourceFromEntity(Review entity)
    {
        return new Resources.ReviewResource(
            entity.Id,
            entity.RentalId,
            entity.VehicleId,
            entity.ReviewerId,
            entity.RevieweeId,
            entity.Rating,
            entity.Comment,
            entity.Type,
            entity.CreatedAt);
    }
}
