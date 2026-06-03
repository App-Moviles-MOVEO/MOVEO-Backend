namespace Moveo_backend.Rental.Domain.Model.Events;

public record RentalAcceptedEvent(
    Guid RentalId,
    Guid VehicleId,
    Guid OwnerId,
    DateTime AcceptedAt);