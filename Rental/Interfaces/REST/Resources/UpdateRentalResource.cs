namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record UpdateRentalResource(
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
    DateTime? AcceptedAt,
    DateTime? CompletedAt
);

public record PatchRentalResource(
    string? Status,
    bool? VehicleRated,
    int? VehicleRating,
    DateTime? AcceptedAt,
    DateTime? CompletedAt
);