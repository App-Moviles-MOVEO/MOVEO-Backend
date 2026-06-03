namespace Moveo_backend.Support.Domain.Model.Commands;

public record CreateSupportTicketCommand(
    int UserId,
    string Subject,
    string Description,
    string Category,
    string? Priority,
    string? Type,
    int? RelatedId,
    string? RelatedType,
    // Damage ticket fields
    decimal? EstimatedCost,
    int? VehicleId,
    string? VehicleName,
    int? RentalId,
    int? RenterId,
    string? RenterName,
    string? AttachmentsJson
);
