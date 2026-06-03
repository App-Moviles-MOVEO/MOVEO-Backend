namespace Moveo_backend.Payment.Domain.Model.Commands;

public record CreatePaymentCommand(
    int PayerId,
    int RecipientId,
    int RentalId,
    decimal Amount,
    string? Currency,
    string Method,
    string Type,
    string? Description = null,
    DateTime? DueDate = null
);
