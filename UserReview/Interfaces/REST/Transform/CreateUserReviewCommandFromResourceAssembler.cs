using Moveo_backend.UserReview.Domain.Model.Commands;
using Moveo_backend.UserReview.Interfaces.REST.Resources;

namespace Moveo_backend.UserReview.Interfaces.REST.Transform;

public static class CreateUserReviewCommandFromResourceAssembler
{
    public static CreateUserReviewCommand ToCommandFromResource(CreateUserReviewResource resource)
    {
        return new CreateUserReviewCommand(
            resource.ReviewerId,
            resource.ReviewedUserId,
            resource.RentalId,
            resource.Rating,
            resource.Comment,
            resource.Type
        );
    }
}
