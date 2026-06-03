using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class CreateReviewCommandFromResourceAssembler
{
    public static CreateReviewCommand ToCommandFromResource(CreateReviewResource resource)
    {
        return new CreateReviewCommand(
            resource.RentalId,
            resource.VehicleId,
            resource.ReviewerId,
            resource.RevieweeId,
            resource.Rating,
            resource.Comment,
            resource.Type);
    }
}
