namespace Moveo_backend.Rental.Interfaces.REST.Resources;

public record CreateReviewResource(
    int RentalId,
    int? VehicleId,
    int ReviewerId,
    int RevieweeId,
    int Rating,
    string? Comment,
    string Type);
