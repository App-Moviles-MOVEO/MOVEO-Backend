using Moveo_backend.Support.Domain.Model.Commands;
using Moveo_backend.Support.Interfaces.REST.Resources;

namespace Moveo_backend.Support.Interfaces.REST.Transform;

public static class CreateSupportTicketCommandFromResourceAssembler
{
    public static CreateSupportTicketCommand ToCommandFromResource(CreateSupportTicketResource resource)
    {
        return new CreateSupportTicketCommand(
            resource.UserId,
            resource.Subject,
            resource.Description,
            resource.GetCategory(),
            resource.GetPriority(),
            resource.GetType(),
            resource.RelatedId,
            resource.RelatedType,
            // Damage ticket fields
            resource.EstimatedCost,
            resource.VehicleId,
            resource.VehicleName,
            resource.RentalId,
            resource.RenterId,
            resource.RenterName,
            resource.GetAttachmentsJson()
        );
    }
}
