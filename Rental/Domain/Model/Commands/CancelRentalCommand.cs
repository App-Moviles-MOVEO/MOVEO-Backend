namespace Moveo_backend.Rental.Domain.Model.Commands;

public record CancelRentalCommand(Guid Id, string? Reason);