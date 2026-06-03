namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record CreateRentalResource(
    int VehicleId,
    int RenterId,
    int OwnerId,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice,
    string? PickupLocation,
    string? ReturnLocation,
    string? Notes,
    int? AdventureRouteId
);