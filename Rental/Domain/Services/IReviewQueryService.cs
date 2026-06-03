using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.Queries;

namespace Moveo_backend.Rental.Domain.Services;

public interface IReviewQueryService
{
    Task<IEnumerable<Review>> Handle(GetAllReviewsQuery query);
    Task<Review?> Handle(GetReviewByIdQuery query);
    Task<IEnumerable<Review>> Handle(GetReviewsByRentalIdQuery query);
    Task<IEnumerable<Review>> Handle(GetReviewsByReviewerIdQuery query);
    Task<IEnumerable<Review>> Handle(GetReviewsByRevieweeIdQuery query);
}
