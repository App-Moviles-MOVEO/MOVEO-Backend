namespace Moveo_backend.Rental.Domain.Model.Commands;

public record UpdateRentalCommand(
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
    DateTime? AcceptedAt,
    DateTime? CompletedAt
);

public record PatchRentalCommand(
    int Id,
    string? Status,
    bool? VehicleRated,
    int? VehicleRating,
    DateTime? AcceptedAt,
    DateTime? CompletedAt
);

public record DeleteRentalCommand(int Id);