namespace Moveo_backend.Support.Domain.Model.Commands;

public record UpdateSupportTicketCommand(
    int Id,
    string? Status,
    string? Priority,
    int? AssignedToId,
    string? ResolutionNotes,
    // Damage cost update
    decimal? EstimatedCost,
    // Dispute fields
    string? DisputeStatus,
    string? DisputeReason,
    string? DisputeDescription,
    int? DisputedBy,
    // Additional proof fields (owner response)
    string? AdditionalProofJson,
    string? AdditionalProofMessage,
    // Payment fields
    string? PaymentStatus
);
