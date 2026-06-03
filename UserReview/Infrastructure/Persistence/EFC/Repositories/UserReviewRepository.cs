using Microsoft.EntityFrameworkCore;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Moveo_backend.UserReview.Domain.Repositories;
using UserReviewEntity = Moveo_backend.UserReview.Domain.Model.Aggregate.UserReview;

namespace Moveo_backend.UserReview.Infrastructure.Persistence.EFC.Repositories;

public class UserReviewRepository(AppDbContext context) 
    : BaseRepository<UserReviewEntity>(context), IUserReviewRepository
{
    public async Task<IEnumerable<UserReviewEntity>> FindByReviewedUserIdAsync(int reviewedUserId)
    {
        return await Context.Set<UserReviewEntity>()
            .Where(r => r.ReviewedUserId == reviewedUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserReviewEntity>> FindByReviewedUserIdAndTypeAsync(int reviewedUserId, string type)
    {
        return await Context.Set<UserReviewEntity>()
            .Where(r => r.ReviewedUserId == reviewedUserId && r.Type == type)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserReviewEntity>> FindByReviewerIdAsync(int reviewerId)
    {
        return await Context.Set<UserReviewEntity>()
            .Where(r => r.ReviewerId == reviewerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserReviewEntity>> FindByRentalIdAsync(int rentalId)
    {
        return await Context.Set<UserReviewEntity>()
            .Where(r => r.RentalId == rentalId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
