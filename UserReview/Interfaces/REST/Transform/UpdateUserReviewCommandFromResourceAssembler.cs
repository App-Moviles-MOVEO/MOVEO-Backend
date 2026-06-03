using Moveo_backend.UserReview.Domain.Model.Commands;
using Moveo_backend.UserReview.Interfaces.REST.Resources;

namespace Moveo_backend.UserReview.Interfaces.REST.Transform;

public static class UpdateUserReviewCommandFromResourceAssembler
{
    public static UpdateUserReviewCommand ToCommandFromResource(int id, UpdateUserReviewResource resource)
    {
        return new UpdateUserReviewCommand(
            id,
            resource.Rating,
            resource.Comment
        );
    }
}
