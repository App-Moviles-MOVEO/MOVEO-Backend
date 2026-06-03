using Moveo_backend.UserReview.Domain.Model.Aggregate;
using Moveo_backend.UserReview.Domain.Model.Queries;

namespace Moveo_backend.UserReview.Domain.Services;

public interface IUserReviewQueryService
{
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> Handle(GetAllUserReviewsQuery query);
    Task<UserReview.Domain.Model.Aggregate.UserReview?> Handle(GetUserReviewByIdQuery query);
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> Handle(GetUserReviewsByReviewedUserIdQuery query);
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> Handle(GetUserReviewsByReviewerIdQuery query);
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> Handle(GetUserReviewsByRentalIdQuery query);
}
