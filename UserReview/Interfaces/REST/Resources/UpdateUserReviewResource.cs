namespace Moveo_backend.UserReview.Interfaces.REST.Resources;

public record UpdateUserReviewResource(
    int? Rating,
    string? Comment
);
