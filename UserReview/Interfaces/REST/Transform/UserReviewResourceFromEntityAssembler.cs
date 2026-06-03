using Moveo_backend.UserReview.Interfaces.REST.Resources;
using UserReviewEntity = Moveo_backend.UserReview.Domain.Model.Aggregate.UserReview;

namespace Moveo_backend.UserReview.Interfaces.REST.Transform;

public static class UserReviewResourceFromEntityAssembler
{
    public static UserReviewResource ToResourceFromEntity(UserReviewEntity entity)
    {
        return new UserReviewResource(
            entity.Id,
            entity.ReviewerId,
            entity.ReviewedUserId,
            entity.RentalId,
            entity.Rating,
            entity.Comment,
            entity.Type,
            entity.CreatedAt
        );
    }
}
