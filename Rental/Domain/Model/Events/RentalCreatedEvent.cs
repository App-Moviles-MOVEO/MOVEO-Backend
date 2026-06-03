namespace Moveo_backend.Rental.Domain.Model.Events;

public record RentalCreatedEvent(
    Guid RentalId,
    Guid VehicleId,
    Guid RenterId,
    Guid OwnerId,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice,
    string PickupLocation,
    string ReturnLocation);