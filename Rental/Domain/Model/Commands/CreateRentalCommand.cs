namespace Moveo_backend.Rental.Domain.Model.Commands;

public record CreateRentalCommand(
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
