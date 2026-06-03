namespace Moveo_backend.Rental.Domain.Model.Events;

public record RentalCancelledEvent(
    Guid RentalId,
    string? Reason,
    DateTime CancelledAt);