namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record ReviewResource(
    int Id,
    int RentalId,
    int? VehicleId,
    int ReviewerId,
    int RevieweeId,
    int Rating,
    string Comment,
    string Type,
    DateTime CreatedAt);
