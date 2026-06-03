namespace Moveo_backend.Rental.Domain.Model.Commands;

public record CreateReviewCommand(
    int RentalId,
    int? VehicleId,
    int ReviewerId,
    int RevieweeId,
    int Rating,
    string? Comment,
    string Type);
