namespace Moveo_backend.Rental.Domain.Model.Events;

public record RentalFinishedEvent(
    Guid RentalId,
    Guid VehicleId,
    DateTime FinishedAt);