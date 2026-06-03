namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record RentalResource(
    int Id,
    int VehicleId,
    int RenterId,
    int OwnerId,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalPrice,
    string Status,
    string? PickupLocation,
    string? ReturnLocation,
    string? Notes,
    int? AdventureRouteId,
    bool? VehicleRated,
    int? VehicleRating,
    DateTime CreatedAt,
    DateTime? AcceptedAt,
    DateTime? CompletedAt
);