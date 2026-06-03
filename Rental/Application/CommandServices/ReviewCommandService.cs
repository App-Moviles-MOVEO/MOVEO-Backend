using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Repositories;
using Moveo_backend.Rental.Domain.Services;
using Moveo_backend.Shared.Domain.Repositories;

namespace Moveo_backend.Rental.Application.CommandServices;

public class ReviewCommandService : IReviewCommandService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewCommandService(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Review?> Handle(CreateReviewCommand command)
    {
        var review = new Review(
            command.RentalId,
            command.VehicleId,
            command.ReviewerId,
            command.RevieweeId,
            command.Rating,
            command.Comment ?? string.Empty,
            command.Type);

        await _reviewRepository.AddAsync(review);
        await _unitOfWork.CompleteAsync();
        return review;
    }

    public async Task<Review?> Handle(UpdateReviewCommand command)
    {
        var review = await _reviewRepository.FindByIdAsync(command.Id);
        if (review == null) return null;

        review.Update(command.Rating, command.Comment);
        _reviewRepository.Update(review);
        await _unitOfWork.CompleteAsync();
        return review;
    }

    public async Task<bool> Handle(DeleteReviewCommand command)
    {
        var review = await _reviewRepository.FindByIdAsync(command.Id);
        if (review == null) return false;

        _reviewRepository.Remove(review);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
