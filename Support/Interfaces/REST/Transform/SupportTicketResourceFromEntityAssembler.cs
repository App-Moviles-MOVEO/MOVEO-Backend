using System.Text.Json;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.Support.Interfaces.REST.Resources;

namespace Moveo_backend.Support.Interfaces.REST.Transform;

public static class SupportTicketResourceFromEntityAssembler
{
    public static SupportTicketResource ToResourceFromEntity(SupportTicket entity, bool includeMessages = false)
    {
        var messages = includeMessages && entity.Messages.Any()
            ? entity.Messages.Select(TicketMessageResourceFromEntityAssembler.ToResourceFromEntity)
            : null;

        // Parse JSON arrays back to lists
        List<string>? attachments = null;
        if (!string.IsNullOrEmpty(entity.AttachmentsJson))
        {
            try { attachments = JsonSerializer.Deserialize<List<string>>(entity.AttachmentsJson); }
            catch { /* ignore parsing errors */ }
        }
        
        List<string>? additionalProof = null;
        if (!string.IsNullOrEmpty(entity.AdditionalProofJson))
        {
            try { additionalProof = JsonSerializer.Deserialize<List<string>>(entity.AdditionalProofJson); }
            catch { /* ignore parsing errors */ }
        }

        return new SupportTicketResource
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Subject = entity.Subject,
            Description = entity.Description,
            Category = entity.Category,
            Status = entity.Status,
            Priority = entity.Priority,
            Type = entity.Type,
            RelatedId = entity.RelatedId,
            RelatedType = entity.RelatedType,
            ResolutionNotes = entity.ResolutionNotes,
            AssignedToId = entity.AssignedToId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ResolvedAt = entity.ResolvedAt,
            // Damage ticket fields
            EstimatedCost = entity.EstimatedCost,
            VehicleId = entity.VehicleId,
            VehicleName = entity.VehicleName,
            RentalId = entity.RentalId,
            RenterId = entity.RenterId,
            RenterName = entity.RenterName,
            Attachments = attachments,
            // Dispute fields
            DisputeStatus = entity.DisputeStatus,
            DisputeReason = entity.DisputeReason,
            DisputeDescription = entity.DisputeDescription,
            DisputedAt = entity.DisputedAt,
            DisputedBy = entity.DisputedBy,
            // Additional proof fields
            AdditionalProof = additionalProof,
            AdditionalProofMessage = entity.AdditionalProofMessage,
            AdditionalProofSentAt = entity.AdditionalProofSentAt,
            // Payment fields
            PaymentStatus = entity.PaymentStatus,
            PaidAt = entity.PaidAt,
            // Messages
            Messages = messages
        };
    }
}
