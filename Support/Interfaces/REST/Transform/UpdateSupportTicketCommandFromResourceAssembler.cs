using Moveo_backend.Support.Domain.Model.Commands;
using Moveo_backend.Support.Interfaces.REST.Resources;

namespace Moveo_backend.Support.Interfaces.REST.Transform;

public static class UpdateSupportTicketCommandFromResourceAssembler
{
    public static UpdateSupportTicketCommand ToCommandFromResource(int id, UpdateSupportTicketResource resource)
    {
        return new UpdateSupportTicketCommand(
            id,
            resource.Status,
            resource.Priority,
            resource.AssignedToId,
            resource.ResolutionNotes,
            // Damage cost update
            resource.EstimatedCost,
            // Dispute fields
            resource.DisputeStatus,
            resource.DisputeReason,
            resource.DisputeDescription,
            resource.DisputedBy,
            // Additional proof fields
            resource.GetAdditionalProofJson(),
            resource.AdditionalProofMessage,
            // Payment fields
            resource.PaymentStatus
        );
    }
}
