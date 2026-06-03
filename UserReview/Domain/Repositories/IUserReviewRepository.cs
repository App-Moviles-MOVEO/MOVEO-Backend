using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.UserReview.Domain.Model.Aggregate;

namespace Moveo_backend.UserReview.Domain.Repositories;

public interface IUserReviewRepository : IBaseRepository<UserReview.Domain.Model.Aggregate.UserReview>
{
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> FindByReviewedUserIdAsync(int reviewedUserId);
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> FindByReviewedUserIdAndTypeAsync(int reviewedUserId, string type);
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> FindByReviewerIdAsync(int reviewerId);
    Task<IEnumerable<UserReview.Domain.Model.Aggregate.UserReview>> FindByRentalIdAsync(int rentalId);
}
