namespace Moveo_backend.Payment.Interfaces.REST.Resources;

public record CreateWithdrawalResource(
    int UserId,
    decimal Amount,
    string? Method,
    string Destination
);

public record PatchWithdrawalResource(
    string Status,
    string? RejectionReason = null
);
