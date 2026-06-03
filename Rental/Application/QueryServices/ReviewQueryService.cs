using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Queries;
using Moveo_backend.Rental.Domain.Repositories;
using Moveo_backend.Rental.Domain.Services;

namespace Moveo_backend.Rental.Application.QueryServices;

public class ReviewQueryService : IReviewQueryService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewQueryService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IEnumerable<Review>> Handle(GetAllReviewsQuery query)
    {
        return await _reviewRepository.ListAsync();
    }

    public async Task<Review?> Handle(GetReviewByIdQuery query)
    {
        return await _reviewRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Review>> Handle(GetReviewsByRentalIdQuery query)
    {
        return await _reviewRepository.FindByRentalIdAsync(query.RentalId);
    }

    public async Task<IEnumerable<Review>> Handle(GetReviewsByReviewerIdQuery query)
    {
        return await _reviewRepository.FindByReviewerIdAsync(query.ReviewerId);
    }

    public async Task<IEnumerable<Review>> Handle(GetReviewsByRevieweeIdQuery query)
    {
        return await _reviewRepository.FindByRevieweeIdAsync(query.RevieweeId);
    }
}
