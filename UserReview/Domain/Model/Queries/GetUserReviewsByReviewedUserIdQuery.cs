namespace Moveo_backend.UserReview.Domain.Model.Queries;

public record GetUserReviewsByReviewedUserIdQuery(int ReviewedUserId, string? Type = null);
