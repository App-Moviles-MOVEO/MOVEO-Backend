using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Commands;
using Moveo_backend.Rental.Domain.Model.Queries;

namespace Moveo_backend.Rental.Domain.Services;

public interface IReviewCommandService
{
    Task<Review?> Handle(CreateReviewCommand command);
    Task<Review?> Handle(UpdateReviewCommand command);
    Task<bool> Handle(DeleteReviewCommand command);
}
