using Moveo_backend.Shared.Domain.Repositories;
using Moveo_backend.UserReview.Domain.Model.Aggregate;
using Moveo_backend.UserReview.Domain.Model.Commands;
using Moveo_backend.UserReview.Domain.Repositories;
using Moveo_backend.UserReview.Domain.Services;

namespace Moveo_backend.UserReview.Application.Internal.CommandServices;

public class UserReviewCommandService(
    IUserReviewRepository userReviewRepository,
    IUnitOfWork unitOfWork) : IUserReviewCommandService
{
    public async Task<Domain.Model.Aggregate.UserReview?> Handle(CreateUserReviewCommand command)
    {
        var userReview = new Domain.Model.Aggregate.UserReview(command);
        await userReviewRepository.AddAsync(userReview);
        await unitOfWork.CompleteAsync();
        return userReview;
    }

    public async Task<Domain.Model.Aggregate.UserReview?> Handle(UpdateUserReviewCommand command)
    {
        var userReview = await userReviewRepository.FindByIdAsync(command.Id);
        if (userReview is null) return null;
        
        userReview.Update(command.Rating, command.Comment);
        userReviewRepository.Update(userReview);
        await unitOfWork.CompleteAsync();
        return userReview;
    }

    public async Task<bool> Handle(DeleteUserReviewCommand command)
    {
        var userReview = await userReviewRepository.FindByIdAsync(command.Id);
        if (userReview is null) return false;
        
        userReviewRepository.Remove(userReview);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
