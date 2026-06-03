namespace Moveo_backend.Payment.Domain.Model.Queries;

public record GetFilteredPaymentsQuery(
    int? PayerId = null,
    int? RecipientId = null,
    int? RentalId = null,
    string? Status = null,
    string? Type = null
);
