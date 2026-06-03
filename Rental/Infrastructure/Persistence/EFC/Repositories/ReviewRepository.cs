using Microsoft.EntityFrameworkCore;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Repositories;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Moveo_backend.Rental.Infrastructure.Persistence.EFC.Repositories;

public class ReviewRepository : BaseRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Review>> FindByRentalIdAsync(int rentalId)
    {
        return await Context.Set<Review>()
            .Where(r => r.RentalId == rentalId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> FindByReviewerIdAsync(int reviewerId)
    {
        return await Context.Set<Review>()
            .Where(r => r.ReviewerId == reviewerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> FindByRevieweeIdAsync(int revieweeId)
    {
        return await Context.Set<Review>()
            .Where(r => r.RevieweeId == revieweeId)
            .ToListAsync();
    }
}
