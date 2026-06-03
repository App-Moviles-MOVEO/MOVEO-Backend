namespace Moveo_backend.Rental.Domain.Model.Events;

public record VehicleUpdatedEvent(
    Guid VehicleId,
    string Brand,
    string Model,
    int Year,
    string Color,
    string Status);