namespace Moveo_backend.UserReview.Interfaces.REST.Resources;

public record CreateUserReviewResource(
    int ReviewerId,
    int ReviewedUserId,
    int RentalId,
    int Rating,
    string? Comment,
    string Type
);
