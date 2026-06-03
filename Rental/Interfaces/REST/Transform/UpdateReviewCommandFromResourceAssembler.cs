using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Interfaces.REST.Resources;

namespace Moveo_backend.Rental.Interfaces.REST.Transform;

public static class UpdateReviewCommandFromResourceAssembler
{
    public static UpdateReviewCommand ToCommandFromResource(int id, UpdateReviewResource resource)
    {
        return new UpdateReviewCommand(
            id,
            resource.Rating,
            resource.Comment);
    }
}
