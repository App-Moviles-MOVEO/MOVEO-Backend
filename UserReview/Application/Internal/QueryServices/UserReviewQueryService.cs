using Moveo_backend.UserReview.Domain.Model.Aggregate;
using Moveo_backend.UserReview.Domain.Model.Queries;
using Moveo_backend.UserReview.Domain.Repositories;
using Moveo_backend.UserReview.Domain.Services;

namespace Moveo_backend.UserReview.Application.Internal.QueryServices;

public class UserReviewQueryService(IUserReviewRepository userReviewRepository) : IUserReviewQueryService
{
    public async Task<IEnumerable<Domain.Model.Aggregate.UserReview>> Handle(GetAllUserReviewsQuery query)
    {
        return await userReviewRepository.ListAsync();
    }

    public async Task<Domain.Model.Aggregate.UserReview?> Handle(GetUserReviewByIdQuery query)
    {
        return await userReviewRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Domain.Model.Aggregate.UserReview>> Handle(GetUserReviewsByReviewedUserIdQuery query)
    {
        if (!string.IsNullOrEmpty(query.Type))
        {
            return await userReviewRepository.FindByReviewedUserIdAndTypeAsync(query.ReviewedUserId, query.Type);
        }
        return await userReviewRepository.FindByReviewedUserIdAsync(query.ReviewedUserId);
    }

    public async Task<IEnumerable<Domain.Model.Aggregate.UserReview>> Handle(GetUserReviewsByReviewerIdQuery query)
    {
        return await userReviewRepository.FindByReviewerIdAsync(query.ReviewerId);
    }

    public async Task<IEnumerable<Domain.Model.Aggregate.UserReview>> Handle(GetUserReviewsByRentalIdQuery query)
    {
        return await userReviewRepository.FindByRentalIdAsync(query.RentalId);
    }
}
