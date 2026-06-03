using Moveo_backend.UserReview.Domain.Model.Aggregate;
using Moveo_backend.UserReview.Domain.Model.Commands;

namespace Moveo_backend.UserReview.Domain.Services;

public interface IUserReviewCommandService
{
    Task<UserReview.Domain.Model.Aggregate.UserReview?> Handle(CreateUserReviewCommand command);
    Task<UserReview.Domain.Model.Aggregate.UserReview?> Handle(UpdateUserReviewCommand command);
    Task<bool> Handle(DeleteUserReviewCommand command);
}
