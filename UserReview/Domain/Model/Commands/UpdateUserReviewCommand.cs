namespace Moveo_backend.UserReview.Domain.Model.Commands;

public record UpdateUserReviewCommand(
    int Id,
    int? Rating,
    string? Comment
);
