namespace Moveo_backend.UserReview.Interfaces.REST.Resources;

public record UserReviewResource(
    int Id,
    int ReviewerId,
    int ReviewedUserId,
    int RentalId,
    int Rating,
    string Comment,
    string Type,
    DateTime CreatedAt
);
