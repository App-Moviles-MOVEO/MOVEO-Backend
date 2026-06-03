namespace Moveo_backend.UserReview.Domain.Model.Commands;

public record CreateUserReviewCommand(
    int ReviewerId,
    int ReviewedUserId,
    int RentalId,
    int Rating,
    string? Comment,
    string Type
);
