namespace Moveo_backend.Payment.Domain.Model.Commands;

public record UpdatePaymentCommand(
    int Id,
    int PayerId,
    int RecipientId,
    int RentalId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string? TransactionId,
    string Type,
    string? Description,
    string? Reason,
    DateTime? DueDate,
    DateTime? CompletedAt
);

public record PatchPaymentCommand(
    int Id,
    string? Status = null,
    string? TransactionId = null,
    string? Reason = null,
    DateTime? CompletedAt = null
);
